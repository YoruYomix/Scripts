using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
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

    // 임시용. 과거의 흔적. 리팩토링 예정
    public class LoopProvider
    {
        private LoopPlayer _loopPlayer;


        public void CollectEffects(GameObject go,
            ChoMiniNode node)
        {
            // 게임오브젝트를 해석하여 루프이펙트인지 검사
            if (go.name.ToString() != "Loop")
                return;
            _loopPlayer = new LoopPlayer();

            // 인덱스 0은 초기 재생용 → 루프 제외
            for (int i = 1; i < node.Actions.Count; i++)
            {
                IChoMiniNodeAction effect = node.Actions[i];
                float duration = effect.GetRequiredDuration();
                _loopPlayer.Register(effect, duration);
            }
        }

    }
}