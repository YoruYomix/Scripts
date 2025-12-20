using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public enum ScopeState
    {
        Created,
        Playing,
        Completed,
        Disposed
    }
    public sealed class ChoMiniLifetimeScope : IDisposable
    {
        private readonly IReadOnlyList<BootRule> _installerRules;
        private readonly IReadOnlyList<BootRule> _factoryRules;
        private readonly IReadOnlyList<BootRule> _providerRules;
        private readonly IReadOnlyList<ReactorRule> _reactorRules;

        private readonly ChoMiniOptions _options;
        private readonly ChoMiniScopeMessageContext _scopeMsg;
        private ChoMiniOrchestrator _orchestrator;

        private readonly Dictionary<(Type installerType, object? key), object> _bindings = new();

        private ChoMiniImplementationSelector _implementationSelector;
        private ChoMiniReactorScheduler _reactorScheduler;
        private List<NodeSource> _nodeSources;

        private ScopeState _state = ScopeState.Created;
        private bool _paused;

        public ScopeState State => _state;
        public ChoMiniOptions Options => _options;

        public IReadOnlyList<BootRule> InstallerRules => _installerRules;
        public IReadOnlyList<BootRule> FactoryRules => _factoryRules;
        public IReadOnlyList<BootRule> ProviderRules => _providerRules;

        private ChoMiniImplementationSelector implementationSelector =>
            _implementationSelector ??= new ChoMiniImplementationSelector(this);

        public ChoMiniLifetimeScope(
            IReadOnlyList<BootRule> installerRules,
            IReadOnlyList<BootRule> factoryRules,
            IReadOnlyList<BootRule> providerRules,
            IReadOnlyList<ReactorRule> reactorRules,
            ChoMiniOptions options,
            ChoMiniScopeMessageContext scopeMsg,
            ChoMiniOrchestrator orchestrator)
        {
            _installerRules = installerRules;
            _factoryRules = factoryRules;
            _providerRules = providerRules;
            _reactorRules = reactorRules;
            _options = options;
            _scopeMsg = scopeMsg;
            _orchestrator = orchestrator;
        }

        // ==================================================
        // Play
        // ==================================================
        public async UniTask Play()
        {
            if (_state == ScopeState.Disposed)
                throw new ObjectDisposedException(nameof(ChoMiniLifetimeScope));

            if (_state != ScopeState.Created)
                throw new InvalidOperationException($"Play() not allowed in state {_state}");

            _state = ScopeState.Playing;

            // ⭐ NodeSource 합성은 전담 객체에 위임
            _nodeSources =
                new ChoMiniNodeSourceAssembler(
                    installerRules: _installerRules,
                    options: _options,
                    bindings: _bindings
                ).Assemble();

            _reactorScheduler = new ChoMiniReactorScheduler(
                rules: _reactorRules,
                nodeSources: _nodeSources,
                msg: _scopeMsg
            );

            try
            {
                IChoMiniFactory factory = BuildFactory();
                _orchestrator.Initialize(factory, _scopeMsg);
                await _orchestrator.PlaySequence();

                _state = ScopeState.Completed;
            }
            catch
            {
                _state = ScopeState.Completed;
                throw;
            }
        }

        // ==================================================
        // Pause / Resume / Complete
        // ==================================================
        public void Pause()
        {
            if (_state != ScopeState.Playing || _paused) return;
            _paused = true;
            _orchestrator.Pause();
        }

        public void Resume()
        {
            if (_state != ScopeState.Playing || !_paused) return;
            _paused = false;
            _orchestrator.Resume();
        }

        public void Complete()
        {
            if (_state != ScopeState.Playing) return;
            _orchestrator.CompleteSequence();
            _state = ScopeState.Completed;
        }

        // ==================================================
        // DSL Binding
        // ==================================================
        public ChoMiniLifetimeScope Bind<TInstaller>(object resource)
            => Bind<TInstaller>(null, resource);

        public ChoMiniLifetimeScope Bind<TInstaller>(object key, object resource)
        {
            var k = (typeof(TInstaller), key);

            if (_bindings.ContainsKey(k))
                throw new InvalidOperationException($"Binding already exists: {typeof(TInstaller).Name}/{key}");

            _bindings.Add(k, resource);
            return this;
        }

        // ==================================================
        // Factory build
        // ==================================================
        private IChoMiniFactory BuildFactory()
        {
            implementationSelector.EnsureSelected();

            if (implementationSelector.SelectedFactoryType == null)
                throw new InvalidOperationException("Factory not selected");

            var factory =
                (IChoMiniFactory)Activator.CreateInstance(implementationSelector.SelectedFactoryType);

            var providers = new List<IChoMiniProvider>();
            foreach (var type in implementationSelector.SelectedProviderTypes)
                providers.Add((IChoMiniProvider)Activator.CreateInstance(type));

            factory.Initialize(
                _nodeSources,
                providers,
                _scopeMsg.CompleteSubscriber,
                _scopeMsg
            );

            return factory;
        }

        // ==================================================
        // Dispose
        // ==================================================
        public void Dispose()
        {
            if (_state == ScopeState.Disposed) return;

            _state = ScopeState.Disposed;
            _paused = false;

            _scopeMsg.CleanupPublisher.Publish(new ChoMiniScopeCleanupRequested());
            _orchestrator?.Dispose();
            _scopeMsg.Dispose();
        }
    }


}