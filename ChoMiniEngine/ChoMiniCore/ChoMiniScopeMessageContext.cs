using MessagePipe;
using System;

namespace Yoru.ChoMiniEngine
{
    public struct ChoMiniScopeCompleteRequested { }
    public struct ChoMiniScopeCleanupRequested { }
    public sealed class ChoMiniScopeMessageContext : IDisposable
    {
        public IPublisher<ChoMiniScopeCompleteRequested> CompletePublisher { get; }
        public ISubscriber<ChoMiniScopeCompleteRequested> CompleteSubscriber { get; }


        private bool _disposed;

        public ChoMiniScopeMessageContext()
        {
            var builder = new BuiltinContainerBuilder();

            builder.AddMessagePipe();
            builder.AddMessageBroker<ChoMiniScopeCompleteRequested>();

            var provider = builder.BuildServiceProvider();



            CompletePublisher =
                provider.GetRequiredService<IPublisher<ChoMiniScopeCompleteRequested>>();

            CompleteSubscriber =
                provider.GetRequiredService<ISubscriber<ChoMiniScopeCompleteRequested>>();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;


        }
    }
}

