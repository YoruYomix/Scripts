using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniLifetimeScope : IDisposable
    {
        private readonly IReadOnlyList<BootRule> _installerRules;
        private readonly IReadOnlyList<BootRule> _factoryRules;
        private readonly IReadOnlyList<BootRule> _providerRules;
        private readonly ChoMiniOptions _options;
        private readonly Dictionary<(Type installerType, object? key), object> _bindings = new();
        private ChoMiniComposer _composer;
        private readonly ChoMiniCommandContext _glovalMsg;
        readonly ChoMiniLocalMessageContext _localMsg;
        ChoMiniOrchestrator _orchestrator;

        public IReadOnlyList<BootRule> InstallerRules => _installerRules;
        public IReadOnlyList<BootRule> FactoryRules => _factoryRules;
        public IReadOnlyList<BootRule> ProviderRules => _providerRules;
        public ChoMiniOptions Options => _options;

        private ChoMiniComposer Composer
        {
            get
            {
                if (_composer == null)
                    _composer = new ChoMiniComposer(this);
                return _composer;
            }
        }
        public ChoMiniLifetimeScope(
            IReadOnlyList<BootRule> installerRules,
            IReadOnlyList<BootRule> factoryRules,
            IReadOnlyList<BootRule> providerRules,
            ChoMiniOptions options,
            ChoMiniCommandContext choMiniCommand,
            ChoMiniLocalMessageContext localMsg,
            ChoMiniOrchestrator orchestrator)
        {
            _installerRules = installerRules;
            _factoryRules = factoryRules;
            _providerRules = providerRules;
            _options = options;
            _glovalMsg = choMiniCommand;
            _localMsg = localMsg;
            _orchestrator = orchestrator;
        }

        // ================================
        // 재생 제어
        // ================================
        public async Task Play()
        {
            Debug.Log("[Scope] Play()");
            IChoMiniFactory factory = BuildFactory(_localMsg);
            _orchestrator.Initialize(
                factory: factory,
                localMessageContext: _localMsg,
                OnComplate: null
                );
            await _orchestrator.PlaySequence();
        }

        // ==========================================================
        // 외부 DSL UX 엔트리:
        // Installer 타입 + 옵션 키 → 리소스 매핑
        // (인스턴스 생성이나 실행은 여기서 하지 않음)
        // ==========================================================
        public ChoMiniLifetimeScope Bind<TInstaller>(object resource)
            => Bind<TInstaller>(null, resource);

        public ChoMiniLifetimeScope Bind<TInstaller>(object key, object resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var k = (typeof(TInstaller),key);

            if (_bindings.ContainsKey(k))
                throw new InvalidOperationException($"Binding already exists: {typeof(TInstaller).Name} / {key}");

            _bindings.Add(k, resource);
            return this;
        }


        // ================================
        // Factory 합성
        // ================================

        // 옵션에 맞는 프로바이더가 주입된 팩토리를 컴포저에게서 가져옴
        public IChoMiniFactory BuildFactory(ChoMiniLocalMessageContext localMsg)
        {
            // 1) Composer 보장
            Composer.EnsureComposed();

            if (Composer.SelectedFactoryType == null)
                throw new InvalidOperationException("Factory not selected");


            // 2) Factory 생성
            IChoMiniFactory factory =
                (IChoMiniFactory)Activator.CreateInstance(Composer.SelectedFactoryType);

            // 3) Provider 생성
            List<ChoMiniProvider> providers = new();

            foreach (Type providerType in Composer.SelectedProviderTypes)
            {
                ChoMiniProvider provider =
                    (ChoMiniProvider)Activator.CreateInstance(providerType);
                providers.Add(provider);
            }

            // 4) Installer 결과를 병합한 NodeSource 생성
            List<NodeSource> nodeSource = BuildComposedNodeSources();

            // 5) Factory Initialize
            factory.Initialize(
                nodeSource,
                providers,
                localMsg.SkipSubscriber
            );

            return factory;
        }

        // 옵션에 맞게 선별된 인스톨러들의 NodeSource를 수집하고 step 기준으로 병합
        public List<NodeSource> BuildComposedNodeSources()
        {
            Debug.Log("[Debug] Build Composed NodeSources By Options");

            // -----------------------------------
            // 1) 모든 Installer의 NodeSource 시퀀스 수집
            // -----------------------------------
            List<List<NodeSource>> allSequences =
                new List<List<NodeSource>>();

            // Installer 타입 수집
            HashSet<Type> installerTypes = new HashSet<Type>();

            foreach (var kv in _bindings)
            {
                installerTypes.Add(kv.Key.installerType);
            }

            // 각 Installer별 NodeSource 시퀀스 생성
            foreach (Type installerType in installerTypes)
            {
                List<NodeSource> sequence =
                    BuildSingleInstallerNodeSources(installerType);

                if (sequence != null && sequence.Count > 0)
                    allSequences.Add(sequence);
            }

            // -----------------------------------
            // 2) step 기준으로 머징
            // -----------------------------------
            List<NodeSource> composed =
                ComposeNodeSources(allSequences);

            return composed;
        }

        // 옵션에 맞는 리소스를 사용해 단일 Installer의 NodeSource를 생성

        private List<NodeSource> BuildSingleInstallerNodeSources(
            Type installerType)
        {
            // -----------------------------------
            // 1) 옵션 + 바인딩으로 key 선택
            // -----------------------------------
            object key = null;

            foreach (KeyValuePair<Type, object> pair in _options.DebugPairs())
            {
                object optionValue = pair.Value;

                if (_bindings.ContainsKey((installerType, optionValue)))
                {
                    key = optionValue;
                    break;
                }
            }

            Debug.Log(
                $"[Debug] Installer={installerType.Name}, key={key ?? "default"}"
            );

            // -----------------------------------
            // 2) raw resource resolve
            // -----------------------------------
            object resource;

            try
            {
                resource = ResolveByType(installerType, key);
            }
            catch
            {
                return null;
            }

            // -----------------------------------
            // 3) Installer 인스턴스 생성
            // -----------------------------------
            IChoMiniInstaller installer =
                (IChoMiniInstaller)Activator.CreateInstance(installerType);

            // -----------------------------------
            // 4) Installer에 resource 주입
            // -----------------------------------
            System.Reflection.MethodInfo bindMethod =
                installerType.GetMethod("Bind");

            bindMethod.Invoke(installer, new[] { resource });

            // -----------------------------------
            // 5) NodeSource 생성
            // -----------------------------------
            return installer.BuildNodeSources(this, _options);
        }

        // key 우선 → default fallback 규칙으로 바인딩 리소스 선택
        private object ResolveByType(Type installerType, object key)
        {
            if (_bindings.TryGetValue((installerType, key), out object obj))
                return obj;

            if (_bindings.TryGetValue((installerType, null), out obj))
                return obj;

            throw new KeyNotFoundException();
        }

        // 복수의 선별된 인스톨러들이 들고온 노드소스를 하나로 머징
        private List<NodeSource> ComposeNodeSources(
           List<List<NodeSource>> sequences)
        {
            List<NodeSource> result = new List<NodeSource>();

            int maxSteps = 0;

            // ---------------------------------
            // 1) 최대 step 수 계산
            // ---------------------------------
            foreach (List<NodeSource> seq in sequences)
            {
                if (seq.Count > maxSteps)
                    maxSteps = seq.Count;
            }

            // ---------------------------------
            // 2) step 기준으로 머징
            // ---------------------------------
            for (int stepIndex = 0; stepIndex < maxSteps; stepIndex++)
            {
                List<object> mergedItems = new List<object>();

                foreach (List<NodeSource> seq in sequences)
                {
                    if (stepIndex < seq.Count)
                    {
                        mergedItems.AddRange(seq[stepIndex].Items);
                    }
                }

                if (mergedItems.Count > 0)
                {
                    result.Add(new NodeSource(mergedItems));
                }
            }

            return result;
        }




        public void Dispose()
        {
            // TODO: Provider / Factory / 컴포저 라이프사이클 클린업
            _composer?.Dispose();
        }


    }

    public readonly struct NodeSource
    {
        public readonly IReadOnlyList<object> Items;

        public NodeSource(IReadOnlyList<object> items)
        {
            Items = items;
        }
    }


}