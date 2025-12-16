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
            ChoMiniOptions options)
        {
            _installerRules = installerRules;
            _factoryRules = factoryRules;
            _providerRules = providerRules;
            _options = options;
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
        public void Play()
        {
            Debug.Log("[Scope] Play()");
            Composer.EnsureComposed();
            List<List<object>> composed = DebugBuildComposedPayload();
        }
        public List<List<object>> DebugBuildComposedPayload()
        {
            Debug.Log("[Debug] Build Composed Payload By Options");

            // -----------------------------------
            // 1) 모든 인스톨러 payload 수집
            // -----------------------------------
            List<List<List<object>>> allPayloads =
                new List<List<List<object>>>();

            // Installer 타입들 순회
            HashSet<Type> installerTypes = new HashSet<Type>();

            foreach (var kv in _bindings)
            {
                installerTypes.Add(kv.Key.installerType);
            }

            foreach (Type installerType in installerTypes)
            {
                List<List<object>> payload =
                    DebugBuildSingleInstallerPayload(installerType);

                if (payload != null && payload.Count > 0)
                    allPayloads.Add(payload);
            }

            // -----------------------------------
            // 2) step 기준으로 함침
            // -----------------------------------
            List<List<object>> composed =
                ComposePayloads(allPayloads);

            // -----------------------------------
            // 3) 결과 출력
            // -----------------------------------
            Debug.Log($"[Debug] Composed Steps = {composed.Count}");

            for (int i = 0; i < composed.Count; i++)
            {
                Debug.Log($" Step {i}:");

                foreach (object obj in composed[i])
                {
                    Debug.Log($"   - {obj} ({obj.GetType().Name})");
                }
            }

            return composed;
        }

        private List<List<object>> DebugBuildSingleInstallerPayload(Type installerType)
        {
            // -----------------------------------
            // 1) 옵션 + 바인딩으로 key 선택
            // -----------------------------------
            object key = null;

            foreach (var pair in _options.DebugPairs())
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
            var bindMethod = installerType.GetMethod("Bind");
            bindMethod.Invoke(installer, new[] { resource });

            // -----------------------------------
            // 5) Payload 생성
            // -----------------------------------
            return installer.BuildPayload(this, _options);
        }

        private object ResolveByType(Type installerType, object key)
        {
            if (_bindings.TryGetValue((installerType, key), out object obj))
                return obj;

            if (_bindings.TryGetValue((installerType, null), out obj))
                return obj;

            throw new KeyNotFoundException();
        }


        private List<List<object>> ComposePayloads(
            List<List<List<object>>> payloads)
        {
            List<List<object>> result = new List<List<object>>();

            int maxSteps = 0;

            foreach (var payload in payloads)
            {
                if (payload.Count > maxSteps)
                    maxSteps = payload.Count;
            }

            for (int i = 0; i < maxSteps; i++)
            {
                List<object> step = new List<object>();

                foreach (var payload in payloads)
                {
                    if (i < payload.Count)
                        step.AddRange(payload[i]);
                }

                if (step.Count > 0)
                    result.Add(step);
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

}