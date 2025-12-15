using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniContainer
    {
        private readonly List<InstallerRule> _installerRules = new();

        private readonly Dictionary<Type, object> _installerBaseOptions = new Dictionary<Type, object>();

        private ChoMiniContainer() {}

        // 빌더 시작
        public static Builder Create()
        {
            return new Builder();
        }

        internal void AddRule(InstallerRule rule)
        {
            _installerRules.Add(rule);
        }



        // 디버그 출력용
        public void DebugPrint()
        {
            foreach (var type in _installerTypes)
            {
                _installerBaseOptions.TryGetValue(type, out var baseOpt);
                Debug.Log($"Installer: {type.Name}, Base: {baseOpt}");
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

                _container.AddRule(new InstallerRule
                {
                    InstallerType = typeof(TInstaller),
                    Kind = RuleKind.Base,
                    Key = null
                });

                _hasBase = true;
                return this;
            }

            public InstallerBuilder<TInstaller> Override(object key)
            {

            }

            public Builder Base<TOption>(TOption option)
            {
                _container.RegisterBaseOption(typeof(TInstaller), option);
                return _builder;
            }
        }
        // ChoMiniContainer 내부
        private readonly List<Type> _installerTypes = new();

        internal void RegisterInstallerType(Type installerType)
        {
            _installerTypes.Add(installerType);
        }

        internal void RegisterBaseOption(Type installerType, object baseOption)
        {
            _installerBaseOptions[installerType] = baseOption;
        }
    }

    public enum RuleKind
    {
        Base,
        Override
    }

    public sealed class InstallerRule
    {
        public Type InstallerType;   // ChoMiniStringSourceInstaller
        public RuleKind Kind;        // Base / Override
        public object? Key;          // Override만 사용
    }

}


