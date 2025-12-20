using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine.Examples
{
    public interface IChoMiniStringTypingProvider { }

    public class ChoMiniStringTypingProvider : IChoMiniProvider, IChoMiniStringTypingProvider
    {
        public void CollectEffects(object cbj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            // TODO:
            // - string[] 입력 구조 확정 필요
            // - TMP_Text / Text 대상 결정 필요
            // - TypingAction 설계 후 구현 예정

            // 임시 구현:
            // 텍스트 타이핑은 아직 Action을 생성하지 않음
            // (Provider 파이프라인 테스트용 더미)
        }
    }
    public class ChoMiniStringTypingProviderSpeed2x : IChoMiniProvider, IChoMiniStringTypingProvider
    {
        public void CollectEffects(object cbj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            // TODO:
            // - string[] 입력 구조 확정 필요
            // - TMP_Text / Text 대상 결정 필요
            // - TypingAction 설계 후 구현 예정

            // 임시 구현:
            // 텍스트 타이핑은 아직 Action을 생성하지 않음
            // (Provider 파이프라인 테스트용 더미)
        }
    }


}