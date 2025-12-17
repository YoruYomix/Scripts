using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
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
            ChoMiniLocalMessageContext localMsg)
        {
            _installerRules = installerRules;
            _factoryRules = factoryRules;
            _providerRules = providerRules;
            _options = options;
            _glovalMsg = choMiniCommand;
            _localMsg = localMsg;
        }

        public void Play()
        {
            Debug.Log("[Scope] Play()");
            IChoMiniFactory factory = BuildFactory(_localMsg);

        }

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

            // 4) Payload 조립 (정식 메서드!)
            List<NodeSource> nodeSource =
                BuildComposedNodeSources();

            // 5) Factory Initialize
            factory.Initialize(
                nodeSource,
                providers,
                localMsg.SkipSubscriber
            );

            return factory;
        }


        public TResource Resolve<TInstaller, TResource>(object? key)
        {
            // 1) override 먼저 시도
            if (key != null &&
                _bindings.TryGetValue((typeof(TInstaller), key), out var obj))
            {
                return Cast<TInstaller, TResource>(obj, key);
            }

            // 2) default(null) fallback
            if (_bindings.TryGetValue((typeof(TInstaller), null), out obj))
            {
                return Cast<TInstaller, TResource>(obj, null);
            }

            // 3) 아무 것도 없으면 실패
            throw new KeyNotFoundException(
                $"Binding not found: {typeof(TInstaller).Name} / {key ?? "default"}");
        }

        private static TResource Cast<TInstaller, TResource>(object obj, object key)
        {
            if (obj is not TResource cast)
                throw new InvalidCastException(
                    $"Binding type mismatch: {typeof(TInstaller).Name} / {key ?? "default"} " +
                    $"expected {typeof(TResource).Name}, got {obj.GetType().Name}");

            return cast;
        }

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

            // -----------------------------------
            // 3) 결과 출력
            // -----------------------------------
            Debug.Log($"[Debug] Composed Steps = {composed.Count}");

            for (int i = 0; i < composed.Count; i++)
            {
                NodeSource source = composed[i];

                Debug.Log($" Step {i}:");

                foreach (object obj in source.Items)
                {
                    Debug.Log($"   - {obj} ({obj.GetType().Name})");
                }
            }

            return composed;
        }


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
            // 4) Bind 호출
            // -----------------------------------
            System.Reflection.MethodInfo bindMethod =
                installerType.GetMethod("Bind");

            bindMethod.Invoke(installer, new[] { resource });

            // -----------------------------------
            // 5) NodeSource 생성
            // -----------------------------------
            return installer.BuildNodeSources(this, _options);
        }


        private object ResolveByType(Type installerType, object key)
        {
            if (_bindings.TryGetValue((installerType, key), out object obj))
                return obj;

            if (_bindings.TryGetValue((installerType, null), out obj))
                return obj;

            throw new KeyNotFoundException();
        }

        // 인스톨러들이 들고온 노드소스를 머징
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
            // 나중에 Provider / Factory 정리
        }

        public void DebugPrint()
        {
            Debug.Log("[ChoMiniLifetimeScope]");

            // -------------------------
            // Options
            // -------------------------
            Debug.Log("Options:");
            foreach (var pair in _options.DebugPairs())
            {
                Debug.Log($"  {pair.Key} = {pair.Value}");
            }

            // -------------------------
            // Bindings
            // -------------------------
            Debug.Log("Bindings:");

            foreach (var kv in _bindings)
            {
                var installerType = kv.Key.installerType.Name;
                var key = kv.Key.key ?? "default";
                var resourceType = kv.Value.GetType().Name;

                Debug.Log($"  {installerType} / {key} -> {resourceType}");
            }
        }

        public void DebugPrintInstallers()
        {
            Debug.Log("[Scope] Installer Bindings:");

            foreach (var kv in _bindings)
            {
                Type installerType = kv.Key.installerType;
                object key = kv.Key.key ?? "default";
                object resource = kv.Value;

                Debug.Log(
                    $"  {installerType.Name} / Key={key} -> {resource.GetType().Name}"
                );
            }
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