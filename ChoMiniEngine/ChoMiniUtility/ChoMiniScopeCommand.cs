using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine.Utility
{
    public static class ChoMiniScopeCommand
    {
        public static void Advance(this ChoMiniLifetimeScope scope)
        {
            if (!scope.IsPlaying)
                scope.Play().Forget();
            else
                scope.Complete();
        }
    }

}
