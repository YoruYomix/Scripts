using Yoru.ChoMiniEngine;
using UnityEngine;

public sealed class ChoMiniComposer
{
    private readonly ChoMiniLifetimeScope _scope;
    private bool _isComposed;

    public ChoMiniComposer(ChoMiniLifetimeScope scope)
    {
        _scope = scope;
    }

    public void EnsureComposed()
    {
        if (_isComposed)
            return;

        Debug.Log("[Composer] Compose start");
        _isComposed = true;
    }
}
