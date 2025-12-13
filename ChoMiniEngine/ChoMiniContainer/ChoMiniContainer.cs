using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniContainer
    {
        // --------------------------
        // Installer storage
        // --------------------------
        internal readonly Dictionary<string, Func<Transform, IChoMiniInstaller>> _installers
            = new();

        internal readonly List<ChoMiniInstallerRule> _installerRules
            = new();

        // Factory / Provider
        internal readonly Dictionary<string, Func<ChoMiniSequenceFactory>> _factories
            = new();

        internal readonly List<Func<IChoMiniActionProvider>> _providers
            = new();

        // Core
        private Func<ChoMiniNodeRunner> _nodeRunnerFactory;
        private Func<ChoMiniOrchestrator> _orchestratorFactory;

        private ChoMiniContainer() { }

        // ===========================================================
        // Scope creation
        // ===========================================================
        public ChoMiniLifetimeScope CreateScope(FlowSessionOptions options)
        {
            // ---------- Resolve Installer Key (Strict) ----------
            string installerKey = ResolveInstallerKey();

            if (!_installers.TryGetValue(installerKey, out var installerFunc))
                throw new Exception($"Installer '{installerKey}' not registered");

            IChoMiniInstaller installer = installerFunc(options.SceneRoot);
            List<Transform> targets = installer.InstallTargets();

            // ---------- Context ----------
            ChoMiniCommandContext commandContext = ChoMiniEngine.CommandContext;
            ChoMiniLocalMessageContext localMsg = new ChoMiniLocalMessageContext();

            // ---------- Orchestrator & Runner ----------
            ChoMiniNodeRunner runner = _nodeRunnerFactory();
            ChoMiniOrchestrator orchestrator = _orchestratorFactory();

            // ---------- Factory ----------
            if (!_factories.TryGetValue(options.FactoryKey, out var factoryFunc))
                throw new Exception($"Factory '{options.FactoryKey}' not registered");

            ChoMiniSequenceFactory factory = factoryFunc();

            factory.Initialize(
                targets,
                _providers,
                localMsg.SkipSubscriber
            );

            return new ChoMiniLifetimeScope(
                orchestrator,
                factory,
                commandContext,
                localMsg
                );
        }

        private string ResolveInstallerKey()
        {
            foreach (var rule in _installerRules)
            {
                foreach (var (cond, key) in rule.Entries)
                {
                    if (cond())
                        return key;
                }
            }
         
            throw new Exception("No Installer matched (Strict)");
        }

        // ===========================================================
        // Builder
        // ===========================================================
        public sealed class Builder
        {
            private readonly ChoMiniContainer _c = new ChoMiniContainer();

            // ---------- Installer DSL ----------
            public ChoMiniInstallerRuleBuilder Installer<TInstaller>()
                where TInstaller : IChoMiniInstaller
            {
                                var rule = new ChoMiniInstallerRule
                {
                    InstallerType = typeof(TInstaller)
                };

                _c._installerRules.Add(rule);
                return new ChoMiniInstallerRuleBuilder(rule);
            }

            // 실제 Installer 생성자 등록 (key → ctor)
            public Builder RegisterInstaller<TInstaller>(string key, Transform root)
                where TInstaller : IChoMiniInstaller
            {
                Type t = typeof(TInstaller);

                _c._installers[key] = _ =>  
                    (IChoMiniInstaller)Activator.CreateInstance(
                        t,
                        new object[] { root }
                    ); 

                return this;
            }

            // ---------- Provider ----------
            public Builder RegisterProvider<TProvider>()
                where TProvider : IChoMiniActionProvider, new()
            {
                _c._providers.Add(() => new TProvider());
                return this;
            }

            // ---------- Factory ----------
            public Builder RegisterFactory<TFactory>(string key)
                where TFactory : ChoMiniSequenceFactory, new()
            {
                _c._factories[key] = () => new TFactory();
                return this;
            }

            // ---------- Core ----------
            public Builder UseNodeRunner(Func<ChoMiniNodeRunner> f)
            {
                _c._nodeRunnerFactory = f;
                return this;
            }

            public Builder UseOrchestrator(Func<ChoMiniOrchestrator> f)
            {
                _c._orchestratorFactory = f;
                return this;
            }

            public ChoMiniContainer Build()
            {
                _c._nodeRunnerFactory ??= () => new ChoMiniNodeRunner();
                _c._orchestratorFactory ??= () =>
                    new ChoMiniOrchestrator(new ChoMiniNodeRunner());

                return _c;
            }
        }


        public abstract class BaseChoMiniRegisterBuilder
        {
            protected readonly Builder _parent;
            protected readonly List<string> _conditions = new List<string>();

            protected BaseChoMiniRegisterBuilder(Builder parent)
            {
                _parent = parent;
            }

            public BaseChoMiniRegisterBuilder When(string condition)
            {
                _conditions.Add(condition);
                return this;
            }

            // 앞으로 .Not() .Until() .Except() 등도 여기에 추가하면 됨.

            public abstract Builder End();
        }
    }
}