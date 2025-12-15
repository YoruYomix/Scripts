using MessagePipe;


public struct ChoMiniCommandAdvanceRequested { }
public struct SequenceStartMessage { }
public struct SequenceEndMessage { }
public struct ChoMiniLocalSkipRequested { }

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





public class ChoMiniLocalMessageContext
{
    public IPublisher<ChoMiniLocalSkipRequested> SkipPublisher { get; }
    public ISubscriber<ChoMiniLocalSkipRequested> SkipSubscriber { get; }

    public IPublisher<SequenceStartMessage> StartPublisher { get; }
    public ISubscriber<SequenceStartMessage> StartSubscriber { get; }


    public IPublisher<SequenceEndMessage> EndPublisher { get; }
    public ISubscriber<SequenceEndMessage> EndSubscriber { get; }

    public ChoMiniLocalMessageContext()
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe();
        builder.AddMessageBroker<ChoMiniLocalSkipRequested>();
        builder.AddMessageBroker<SequenceStartMessage>();
        builder.AddMessageBroker<SequenceEndMessage>();

        var provider = builder.BuildServiceProvider();

        SkipPublisher = provider.GetRequiredService<IPublisher<ChoMiniLocalSkipRequested>>();
        SkipSubscriber = provider.GetRequiredService<ISubscriber<ChoMiniLocalSkipRequested>>();

        StartPublisher = provider.GetRequiredService<IPublisher<SequenceStartMessage>>();
        StartSubscriber = provider.GetRequiredService<ISubscriber<SequenceStartMessage>>();

        EndPublisher = provider.GetRequiredService<IPublisher<SequenceEndMessage>>();
        EndSubscriber = provider.GetRequiredService<ISubscriber<SequenceEndMessage>>();
    }
}