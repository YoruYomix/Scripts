using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using static Yoru.ChoMiniEngine.ChoMiniContainer;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniContainer
    {
        private readonly List<BootRule> _installerRules = new();
        private readonly List<BootRule> _factoryRules = new();
        private readonly List<BootRule> _providerRules = new();
        private readonly List<ReactorRule> _reactorRules = new();



        internal void RegisterInstallerType(Type installerType)
        {
            _installerTypes.Add(installerType);
        }
        // ChoMiniContainer 내부
        private readonly List<Type> _installerTypes = new();

        private ChoMiniContainer() {}

        // 빌더 시작
        public static Builder Create()
        {
            return new Builder();
        }

        internal void AddInstallerRulesRule(BootRule rule)
        {
            _installerRules.Add(rule);
        }
        internal void AddFactoryRule(BootRule rule)
        {
            _factoryRules.Add(rule);
        }
        internal void AddProviderRules(BootRule rule)
        {
            _providerRules.Add(rule);
        }
        internal void AddReactorRule(ReactorRule rule)
        {
            _reactorRules.Add(rule);
        }
        public ChoMiniLifetimeScope CreateScope(ChoMiniOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");


            ChoMiniScopeMessageContext localMsg = new ChoMiniScopeMessageContext();
            ChoMiniNodeRunner nodeRunner = new ChoMiniNodeRunner();
            ChoMiniOrchestrator orchestrator = new ChoMiniOrchestrator(nodeRunner);

            return new ChoMiniLifetimeScope(
                    installerRules: _installerRules,
                    factoryRules: _factoryRules,
                    providerRules: _providerRules,
                    reactorRules: _reactorRules, 
                    options: options,
                    localMsg: localMsg,
                    orchestrator: orchestrator
                );
        }



        // 디버그 출력용
        public void DebugPrint()
        {
            Debug.Log("[ChoMiniContainer Rules]");

            foreach (var group in _providerRules.GroupBy(r => r.Category))
            {
                Debug.Log($"Category: {group.Key.Name}");

                foreach (var rule in group)
                {
                    string kind = rule.Kind == RuleKind.Base
                        ? "  Base"
                        : $"  Override({rule.Key})";

                    // ImplType이 있는 경우 (Factory / Provider)
                    if (rule.ImplType != null)
                    {
                        Debug.Log($"{kind} -> {rule.ImplType.Name}");
                    }
                    // ImplType이 없는 경우 (Installer)
                    else
                    {
                        Debug.Log(kind);
                    }
                }
            }
        }


        //// 빌더
        /// 
        public class Builder
        {
            private ChoMiniContainer _container = new ChoMiniContainer();
            // ChoMiniContainer 내부



            // 인스톨러 등록
            public InstallerBuilder<TInstaller> RegisterInstaller<TInstaller>()
            {
                _container.RegisterInstallerType(typeof(TInstaller));
                return new InstallerBuilder<TInstaller>(_container,this);  // 체이닝
            }

            // 팩토리 등록
            public FactoryBuilder<TCategory> RegisterFactory<TCategory>()
            {
                return new FactoryBuilder<TCategory>(_container,this);
            }
            // 프로바이더 등록
            public ProviderBuilder<TCategory> RegisterProvider<TCategory>()
            {
                return new ProviderBuilder<TCategory>(_container, this);
            }

            // 리액터 등록
            public SimpleReactorBuilder RegisterReactor()
            {
                return new SimpleReactorBuilder(_container, this);
            }
            public ReactorBuilder<TProvider> RegisterReactor<TProvider>()
            {
                return new ReactorBuilder<TProvider>(_container, this);
            }
            public ChoMiniContainer Build()
            {
                return _container;
            }
        }
        // Provider 없는 Reactor

        public sealed class SimpleReactorBuilder
        {
            private readonly ChoMiniContainer _container;
            private readonly ChoMiniContainer.Builder _builder;
            private readonly ReactorRule _rule;

            internal SimpleReactorBuilder(
                ChoMiniContainer container,
                ChoMiniContainer.Builder builder)
            {
                _container = container;
                _builder = builder;

                _rule = new ReactorRule
                {
                    ProviderType = null // ⭐ 핵심
                };
            }

            // -------------------------
            // When
            // -------------------------

            public SimpleReactorBuilder WhenSequenceCompleted
            {
                get
                {
                    _rule.ScheduleConditions.Add(new OnSequenceCompletedCondition());
                    return this;
                }
            }

            public SimpleReactorBuilder When(Func<bool> predicate)
            {
                _rule.ScheduleConditions.Add(new ExternalPredicateCondition(predicate));
                return this;
            }


            // -------------------------
            // Do
            // -------------------------

            public ChoMiniContainer.Builder Do(Action action)
            {
                _rule.DoHook = action;
                _container.AddReactorRule(_rule);
                return _builder;
            }

            public ChoMiniContainer.Builder Do()
            {
                return Do(() => { });
            }
        }


        public sealed class ReactorBuilder<TProvider>
        {
            private readonly ChoMiniContainer _container;
            private readonly ChoMiniContainer.Builder _builder;

            private readonly ReactorRule _rule;

            internal ReactorBuilder(
                ChoMiniContainer container,
                ChoMiniContainer.Builder builder)
            {
                _container = container;
                _builder = builder;

                _rule = new ReactorRule
                {
                    ProviderType = typeof(TProvider)
                };
            }

            // -------------------------
            // When 계열
            // -------------------------

            public ReactorBuilder<TProvider> WhenSequenceCompleted
            {
                get
                {
                    _rule.ScheduleConditions.Add(
                        new OnSequenceCompletedCondition());
                    return this;
                }
            }



            public ReactorBuilder<TProvider> When(Func<bool> predicate)
            {
                _rule.ScheduleConditions.Add(
                    new ExternalPredicateCondition(predicate));
                return this;
            }

            // -------------------------
            // Target 계열
            // -------------------------

            public ReactorBuilder<TProvider> TargetNodeTag(string tag)
            {
                _rule.NodeConditions.Add(
                    new NodeTagCondition(tag));
                return this;
            }

            // -------------------------
            // Lifetime
            // -------------------------

            public ReactorBuilder<TProvider> LifetimeLoop()
            {
                _rule.IsLifetimeLoop = true;
                return this;
            }

            // -------------------------
            // Do (외부 훅)
            // -------------------------

            public ChoMiniContainer.Builder Do(Action action)
            {
                _rule.DoHook = action;
                _container.AddReactorRule(_rule);
                return _builder;
            }
            public ChoMiniContainer.Builder Do()
            {
                return Do(() => { });
            }
        }

        public sealed class InstallerBuilder<TInstaller>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal InstallerBuilder(
                ChoMiniContainer container,
                Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public InstallerBuilder<TInstaller> Base()
            {
                if (_hasBase)
                {
                    throw new InvalidOperationException("Base() already defined.");
                }

                _container.AddInstallerRulesRule(new BootRule
                {
                    Category  = typeof(TInstaller),
                    Kind = RuleKind.Base,
                    Key = null
                });

                _hasBase = true;
                return this;
            }

            public InstallerBuilder<TInstaller> Override(object key)
            {
                _container.AddInstallerRulesRule(new BootRule
                {
                    Category  = typeof(TInstaller),
                    Kind = RuleKind.Override,
                    Key = key

                });
                return this;
            }

            public Builder End()
            {
                if (!_hasBase)
                {
                    throw new InvalidOperationException(
                            $"Installer {typeof(TInstaller).Name} requires Base().");
                }
                return _builder;
            }
        }
        // ======================================================
        // Factory Builder
        // ======================================================
        public sealed class FactoryBuilder<TCategory>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal FactoryBuilder(ChoMiniContainer container, Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public FactoryBuilder<TCategory> Base<TImpl>()
                where TImpl : TCategory
            {
                EnsureBaseOnce();
                _container.AddFactoryRule(new BootRule
                {
                    Category = typeof(TCategory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Base,
                    Key = null
                });
                return this;
            }

            public FactoryBuilder<TCategory> Override<TImpl>(object key)
                where TImpl : TCategory
            {
                _container.AddFactoryRule(new BootRule
                {
                    Category = typeof(TCategory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Override,
                    Key = key
                });
                return this;
            }

            public Builder End()
            {
                if (!_hasBase)
                    throw new InvalidOperationException(
                        $"{typeof(TCategory).Name} requires Base().");   
                return _builder;
            }
            private void EnsureBaseOnce()
            {
                if (_hasBase) throw new InvalidOperationException("Base already set.");
                _hasBase = true;
            }
        }

        // ======================================================
        // Provider Builder
        // ======================================================
        public sealed class ProviderBuilder<TCategory>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal ProviderBuilder(ChoMiniContainer container, Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public ProviderBuilder<TCategory> Base<TImpl>()
                where TImpl : TCategory
            {
                EnsureBaseOnce();
                _container.AddProviderRules(new BootRule
                {
                    Category = typeof(TCategory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Base,
                    Key = null
                });
                return this;
            }

            public ProviderBuilder<TCategory> Override<TImpl>(object key)
                where TImpl : TCategory
            {
                _container.AddProviderRules(new BootRule
                {
                    Category = typeof(TCategory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Override,
                    Key = key
                });
                return this;
            }

            public Builder End()
            {
                if (!_hasBase)
                    throw new InvalidOperationException(
                        $"{typeof(TCategory).Name} requires Base().");
                return _builder;
            }
            private void EnsureBaseOnce()
            {
                if (_hasBase) throw new InvalidOperationException("Base already set.");
                _hasBase = true;
            }
        }
    }

    // ======================================================
    // BootRule
    // ======================================================
    public sealed class BootRule
    {
        public Type Category;   // ChoMiniStringSourceInstaller
        public Type ImplType; // ChoMiniSequenceFactory 등
        public RuleKind Kind;        // Base / Override
        public object? Key;          // Override만 사용
    }
    public enum RuleKind
    {
        Base,
        Override
    }

    public sealed class ReactorRule
    {
        // 언제 실행할지 (Scheduler용)
        public readonly List<IReactorScheduleCondition> ScheduleConditions = new();

        // 무엇을 실행할지 (ReactorNodeFactory용)
        public readonly List<IReactorNodeCondition> NodeConditions = new();

        public Type? ProviderType;
        public bool IsLifetimeLoop;
        public Action DoHook;
    }
}




