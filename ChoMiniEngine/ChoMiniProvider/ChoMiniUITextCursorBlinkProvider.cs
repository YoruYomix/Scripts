using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yoru.ChoMiniEngine;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniUITextCursorBlinkProvider : IChoMiniProvider
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

            node.Actions.Add(new ChoMiniTextComponentCursorBlinkAction(text, scopeMsg));
        }
    }
}
