using MessagePipe;
using System;


public struct ChoMiniCommandAdvanceRequested { }


public class ChoMiniCommandContext : IDisposable
{

    public IPublisher<ChoMiniCommandAdvanceRequested> SkipPublisher { get; private set; }
    public ISubscriber<ChoMiniCommandAdvanceRequested> AdvanceSubscriber { get; private set; }

    // Provider를 필드로 보관합니다.
    private readonly IServiceProvider _provider;
    private bool _disposed;
    public ChoMiniCommandContext()
    {
        var builder = new BuiltinContainerBuilder();

        builder.AddMessagePipe();
        builder.AddMessageBroker<ChoMiniCommandAdvanceRequested>();

        _provider = builder.BuildServiceProvider();

        // 스킵 인풋에 연결됨. 노드들의 듀레이션을 0으로 만든다.
        SkipPublisher = _provider.GetRequiredService<IPublisher<ChoMiniCommandAdvanceRequested>>();
        AdvanceSubscriber = _provider.GetRequiredService<ISubscriber<ChoMiniCommandAdvanceRequested>>();
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




