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
        public IPublisher<ChoMiniScopeCleanupRequested> CleanupPublisher { get; }
        public ISubscriber<ChoMiniScopeCleanupRequested> CleanupSubscriber { get; }

        // Provider를 필드로 보관합니다.
        private readonly IServiceProvider _provider;
        private bool _disposed;

        public ChoMiniScopeMessageContext()
        {
            var builder = new BuiltinContainerBuilder();

            builder.AddMessagePipe();
            builder.AddMessageBroker<ChoMiniScopeCompleteRequested>();

            _provider = builder.BuildServiceProvider();



            CompletePublisher =
                _provider.GetRequiredService<IPublisher<ChoMiniScopeCompleteRequested>>();

            CompleteSubscriber =
                _provider.GetRequiredService<ISubscriber<ChoMiniScopeCompleteRequested>>();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // IDisposable을 구현한 Provider라면 Dispose를 호출해줍니다.
            if (_provider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }

        }
    }
}

