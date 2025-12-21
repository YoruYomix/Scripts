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
            if (scope == null) return;

            switch (scope.State)
            {
                case ScopeState.Created:
                    scope.Play().Forget();
                    break;

                case ScopeState.Playing:
                    scope.Complete();
                    break;

                case ScopeState.Completed:
                    scope.Dispose();
                    break;

                case ScopeState.Disposed:
                    // noop
                    break;
            }
        }
    }

}
