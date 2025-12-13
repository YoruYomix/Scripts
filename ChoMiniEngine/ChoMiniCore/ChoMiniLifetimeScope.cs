
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// 한 번의 시퀀스 재생 생명주기를 감싸는 최소 스코프.
/// 지금 구조에서는 FlowOrchestrator + NodeFactory + MessagePipeContext만 감싼다.
/// 나중에 Recomposer/Reactor 생기면 여기로 편입시키면 됨.
/// </summary>
public sealed class FlowLifetimeScope : IDisposable
{
    private readonly FlowOrchestrator _orchestrator;
    private readonly FlowSequenceFactory _factory;
    private readonly GlobalMessageContext _msg;
    readonly LocalMessageContext _localMsg;

    private bool _disposed;

    public FlowLifetimeScope(
        FlowOrchestrator orchestrator,
        FlowSequenceFactory factory,
        GlobalMessageContext msg,
        LocalMessageContext localMsg)
    {
        _orchestrator = orchestrator;
        _factory = factory;
        _msg = msg;
        _localMsg = localMsg;


        _orchestrator.Initialize(_localMsg);

        // 지금 구조에선 오케스트레이터가 팩토리를 알고 있어야 하니까
        // 스코프 생성 시점에 한 번만 묶어준다.
        _orchestrator.SetFactory(_factory);
    }

    /// <summary>
    /// 스코프 한 번 재생 (지금은 테스트용으로 1회만 쓴다고 가정)
    /// </summary>
    public async UniTask PlayAsync()
    {
        ThrowIfDisposed();

        // 나중에 여기서 Reactor.Activate() / Recomposer.Compose() 같은 단계가 추가될 예정
        await _orchestrator.PlaySequence();

        // 여기서 바로 Dispose 해도 되고, 바깥에서 수동으로 Dispose() 해도 됨.
        // 테스트 단계에선 바깥에서 Dispose 호출하는 쪽으로 두는 게 컨트롤하기 편함.
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FlowLifetimeScope));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 메시지파이프 컨텍스트 정리 (구독 해제 등)
        // _msg?.Dispose();

        // NodeFactory나 Orchestrator가 IDisposable이면 여기서 같이 Dispose 해도 됨.
        // 지금은 IDisposable 아니라고 가정.
    }
}

