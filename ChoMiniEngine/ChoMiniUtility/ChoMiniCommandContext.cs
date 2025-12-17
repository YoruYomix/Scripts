using MessagePipe;
using System;


public struct ChoMiniCommandAdvanceRequested { }
public struct SequenceStartMessage { }
public struct SequenceEndMessage { }
public struct ChoMiniLocalCompleteRequested { }

public class ChoMiniCommandContext
{

    public IPublisher<ChoMiniCommandAdvanceRequested> SkipPublisher { get; private set; }
    public ISubscriber<ChoMiniCommandAdvanceRequested> AdvanceSubscriber { get; private set; }


    public ChoMiniCommandContext()
    {
        var builder = new BuiltinContainerBuilder();

        builder.AddMessagePipe();
        builder.AddMessageBroker<ChoMiniCommandAdvanceRequested>();

        var provider = builder.BuildServiceProvider();

        // 스킵 인풋에 연결됨. 노드들의 듀레이션을 0으로 만든다.
        SkipPublisher = provider.GetRequiredService<IPublisher<ChoMiniCommandAdvanceRequested>>();
        AdvanceSubscriber = provider.GetRequiredService<ISubscriber<ChoMiniCommandAdvanceRequested>>();
    }
}



public sealed class ChoMiniLocalMessageContext : IDisposable
{
    public IPublisher<ChoMiniLocalCompleteRequested> CompletePublisher { get; }
    public ISubscriber<ChoMiniLocalCompleteRequested> CompleteSubscriber { get; }


    private bool _disposed;

    public ChoMiniLocalMessageContext()
    {
        var builder = new BuiltinContainerBuilder();

        builder.AddMessagePipe();
        builder.AddMessageBroker<ChoMiniLocalCompleteRequested>();

        var provider = builder.BuildServiceProvider();



        CompletePublisher =
            provider.GetRequiredService<IPublisher<ChoMiniLocalCompleteRequested>>();

        CompleteSubscriber =
            provider.GetRequiredService<ISubscriber<ChoMiniLocalCompleteRequested>>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;


    }
}
