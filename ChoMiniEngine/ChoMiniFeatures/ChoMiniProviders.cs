using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Yoru.ChoMiniEngine
{
    public abstract class ChoMiniProvider 
    {
        abstract public void CollectEffects(List<object> objects, ChoMiniNode node);
    }
    public interface IChoMiniGameObjectActivationProvider{}

    public class ChoMiniGameObjectActivationProvider : ChoMiniProvider, IChoMiniGameObjectActivationProvider
    {
        public override void CollectEffects(List<object> objects, ChoMiniNode node)
        {
            foreach (var obj in objects)
            {
                if (obj is not GameObject go)
                    continue;

                Debug.Log("콜렉트 이펙트: " + go.name);
                node.Actions.Add(new ActivationAction(go));
            }
        }
    }

    public interface IChoMiniImageProvider{}

    public class ChoMiniImageFadeProvider : ChoMiniProvider, IChoMiniImageProvider
    {
        public override void CollectEffects(List<object> objects, ChoMiniNode node)
        {
            foreach (var obj in objects)
            {
                if (obj is not GameObject go)
                    continue;

                var img = go.GetComponent<Image>();
                if (img == null)
                    continue;

                node.Actions.Add(new FadeInAction(img));
            }
        }
    }
    public class ChoMiniImageFadeProviderSpeed2x : ChoMiniProvider, IChoMiniImageProvider
    {
        public override void CollectEffects(List<object> objects, ChoMiniNode node)
        {
            // TODO:
            // - Speed2x 전용 Fade 연출 분리 (duration 축소 또는 전용 Action)
            // - FadeInActionSpeed2x 또는 speedMultiplier 적용 방식 결정
            // - 공통 Fade Provider 베이스로 중복 제거 가능

            foreach (var obj in objects)
            {
                if (obj is not GameObject go)
                    continue;

                var img = go.GetComponent<Image>();
                if (img == null)
                    continue;

                node.Actions.Add(new FadeInAction(img));
            }
        }
    }

    public interface IChoMiniTextTypingProvider { }


    public class ChoMiniTextTypingProvider : ChoMiniProvider, IChoMiniTextTypingProvider
    {
        public override void CollectEffects(List<object> objects, ChoMiniNode node)
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
    public class ChoMiniTextTypingProviderSpeed2x : ChoMiniProvider, IChoMiniTextTypingProvider
    {
        public override void CollectEffects(List<object> objects, ChoMiniNode node)
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