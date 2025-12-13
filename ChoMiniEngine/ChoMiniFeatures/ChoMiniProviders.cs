using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IEffectProvider
{
    void CollectEffects(GameObject go, FlowNode node);
}

public class DefaultActivationProvider : IEffectProvider
{
    public void CollectEffects(GameObject go, FlowNode node)
    {
        Debug.Log("콜렉트 이펙트" + go.name);
        node.Effects.Add(new ActivationEffect(go));
    }
}
public class ImageEffectProvider : IEffectProvider
{
    public void CollectEffects(GameObject go, FlowNode node)
    {
        var img = go.GetComponent<Image>();
        if (img == null) return;

        node.Effects.Add(new ActivationEffect(go));
        node.Effects.Add(new FadeInEffect(img));
    }
}

public class LoopProvider
{
    private LoopPlayer _loopPlayer;


    public void CollectEffects(GameObject go,
        FlowNode node)
    {
        // 게임오브젝트를 해석하여 루프이펙트인지 검사
        if (go.name.ToString() != "Loop")
            return;
        _loopPlayer = new LoopPlayer();

        // 인덱스 0은 초기 재생용 → 루프 제외
        for (int i = 1; i < node.Effects.Count; i++)
        {
            IFlowNodeEffect effect = node.Effects[i];
            float duration = effect.GetRequiredDuration();
            _loopPlayer.Register(effect, duration);
        }
    }

}