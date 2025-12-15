using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniContainer
    {
        private readonly List<BootRule> _rules  = new();

        private readonly Dictionary<Type, object> _installerBaseOptions = new Dictionary<Type, object>();
        internal void RegisterInstallerType(Type installerType)
        {
            _installerTypes.Add(installerType);
        }
        // ChoMiniContainer 내부
        private readonly List<Type> _installerTypes = new();
        internal void RegisterBaseOption(Type installerType, object baseOption)
        {
            _installerBaseOptions[installerType] = baseOption;
        }
        private ChoMiniContainer() {}

        // 빌더 시작
        public static Builder Create()
        {
            return new Builder();
        }

        internal void AddRule(BootRule rule)
        {
            _rules .Add(rule);
        }



        // 디버그 출력용
        public void DebugPrint()
        {
            Debug.Log("[ChoMiniContainer Rules]");

            foreach (var group in _rules.GroupBy(r => r.Category))
            {
                Debug.Log($"Category: {group.Key.Name}");

                foreach (var rule in group)
                {
                    Debug.Log(
                        rule.Kind == RuleKind.Base
                        ? "  Base()"
                        : $"  Override({rule.Key})"
                    );
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
            public FactoryBuilder<TFactory> RegisterFactory<TFactory>()
            {
                return new FactoryBuilder<TFactory>(_container,this);
            }

            public ProviderBuilder<TProvider> RegisterProvider<TProvider>()
            {
                return new ProviderBuilder<TProvider>(_container, this);
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

                _container.AddRule(new BootRule
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
                _container.AddRule(new BootRule
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
        public sealed class FactoryBuilder<TFactory>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal FactoryBuilder(ChoMiniContainer container, Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public FactoryBuilder<TFactory> Base<TImpl>()
                where TImpl : TFactory
            {
                EnsureBaseOnce();
                _container.AddRule(new BootRule
                {
                    Category = typeof(TFactory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Base,
                    Key = null
                });

                _hasBase = true;
                return this;
            }

            public FactoryBuilder<TFactory> Override<TImpl>(object key)
                where TImpl : TFactory
            {
                _container.AddRule(new BootRule
                {
                    Category = typeof(TFactory),
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
                        $"{typeof(TFactory).Name} requires Base().");   
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
        public sealed class ProviderBuilder<TProvider>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal ProviderBuilder(ChoMiniContainer container, Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public ProviderBuilder<TProvider> Base<TImpl>()
                where TImpl : TProvider
            {
                EnsureBaseOnce();
                _container.AddRule(new BootRule
                {
                    Category = typeof(TProvider),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Base,
                    Key = null
                });

                _hasBase = true;
                return this;
            }

            public ProviderBuilder<TProvider> Override<TImpl>(object key)
                where TImpl : TProvider
            {
                _container.AddRule(new BootRule
                {
                    Category = typeof(TProvider),
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
                        $"{typeof(TProvider).Name} requires Base().");
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




