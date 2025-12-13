using System;
using System.Collections.Generic;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniContainer
    {
        internal readonly List<ChoMiniInstallerRule> _installerRules = new();
        internal readonly Dictionary<string, Func<ChoMiniSequenceFactory>> _factories = new();
        internal readonly List<Func<IChoMiniActionProvider>> _providers = new();

        private Func<ChoMiniNodeRunner> _nodeRunnerFactory;
        private Func<ChoMiniOrchestrator> _orchestratorFactory;

        private ChoMiniContainer() { }


        public IInstallerKeyResolver CreateInstallerKeyResolver()
        {
            return new InstallerKeyResolver(_installerRules);
        }



        // =========================
        // Scope Creation
        // =========================
        public ChoMiniLifetimeScope CreateScope()
        {
            var resolver = CreateInstallerKeyResolver();

            // ⚠ 임시: Installer 인스턴스 생성용
            var rule = ResolveInstallerRule(out string installerKey);

            var installer =
                (IChoMiniInstaller)Activator.CreateInstance(rule.InstallerType);

            var orchestrator = _orchestratorFactory();

            return new ChoMiniLifetimeScope(
                resolver,
                installer,
                orchestrator,
                _factories,
                _providers,
                ChoMiniEngine.CommandContext
                );
        }



        private ChoMiniInstallerRule ResolveInstallerRule(out string key)
        {
            foreach (var rule in _installerRules)
            {
                foreach (var (cond, k) in rule.Entries)
                {
                    if (cond())
                    {
                        key = k;
                        return rule;
                    }
                }
            }

            throw new Exception("No Installer matched (Strict)");
        }

        // =========================
        // Builder
        // =========================
        public sealed class Builder
        {
            private readonly ChoMiniContainer _c = new();

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

            public Builder RegisterProvider<TProvider>()
                where TProvider : IChoMiniActionProvider, new()
            {
                _c._providers.Add(() => new TProvider());
                return this;
            }

            public Builder RegisterFactory<TFactory>(string key)
                where TFactory : ChoMiniSequenceFactory, new()
            {
                _c._factories[key] = () => new TFactory();
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

    public interface IInstallerKeyResolver
    {
        string Resolve();
    }
    internal sealed class InstallerKeyResolver : IInstallerKeyResolver
    {
        private readonly List<ChoMiniInstallerRule> _rules;

        public InstallerKeyResolver(List<ChoMiniInstallerRule> rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string Resolve()
        {
            foreach (var rule in _rules)
            {
                foreach (var (cond, key) in rule.Entries)
                {
                    if (cond())
                        return key;
                }
            }

            throw new Exception("No Installer matched (InstallerKeyResolver)");
        }
    }
}


