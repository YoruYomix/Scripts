using UnityEngine;
using UnityEngine.UI;

namespace Yoru.ChoMiniEngine
{
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

            node.Actions.Add(new ChoMiniGameObjectActivationAction(go));
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

}


