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
            DebugBuildPayloadOnly();
        }
        public void DebugBuildPayloadOnly()
        {
            Debug.Log("[Scope] DebugBuildPayloadOnly");

            var payload = BuildPayloadFromInstallers();

            Debug.Log("[Scope] Payload Steps = " + payload.Count);
        }

        public List<List<object>> BuildPayloadFromInstallers()
        {
            Debug.Log("[Scope] BuildPayloadFromInstallers Start");

            var merged = new List<List<object>>();

            foreach (var rule in InstallerRules)
            {
                // Installer 타입 (예: ChoMiniGameObjectInstaller)
                Type installerType = rule.Category;

                // 옵션 키로 resolve할지, default로 resolve할지 결정
                object keyToUse = null;

                if (rule.Kind == RuleKind.Override)
                {
                    if (Options.Has(rule.Key))
                    {
                        keyToUse = rule.Key;
                    }
                    else
                    {
                        continue; // 이 override는 이번 옵션에 해당 없음
                    }
                }

                // 1) Installer가 사용할 리소스 resolve (이미 Bind되어 있어야 함)
                object resource = Resolve(installerType, keyToUse);

                Debug.Log($"[Scope] Installer Resource: {installerType.Name} / Key={(keyToUse ?? "default")} / {resource.GetType().Name}");

                // 2) Installer 인스턴스 생성
                IChoMiniInstaller installer = (IChoMiniInstaller)Activator.CreateInstance(installerType);

                // 3) 리소스 바인딩 (여기서 Install 타입별로 처리)
                BindResourceToInstaller(installer, resource);

                // 4) payload 생성
                List<List<object>> payload = installer.BuildPayload(this, Options);

                Debug.Log($"[Scope] Installer Payload Steps = {payload.Count}");

                // 5) merge
                foreach (var step in payload)
                {
                    merged.Add(step);
                }
            }

            Debug.Log("[Scope] BuildPayloadFromInstallers End");
            Debug.Log("[Scope] Merged Steps = " + merged.Count);

            return merged;
        }

        private object Resolve(Type installerType, object key)
        {
            object obj;

            // override 먼저
            if (key != null)
            {
                if (_bindings.TryGetValue((installerType, key), out obj))
                    return obj;
            }

            // default fallback
            if (_bindings.TryGetValue((installerType, null), out obj))
                return obj;

            throw new KeyNotFoundException(
                "Binding not found: " + installerType.Name + " / " + (key ?? "default")
            );
        }

        private void BindResourceToInstaller(IChoMiniInstaller installer, object resource)
        {
            // GameObjectInstaller
            if (installer is ChoMiniGameObjectInstaller goInstaller)
            {
                GameObject root = resource as GameObject;
                if (root == null)
                    throw new InvalidOperationException("GameObjectInstaller requires GameObject");

                goInstaller.Bind(root);
                return;
            }

            // StringInstaller
            if (installer is ChoMiniStringInstaller strInstaller)
            {
                string[] lines = resource as string[];
                if (lines == null)
                    throw new InvalidOperationException("StringInstaller requires string[]");

                strInstaller.Bind(lines);
                return;
            }

            throw new InvalidOperationException(
                "Unknown installer type: " + installer.GetType().Name
            );
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