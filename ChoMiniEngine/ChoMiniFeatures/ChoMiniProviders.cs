using UnityEngine;
using UnityEngine.UI;


namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniActionProvider
    {
        void CollectEffects(GameObject go, ChoMiniNode node);
    }

    public class DefaultActivationProvider : IChoMiniActionProvider
    {
        public void CollectEffects(GameObject go, ChoMiniNode node)
        {
            Debug.Log("콜렉트 이펙트" + go.name);
            node.Actions.Add(new ActivationAction(go));
        }
    }
    public class ImageActionProvider : IChoMiniActionProvider
    {
        public void CollectEffects(GameObject go, ChoMiniNode node)
        {
            var img = go.GetComponent<Image>();
            if (img == null) return;

            node.Actions.Add(new ActivationAction(go));
            node.Actions.Add(new FadeInAction(img));
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