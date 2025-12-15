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
            public ImplementationBuilder<TCategory> RegisterFactory<TCategory>()
            {
                return new ImplementationBuilder<TCategory>(_container,this);
            }

            public ImplementationBuilder<TCategory> RegisterProvider<TCategory>()
            {
                return new ImplementationBuilder<TCategory>(_container, this);
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
        // Implementation Builder
        // ======================================================
        public sealed class ImplementationBuilder<TCategory>
        {
            private readonly ChoMiniContainer _container;
            private readonly Builder _builder;
            private bool _hasBase;

            internal ImplementationBuilder(ChoMiniContainer container, Builder builder)
            {
                _container = container;
                _builder = builder;
            }

            public ImplementationBuilder<TCategory> Base<TImpl>()
                where TImpl : TCategory
            {
                EnsureBaseOnce();
                _container.AddRule(new BootRule
                {
                    Category = typeof(TCategory),
                    ImplType = typeof(TImpl),
                    Kind = RuleKind.Base,
                    Key = null
                });
                return this;
            }

            public ImplementationBuilder<TCategory> Override<TImpl>(object key)
                where TImpl : TCategory
            {
                _container.AddRule(new BootRule
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




