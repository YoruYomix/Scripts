using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniContainer
    {
        private readonly List<BootRule> _installerRules = new();
        private readonly List<BootRule> _factoryRules = new();
        private readonly List<BootRule> _providerRules = new();

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
        public ChoMiniLifetimeScope CreateScope(ChoMiniOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            return new ChoMiniLifetimeScope(
                    installerRules: _installerRules,
                    factoryRules: _factoryRules,
                    providerRules: _providerRules,
                    options: options
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

            public FactoryBuilder<TCategory> RegisterProvider<TCategory>()
            {
                return new FactoryBuilder<TCategory>(_container, this);
            }


            public ChoMiniContainer Build()
            {
                return _container;
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
        // Factory Builder
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
}




