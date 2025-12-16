using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Yoru.ChoMiniEngine;

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

        DebugResolveFactoryOnly();
        DebugResolveProvidersOnly();
        _isComposed = true;
    }

    private void DebugResolveFactoryOnly()
    {
        Debug.Log("[Composer] Factory Rules:");

        foreach (var rule in _scope.FactoryRules)
        {
            Debug.Log(
                $"  {rule.Kind} / Key={rule.Key ?? "default"} / Impl={rule.ImplType?.Name}"
            );
        }

        BootRule selected = null;

        // 1) Override 우선
        foreach (var rule in _scope.FactoryRules)
        {
            if (rule.Kind == RuleKind.Override &&
                _scope.Options.Has(rule.Key))
            {
                selected = rule;
                break;
            }
        }

        // 2) Base fallback
        if (selected == null)
        {
            foreach (var rule in _scope.FactoryRules)
            {
                if (rule.Kind == RuleKind.Base)
                {
                    selected = rule;
                    break;
                }
            }
        }

        Debug.Log($"[Composer] Selected Factory = {selected?.ImplType?.Name}");
    }

    private void DebugResolveProvidersOnly()
    {
        Debug.Log("[Composer] Resolve Providers Start");

        // -------------------------------------------------
        // 1) Provider Rule만 추출 (ImplType 기준)
        // -------------------------------------------------
        List<BootRule> providerRules = new();

        foreach (var rule in _scope.ProviderRules)
        {
            if (typeof(ChoMiniProvider).IsAssignableFrom(rule.ImplType))
            {
                providerRules.Add(rule);
            }
        }

        // -------------------------------------------------
        // 2) Category(= IChoMiniXXXProvider) 기준으로 그룹핑
        // -------------------------------------------------
        Dictionary<Type, List<BootRule>> grouped = new();

        foreach (var rule in providerRules)
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
        // 3) 각 Provider 그룹에서 하나 선택
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

            BootRule selected = null;

            // Override 우선
            foreach (var r in rules)
            {
                if (r.Kind == RuleKind.Override && _scope.Options.Has(r.Key))
                {
                    selected = r;
                    break;
                }
            }

            // Base fallback
            if (selected == null)
                selected = rules.Find(r => r.Kind == RuleKind.Base);

            Debug.Log(
                selected != null
                    ? $"[Composer] Selected Provider = {selected.ImplType.Name}"
                    : $"[Composer] Selected Provider = <none>"
            );
        }

        Debug.Log("[Composer] Resolve Providers End");
    }

}
