using System.Collections.Generic;
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
        _isComposed = true;
    }
    private void DebugResolveFactoryOnly()
    {
        List<BootRule> factoryRules = new List<BootRule>();

        // 스코프의 BootRule(팩토리/프로바이더/인스톨러에 대한 선언 룰 목록)에서 팩토리만 받아온다
        foreach (BootRule scopeRule in _scope.Rules)
        {
            if (scopeRule.Category == typeof(IChoMiniFactory))
            {
                factoryRules.Add(scopeRule);
            }
        }
        Debug.Log("[Composer] Factory Rules:");

        // 받아온 팩토리 룰들을 전부 출력한다
        foreach (BootRule factoryRule in factoryRules)
        {
            Debug.Log(
                $"  {factoryRule.Kind} / Key={factoryRule.Key ?? "default"} / Impl={factoryRule.ImplType?.Name}"
            );
        }
        // 

        BootRule selectedFactoryRule = null;
        // 받아온 팩토리 룰 들에서 오버라이드 먼저 스코프 옵션이 가지고 있는 키를 조회해 찾아 매칭한다
        foreach (BootRule factoryRule in factoryRules)
        {
            if (factoryRule.Kind != RuleKind.Override)
                continue;
            // 여기서 스코프에 등록된 옵션키를 조회
            if (!_scope.Options.Has(factoryRule.Key))
                continue;

            selectedFactoryRule = factoryRule;
            break; // FirstOrDefault니까 첫 개에서 종료
        }

        // 오버라이드 매칭결과가 없으면 디폴트를 매칭한다
        if (selectedFactoryRule == null)
        {
            foreach (BootRule factoryRule in factoryRules)
            {
                if (factoryRule.Kind == RuleKind.Base)
                {
                    selectedFactoryRule = factoryRule;
                    break;
                }
            }
        }

        Debug.Log($"[Composer] Selected Factory = {selectedFactoryRule?.ImplType?.Name}");

    }
}
