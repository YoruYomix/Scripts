using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniComposer
    {
        void Compose();
    }

    /// <summary>
    /// "Installer 선택/바인딩"만 담당하는 최소 컴포저.
    /// - Resolve()는 매번 호출 가능
    /// - Key가 바뀔 때만 재바인딩
    /// - 아직 Factory/Provider 초기화는 하지 않음 (다음 단계)
    /// </summary>
    public sealed class ChoMiniInstallerComposer : IChoMiniComposer
    {
        private readonly IInstallerKeyResolver _resolver;
        private readonly IChoMiniInstaller _installer;
        private readonly Dictionary<string, IInstallerResource> _resources;

        private string _currentKey;
        private List<Transform> _latestTargets;

        public ChoMiniInstallerComposer(
            IInstallerKeyResolver resolver,
            IChoMiniInstaller installer,
            Dictionary<string, IInstallerResource> resources)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _installer = installer ?? throw new ArgumentNullException(nameof(installer));
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        /// <summary>가장 최근 InstallTargets 결과</summary>
        public IReadOnlyList<Transform> LatestTargets => _latestTargets;

        /// <summary>가장 최근 적용된 Installer Key</summary>
        public string CurrentKey => _currentKey;

        public void Compose()
        {
            var nextKey = _resolver.Resolve();

            // 동일 Key면 아무것도 안 함
            if (_currentKey == nextKey)
                return;

            if (!_resources.TryGetValue(nextKey, out var resource))
                throw new Exception($"Resource not bound for installer '{nextKey}'");

            _installer.Bind(resource);
            _latestTargets = _installer.InstallTargets();

            _currentKey = nextKey;
        }
    }
}
