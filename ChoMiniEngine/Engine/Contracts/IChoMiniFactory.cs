using MessagePipe;
using System.Collections.Generic;

namespace Yoru.ChoMiniEngine
{

    public interface IChoMiniFactory 
    {
        void Initialize(
            List<NodeSource> sources,
            List<IChoMiniProvider> providers,
            ISubscriber<ChoMiniScopeCompleteRequested> skipSubscriber,
            ChoMiniScopeMessageContext scopeMessageContext);

        int Count { get; }


        ChoMiniNode Create();

    }

}