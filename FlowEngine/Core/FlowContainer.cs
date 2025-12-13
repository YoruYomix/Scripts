using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

  
public sealed class FlowContainer
{
    // Installer / Factory / Provider 저장소
    internal readonly Dictionary<string, Func<Transform, IInstaller>> _installers
        = new Dictionary<string, Func<Transform, IInstaller>>();

    internal readonly Dictionary<string, Func<FlowSequenceFactory>> _factories
        = new Dictionary<string, Func<FlowSequenceFactory>>();

    internal readonly List<Func<IEffectProvider>> _providers
        = new List<Func<IEffectProvider>>();

    // Runner / Orchestrator / MessagePipe
    private Func<FlowNodeRunner> _nodeRunnerFactory;
    private Func<GlobalMessageContext> _msgFactory;
    private Func<FlowOrchestrator> _orchestratorFactory;
    internal GlobalMessageContext _globalContext;


    private FlowContainer() { }

    // --------------------------
    // 생성 시작
    // --------------------------
    public static FlowContainerBuilder Create()
    {
        return new FlowContainerBuilder();
    }

    // --------------------------
    // 세션(스코프) 생성
    // --------------------------
    public FlowLifetimeScope CreateScope(FlowSessionOptions options)
    {
        // Installer 선택
        if (!_installers.TryGetValue(options.InstallerKey, out var installerFunc))
            throw new Exception($"Installer '{options.InstallerKey}' not registered");

        IInstaller installer = installerFunc(options.SceneRoot);
        List<Transform> targets = installer.InstallTargets();


        // 메시지 컨텍스트
        GlobalMessageContext msg = _globalContext;
        LocalMessageContext _localMsg = new LocalMessageContext();

        // Orchestrator & Runner
        FlowNodeRunner nodeRunner = _nodeRunnerFactory();
        FlowOrchestrator orchestrator = _orchestratorFactory();

        // Factory 선택
        if (!_factories.TryGetValue(options.FactoryKey, out var factoryFunc))
            throw new Exception($"Factory '{options.FactoryKey}' not registered");

        FlowSequenceFactory factory = factoryFunc();

        // 초기화
        factory.Initialize(
            targets,
            _providers,
            msg.SkipAllSubscriber
        );

        return new FlowLifetimeScope(orchestrator, factory, msg, _localMsg);
    }

    // ===========================================================
    // Builder
    // ===========================================================
    public sealed class FlowContainerBuilder
    {
        private readonly FlowContainer _c = new FlowContainer();


        // ---------- Installer DSL ----------
        public InstallerBuilder<TInstaller> Register<TInstaller>(string key)
            where TInstaller : IInstaller
        {
            return new InstallerBuilder<TInstaller>(_c, key);
        }

        public sealed class InstallerBuilder<TInstaller>
            where TInstaller : IInstaller
        {
            private readonly FlowContainer _c;
            private readonly string _key;
            private readonly Type _installerType;

            public InstallerBuilder(FlowContainer c, string key)
            {
                _c = c;
                _key = key;
                _installerType = typeof(TInstaller);   // 🔥 핵심
            }

            public FlowContainerBuilder Using(Transform root)
            {
                _c._installers[_key] = (sceneRoot) =>
                {
                    return (IInstaller)Activator.CreateInstance(
                        _installerType,
                        new object[] { root }          // root 생성자 매칭
                    );
                };

                return new FlowContainerBuilder(_c);
            }
        }


        // ---------- Provider DSL ----------
        public FlowContainerBuilder Register<TProvider>()
            where TProvider : IEffectProvider, new()
        {
            _c._providers.Add(() => new TProvider());
            return this;
        }

        // ---------- Factory DSL ----------
        public FlowContainerBuilder RegisterFactory<TFactory>(string key)
            where TFactory : FlowSequenceFactory, new()
        {
            _c._factories[key] = () => new TFactory();
            return this;
        }

        // ---------- Runner/Orchestrator/MessagePipe ----------
        public FlowContainerBuilder UseNodeRunner(Func<FlowNodeRunner> f)
        {
            _c._nodeRunnerFactory = f;
            return this;
        }

        public FlowContainerBuilder UseOrchestrator(Func<FlowOrchestrator> f)
        {
            _c._orchestratorFactory = f;
            return this;
        }

        public FlowContainerBuilder UseMessagePipe(Func<GlobalMessageContext> f)
        {
            _c._msgFactory = f;
            return this;
        }

        public FlowContainerBuilder SubscribeGlobalMessages(GlobalMessageContext global)
        {
            _c._globalContext = global;
            return this;
        }


        public FlowContainer Build()
        {
            if (_c._msgFactory == null)
                _c._msgFactory = () => new GlobalMessageContext();

            if (_c._nodeRunnerFactory == null)
                _c._nodeRunnerFactory = () => new FlowNodeRunner();

            if (_c._orchestratorFactory == null)
                _c._orchestratorFactory = () =>
                    new FlowOrchestrator(
                        new FlowNodeRunner());

            return _c;
        }

        // 내부 생성자
        private FlowContainerBuilder(FlowContainer c)
        {
            _c = c;
        }
        public FlowContainerBuilder() { }
    }


    public abstract class BaseRegisterBuilder
    {
        protected readonly FlowContainerBuilder _parent;
        protected readonly List<string> _conditions = new List<string>();

        protected BaseRegisterBuilder(FlowContainerBuilder parent)
        {
            _parent = parent;
        }

        public BaseRegisterBuilder When(string condition)
        {
            _conditions.Add(condition);
            return this;
        }

        // 앞으로 .Not() .Until() .Except() 등도 여기에 추가하면 됨.

        public abstract FlowContainerBuilder End();
    }
}

// git test

public class TestGit{}
// SADSAD
//ASDASDASD