using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniProvider 
    {
        public void CollectEffects(object objects, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg );
    }
    public interface IChoMiniGameObjectActivationProvider{}

    public class ChoMiniGameObjectActivationProvider
        : IChoMiniProvider, IChoMiniGameObjectActivationProvider
    {
        public void CollectEffects(object obj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            if (obj is not GameObject go)
                return;

            if (go == null)
                return;

            Debug.Log("콜렉트 이펙트: " + go.name);
            node.Actions.Add(new ChoMiniGameObjectActivationAction(go));
        }
    }

    public interface IChoMiniImageProvider{}

    public class ChoMiniImageFadeProvider
        : IChoMiniProvider, IChoMiniImageProvider
    {
        public void CollectEffects(object obj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            if (obj is not GameObject go)
                return;

            if (go == null)
                return;

            Image img = go.GetComponent<Image>();
            if (img == null)
                return;

            node.Actions.Add(new ChoMiniUIImageFadeInAction(img));
        }
    }

    public class ChoMiniImageFadeProviderSpeed2x : IChoMiniProvider, IChoMiniImageProvider
    {
        public void CollectEffects(object obj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            // TODO:
            // - Speed2x 전용 Fade 연출 분리 (duration 축소 또는 전용 Action)
            // - FadeInActionSpeed2x 또는 speedMultiplier 적용 방식 결정
            // - 공통 Fade Provider 베이스로 중복 제거 가능


            if (obj is not GameObject go)
                return;

            if (go == null)
                return;

            Image img = go.GetComponent<Image>();
            if (img == null)
                return;

            node.Actions.Add(new ChoMiniUIImageFadeInAction(img));
        }
    }

    public interface IChoMiniUITextComponentProvider { }

    public class ChoMiniUITextComponentTypingProvider : IChoMiniProvider, IChoMiniUITextComponentProvider
    {
        public void CollectEffects(object obj, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg)
        {
            if (obj is not GameObject go)
                return;

            if (go == null)
                return;

            Text text = go.GetComponent<Text>();
            if (text == null)
                return;

            node.Actions.Add(new ChoMiniTextComponentTypingAction(text, scopeMsg));
        }
    }

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


