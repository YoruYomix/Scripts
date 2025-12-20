using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniProvider
    {
        public void CollectEffects(object objects, ChoMiniNode node, ChoMiniScopeMessageContext scopeMsg);
    }
}