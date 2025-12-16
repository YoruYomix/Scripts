using System.Collections.Generic;
using System;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniComposer
    {
        private readonly ChoMiniLifetimeScope _scope;
        private bool _isComposed;

        public ChoMiniComposer(ChoMiniLifetimeScope scope)
        {
            _scope = scope;
        }

        public void EnsureComposed()
        {
            if (_isComposed)
                return;

            Debug.Log("[Composer] Compose start");

            DebugResolveInstallersOnly();
            DebugResolveFactoryOnly();
            DebugResolveProvidersOnly();
            _isComposed = true;
        }

        private void DebugResolveFactoryOnly()
        {
            Debug.Log("[Composer] Factory Rules:");

            foreach (var r in _scope.FactoryRules)
                Debug.Log($"  {r.Kind} / {r.ImplType.Name}");

            var selected = RuleSelect.SelectOne(
                _scope.FactoryRules,
                _scope.Options);

            Debug.Log($"[Composer] Selected Factory = {selected?.ImplType.Name}");
        }

        private void DebugResolveProvidersOnly()
        {
            Debug.Log("[Composer] Resolve Providers Start");

            // -------------------------------------------------
            // 1) Category (= IChoMiniXXXProvider) 기준으로 그룹핑
            // -------------------------------------------------
            Dictionary<Type, List<BootRule>> grouped = new();

            foreach (var rule in _scope.ProviderRules)
            {
                Type groupKey = rule.Category;

                if (!grouped.TryGetValue(groupKey, out var list))
                {
                    list = new List<BootRule>();
                    grouped[groupKey] = list;
                }

                list.Add(rule);
            }

            // -------------------------------------------------
            // 2) 각 Provider 그룹에서 하나 선택
            // -------------------------------------------------
            foreach (var pair in grouped)
            {
                Type providerInterface = pair.Key;
                List<BootRule> rules = pair.Value;

                Debug.Log($"[Composer] Provider Group: {providerInterface.Name}");

                foreach (var r in rules)
                {
                    Debug.Log(
                        $"  {r.Kind} / Key={r.Key ?? "default"} / Impl={r.ImplType.Name}"
                    );
                }

                // 공통 셀렉트 로직 사용
                BootRule selected = RuleSelect.SelectOne(
                    rules,
                    _scope.Options
                );

                Debug.Log(
                    selected != null
                        ? $"[Composer] Selected Provider = {selected.ImplType.Name}"
                        : $"[Composer] Selected Provider = <none>"
                );
            }

            Debug.Log("[Composer] Resolve Providers End");
        }

        private void DebugResolveInstallersOnly()
        {
            Debug.Log("[Composer] Resolve Installers Start");

            foreach (var rule in _scope.InstallerRules)
            {
                string key;

                if (rule.Key == null)
                {
                    key = "default";
                }
                else
                {
                    key = rule.Key.ToString();
                }

                Debug.Log(
                    $"[Composer] Installer Rule: {rule.Category.Name} / {rule.Kind} / Key={key}"
                );
            }

            Debug.Log("[Composer] Resolve Installers End");
        }



        internal static class RuleSelect
        {
            public static BootRule SelectOne(
                IEnumerable<BootRule> rules,
                ChoMiniOptions options)
            {
                // Override 우선
                foreach (var r in rules)
                {
                    if (r.Kind == RuleKind.Override &&
                        options.Has(r.Key))
                        return r;
                }

                // Base fallback
                foreach (var r in rules)
                {
                    if (r.Kind == RuleKind.Base)
                        return r;
                }

                return null;
            }
        }


    }

}
