using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniContainer
    {
        // Installer / Factory / Provider 저장소
        internal readonly Dictionary<string, Func<Transform, IChoMiniInstaller>> _installers
            = new Dictionary<string, Func<Transform, IChoMiniInstaller>>();

        internal readonly Dictionary<string, Func<ChoMiniSequenceFactory>> _factories
            = new Dictionary<string, Func<ChoMiniSequenceFactory>>();

        internal readonly List<Func<IChoMiniActionProvider>> _providers
            = new List<Func<IChoMiniActionProvider>>();

        // Runner / Orchestrator / MessagePipe
        private Func<ChoMiniNodeRunner> _nodeRunnerFactory;
        private Func<GlobalMessageContext> _msgFactory;
        private Func<ChoMiniOrchestrator> _orchestratorFactory;
        internal GlobalMessageContext _globalContext;


        private ChoMiniContainer() { }

        // --------------------------
        // 생성 시작
        // --------------------------
        public static ChoMiniContainerBuilder Create()
        {
            return new ChoMiniContainerBuilder();
        }

        // --------------------------
        // 세션(스코프) 생성
        // --------------------------
        public ChoMiniLifetimeScope CreateScope(FlowSessionOptions options)
        {
            // Installer 선택
            if (!_installers.TryGetValue(options.InstallerKey, out var installerFunc))
                throw new Exception($"Installer '{options.InstallerKey}' not registered");

            IChoMiniInstaller installer = installerFunc(options.SceneRoot);
            List<Transform> targets = installer.InstallTargets();


            // 메시지 컨텍스트
            GlobalMessageContext msg = _globalContext;
            LocalMessageContext _localMsg = new LocalMessageContext();

            // Orchestrator & Runner
            ChoMiniNodeRunner nodeRunner = _nodeRunnerFactory();
            ChoMiniOrchestrator orchestrator = _orchestratorFactory();

            // Factory 선택
            if (!_factories.TryGetValue(options.FactoryKey, out var factoryFunc))
                throw new Exception($"Factory '{options.FactoryKey}' not registered");

            ChoMiniSequenceFactory factory = factoryFunc();

            // 초기화
            factory.Initialize(
                targets,
                _providers,
                msg.SkipAllSubscriber
            );

            return new ChoMiniLifetimeScope(orchestrator, factory, msg, _localMsg);
        }

        // ===========================================================
        // Builder
        // ===========================================================
        public sealed class ChoMiniContainerBuilder
        {
            private readonly ChoMiniContainer _c = new ChoMiniContainer();


            // ---------- Installer DSL ----------
            public ChoMiniInstallerBuilder<TInstaller> Register<TInstaller>(string key)
                where TInstaller : IChoMiniInstaller
            {
                return new ChoMiniInstallerBuilder<TInstaller>(_c, key);
            }

            public sealed class ChoMiniInstallerBuilder<TInstaller>
                where TInstaller : IChoMiniInstaller
            {
                private readonly ChoMiniContainer _c;
                private readonly string _key;
                private readonly Type _installerType;

                public ChoMiniInstallerBuilder(ChoMiniContainer c, string key)
                {
                    _c = c;
                    _key = key;
                    _installerType = typeof(TInstaller);   // 🔥 핵심
                }

                public ChoMiniContainerBuilder Using(Transform root)
                {
                    _c._installers[_key] = (sceneRoot) =>
                    {
                        return (IChoMiniInstaller)Activator.CreateInstance(
                            _installerType,
                            new object[] { root }          // root 생성자 매칭
                        );
                    };

                    return new ChoMiniContainerBuilder(_c);
                }
            }


            // ---------- Provider DSL ----------
            public ChoMiniContainerBuilder Register<TProvider>()
                where TProvider : IChoMiniActionProvider, new()
            {
                _c._providers.Add(() => new TProvider());
                return this;
            }

            // ---------- Factory DSL ----------
            public ChoMiniContainerBuilder RegisterFactory<TFactory>(string key)
                where TFactory : ChoMiniSequenceFactory, new()
            {
                _c._factories[key] = () => new TFactory();
                return this;
            }

            // ---------- Runner/Orchestrator/MessagePipe ----------
            public ChoMiniContainerBuilder UseNodeRunner(Func<ChoMiniNodeRunner> f)
            {
                _c._nodeRunnerFactory = f;
                return this;
            }

            public ChoMiniContainerBuilder UseOrchestrator(Func<ChoMiniOrchestrator> f)
            {
                _c._orchestratorFactory = f;
                return this;
            }

            public ChoMiniContainerBuilder UseMessagePipe(Func<GlobalMessageContext> f)
            {
                _c._msgFactory = f;
                return this;
            }

            public ChoMiniContainerBuilder SubscribeGlobalMessages(GlobalMessageContext global)
            {
                _c._globalContext = global;
                return this;
            }


            public ChoMiniContainer Build()
            {
                if (_c._msgFactory == null)
                    _c._msgFactory = () => new GlobalMessageContext();

                if (_c._nodeRunnerFactory == null)
                    _c._nodeRunnerFactory = () => new ChoMiniNodeRunner();

                if (_c._orchestratorFactory == null)
                    _c._orchestratorFactory = () =>
                        new ChoMiniOrchestrator(
                            new ChoMiniNodeRunner());

                return _c;
            }

            // 내부 생성자
            private ChoMiniContainerBuilder(ChoMiniContainer c)
            {
                _c = c;
            }
            public ChoMiniContainerBuilder() { }
        }


        public abstract class BaseChoMiniRegisterBuilder
        {
            protected readonly ChoMiniContainerBuilder _parent;
            protected readonly List<string> _conditions = new List<string>();

            protected BaseChoMiniRegisterBuilder(ChoMiniContainerBuilder parent)
            {
                _parent = parent;
            }

            public BaseChoMiniRegisterBuilder When(string condition)
            {
                _conditions.Add(condition);
                return this;
            }

            // 앞으로 .Not() .Until() .Except() 등도 여기에 추가하면 됨.

            public abstract ChoMiniContainerBuilder End();
        }
    }
}