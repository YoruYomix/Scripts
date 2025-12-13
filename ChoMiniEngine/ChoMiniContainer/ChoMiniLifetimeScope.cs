using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{

    /// <summary>
    /// 한 번의 시퀀스 재생 생명주기를 감싸는 최소 스코프.
    /// 지금 구조에서는 FlowOrchestrator + NodeFactory + MessagePipeContext만 감싼다.
    /// 나중에 Recomposer/Reactor 생기면 여기로 편입시키면 됨.
    /// </summary>
    public sealed class ChoMiniLifetimeScope : IDisposable
    {
        private readonly IInstallerKeyResolver _installerKeyResolver;
        private readonly IChoMiniInstaller _installer;
        private readonly ChoMiniOrchestrator _orchestrator;
        private readonly List<Func<IChoMiniActionProvider>> _providerCreators;
        private readonly Dictionary<string, Func<ChoMiniSequenceFactory>> _factoryCreators;
        private readonly Dictionary<string, IInstallerResource> _resources = new();
        readonly ChoMiniLocalMessageContext _localMsg;
        private readonly ChoMiniSequenceFactory _factory;
        readonly ChoMiniCommandContext _msg;


        private bool _disposed;

        public ChoMiniLifetimeScope(
            IInstallerKeyResolver installerKeyResolver,
            IChoMiniInstaller installer,
            ChoMiniOrchestrator orchestrator,
            Dictionary<string, Func<ChoMiniSequenceFactory>> factories,
            List<Func<IChoMiniActionProvider>> providers,
            ChoMiniCommandContext msg)
        {
            _installerKeyResolver = installerKeyResolver
                ?? throw new ArgumentNullException(nameof(installerKeyResolver));
            _installer = installer ?? throw new ArgumentNullException(nameof(installer));
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _factoryCreators = factories ?? throw new ArgumentNullException(nameof(factories));
            _providerCreators = providers ?? throw new ArgumentNullException(nameof(providers));
            _localMsg = new ChoMiniLocalMessageContext();
            _msg = msg;

            _factory = _factoryCreators["Default"]();
            _orchestrator.Initialize(_localMsg, Dispose);

            // 지금 구조에선 오케스트레이터가 팩토리를 알고 있어야 하니까
            // 스코프 생성 시점에 한 번만 묶어준다.
            _orchestrator.SetFactory(_factory);
        }

        public ChoMiniLifetimeScope Bind<TInstaller>(
            string key,
            IInstallerResource resource)
            where TInstaller : IChoMiniInstaller
                {
                    _resources[key] = resource;
                    return this;
                }

        public async UniTask BootAsync()
        {


            var installerKey = _installerKeyResolver.Resolve();

            if (!_resources.TryGetValue(installerKey, out var resource))
                throw new Exception(
                    $"Resource not bound for installer '{installerKey}'");


            _installer.Bind(resource);
    
            var targets = _installer.InstallTargets();


            _factory.Initialize(
                targets,
                _providerCreators,
                _localMsg.SkipSubscriber
            );
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
                throw new ObjectDisposedException(nameof(ChoMiniLifetimeScope));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _orchestrator?.Dispose();

            // 메시지파이프 컨텍스트 정리 (구독 해제 등)
            // _msg?.Dispose();

            // NodeFactory나 Orchestrator가 IDisposable이면 여기서 같이 Dispose 해도 됨.
            // 지금은 IDisposable 아니라고 가정.
        }
    }

}