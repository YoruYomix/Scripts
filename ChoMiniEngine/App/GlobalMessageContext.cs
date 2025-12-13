using MessagePipe;


public struct SkipAllNodesMessage { }
public struct SequenceStartMessage { }
public struct SequenceEndMessage { }

public class GlobalMessageContext
{

    public IPublisher<SkipAllNodesMessage> SkipAllPublisher { get; private set; }
    public ISubscriber<SkipAllNodesMessage> SkipAllSubscriber { get; private set; }


    public GlobalMessageContext()
    {
        var builder = new BuiltinContainerBuilder();

        builder.AddMessagePipe();
        builder.AddMessageBroker<SkipAllNodesMessage>();

        var provider = builder.BuildServiceProvider();

        // 스킵 인풋에 연결됨. 노드들의 듀레이션을 0으로 만든다.
        SkipAllPublisher = provider.GetRequiredService<IPublisher<SkipAllNodesMessage>>();
        SkipAllSubscriber = provider.GetRequiredService<ISubscriber<SkipAllNodesMessage>>();
    }
}





public class LocalMessageContext
{
    public IPublisher<SequenceStartMessage> StartPublisher { get; }
    public ISubscriber<SequenceStartMessage> StartSubscriber { get; }

    public IPublisher<SequenceEndMessage> EndPublisher { get; }
    public ISubscriber<SequenceEndMessage> EndSubscriber { get; }

    public LocalMessageContext()
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe();
        builder.AddMessageBroker<SequenceStartMessage>();
        builder.AddMessageBroker<SequenceEndMessage>();

        var provider = builder.BuildServiceProvider();

        StartPublisher = provider.GetRequiredService<IPublisher<SequenceStartMessage>>();
        StartSubscriber = provider.GetRequiredService<ISubscriber<SequenceStartMessage>>();

        EndPublisher = provider.GetRequiredService<IPublisher<SequenceEndMessage>>();
        EndSubscriber = provider.GetRequiredService<ISubscriber<SequenceEndMessage>>();
    }
}