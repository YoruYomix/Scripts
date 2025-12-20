using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniUIImageProvider { }

    public class ChoMiniUIImageFadeProvider
        : IChoMiniProvider, IChoMiniUIImageProvider
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

            node.Actions.Add(new ChoMiniUIImageFadeInAction(img, scopeMsg));
        }
    }

    public class ChoMiniUIImageFadeProviderSpeed2x : IChoMiniProvider, IChoMiniUIImageProvider
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

            node.Actions.Add(new ChoMiniUIImageFadeInAction(img, scopeMsg));
        }
    }

}
