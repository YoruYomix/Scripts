using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniLifetimeScope : IDisposable
    {
        private readonly IReadOnlyList<BootRule> _rules;
        private readonly ChoMiniOptions _options;
        private readonly Dictionary<(Type installerType, object? key), object> _bindings = new();
        private ChoMiniComposer _composer;

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
            IReadOnlyList<BootRule> rules,
            ChoMiniOptions options)
        {
            _rules = rules;
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

    }

}