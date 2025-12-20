using System.Collections.Generic;

namespace Yoru.ChoMiniEngine
{

    public interface IChoMiniInstaller
    {
        List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options
        );
    }
}