using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{

    public interface IChoMiniFactory { }
    // 팩토리
    public class ChoMiniSequenceFactory : IChoMiniFactory
    {
        List<List<object>> _targetObjectGroups;

        private List<ChoMiniProvider> _providers;  // Lazy-created, cached per scope

        LoopProvider _mockLoopProvider;
        int _index = 0;
        ISubscriber<ChoMiniLocalSkipRequested> _skipSubscriber;
        public int Count => _targetObjectGroups?.Count ?? 0;



        public void Initialize(
            List<List<object>> targetObjectGroups,
            List<ChoMiniProvider> providers,
            ISubscriber<ChoMiniLocalSkipRequested> skipSubscriber)
        {
            if (targetObjectGroups == null)
                throw new ArgumentNullException(nameof(targetObjectGroups));

            _targetObjectGroups = targetObjectGroups;
            _providers = providers;
            _skipSubscriber = skipSubscriber;

            Debug.Log($"팩토리 타겟 그룹 수: {_targetObjectGroups.Count}");
        }

        public ChoMiniNode Create()
        {
            if (_targetObjectGroups == null || _targetObjectGroups.Count == 0)
                throw new InvalidOperationException(
                    "ChoMiniSequenceFactory is not initialized or has no target groups."
                );

            // 1) 현재 시퀀스 스텝(payload) 선택
            var payload = _targetObjectGroups[_index];
            _index = (_index + 1) % _targetObjectGroups.Count;

            // 2) Node 생성 (payload 그대로 보관)
            ChoMiniNode node = new ChoMiniNode(_skipSubscriber, payload);

            // 3) Provider들에게 payload 그대로 전달
            foreach (var item in payload)
            {
                if (item == null) continue;

                foreach (var provider in _providers)
                {
                    if (provider == null) continue;
                        provider.CollectEffects(item, node);
                }
            }


            // 4) Node Duration 계산
            float maxDuration = 0f;
            foreach (var effect in node.Actions)
            {
                maxDuration = Mathf.Max(maxDuration, effect.GetRequiredDuration());
            }

            node.Duration = maxDuration;
            return node;
        }
    }


    public class ChoMiniRewindFactory : IChoMiniFactory
    {
        List<Transform> _targets;
        private List<Func<IChoMiniGameObjectActivationProvider>> _providerFactories;

        private List<IChoMiniGameObjectActivationProvider> _providers;  // Lazy-created, cached per scope

        LoopProvider _mockLoopProvider;
        int _index = 0;
        public int Count
        {
            get
            {
                Debug.Log(_targets);
                return _targets.Count;
            }

        }


        ISubscriber<ChoMiniLocalSkipRequested> _skipSubscriber;

        public ChoMiniRewindFactory()
        {
            _mockLoopProvider = new LoopProvider();  // 리팩토링중 임시
        }

        // ------------------------
        // Lazy Provider 초기화
        // ------------------------
        private void EnsureProviders()
        {
            if (_providers != null)
                return;

            _providers = new List<IChoMiniGameObjectActivationProvider>();

            if (_providerFactories == null)
                return; // 빈 Provider 목록으로 동작 가능

            foreach (var factory in _providerFactories)
                _providers.Add(factory());
        }


        public void Initialize(
            List<Transform> targets,
            List<Func<IChoMiniGameObjectActivationProvider>> providerFactories,
            ISubscriber<ChoMiniLocalSkipRequested> skipSubscriber)
        {
            Debug.Log("팩토리 타겟:" + targets);
            _targets = targets;
            _providerFactories = providerFactories;
            _skipSubscriber = skipSubscriber;

            // 테스트/실사용 모두 안정적
            EnsureProviders();
        }

        public ChoMiniNode Create()
        {
            var t = _targets[_index];
            _index = (_index + 1) % _targets.Count;

            GameObject go = t.gameObject;
            ChoMiniNode node = new ChoMiniNode(_skipSubscriber, go);
            Debug.Log("팩토리 내부의 크리에이트: " + go.name);


            // Provider가 Effects를 채운다
            foreach (var provider in _providers)
                provider.CollectEffects(go, node);

            // LoopProvider도 Effects를 채운다
            _mockLoopProvider.CollectEffects(go, node);

            // Duration 계산: 이벤트 리스트의 모든 듀레이션중 가장 큰 값이 노드 자체의 듀레이션 됨
            float maxDuration = 0f;
            foreach (var effect in node.Actions)
                maxDuration = Mathf.Max(maxDuration, effect.GetRequiredDuration());

            node.Duration = maxDuration;

            return node;
        }
    }

    public class ChoMiniRandomFactory : IChoMiniFactory
    {
        List<Transform> _targets;
        private List<Func<IChoMiniGameObjectActivationProvider>> _providerFactories;

        private List<IChoMiniGameObjectActivationProvider> _providers;  // Lazy-created, cached per scope

        LoopProvider _mockLoopProvider;
        int _index = 0;
        public int Count
        {
            get
            {
                Debug.Log(_targets);
                return _targets.Count;
            }

        }


        ISubscriber<ChoMiniLocalSkipRequested> _skipSubscriber;

        public ChoMiniRandomFactory()
        {
            _mockLoopProvider = new LoopProvider();  // 리팩토링중 임시
        }

        // ------------------------
        // Lazy Provider 초기화
        // ------------------------
        private void EnsureProviders()
        {
            if (_providers != null)
                return;

            _providers = new List<IChoMiniGameObjectActivationProvider>();

            if (_providerFactories == null)
                return; // 빈 Provider 목록으로 동작 가능

            foreach (var factory in _providerFactories)
                _providers.Add(factory());
        }


        public void Initialize(
            List<Transform> targets,
            List<Func<IChoMiniGameObjectActivationProvider>> providerFactories,
            ISubscriber<ChoMiniLocalSkipRequested> skipSubscriber)
        {
            Debug.Log("팩토리 타겟:" + targets);
            _targets = targets;
            _providerFactories = providerFactories;
            _skipSubscriber = skipSubscriber;

            // 테스트/실사용 모두 안정적
            EnsureProviders();
        }

        public ChoMiniNode Create()
        {
            var t = _targets[_index];
            _index = (_index + 1) % _targets.Count;

            GameObject go = t.gameObject;
            ChoMiniNode node = new ChoMiniNode(_skipSubscriber, go);
            Debug.Log("팩토리 내부의 크리에이트: " + go.name);


            // Provider가 Effects를 채운다
            foreach (var provider in _providers)
                provider.CollectEffects(go, node);

            // LoopProvider도 Effects를 채운다
            _mockLoopProvider.CollectEffects(go, node);

            // Duration 계산: 이벤트 리스트의 모든 듀레이션중 가장 큰 값이 노드 자체의 듀레이션 됨
            float maxDuration = 0f;
            foreach (var effect in node.Actions)
                maxDuration = Mathf.Max(maxDuration, effect.GetRequiredDuration());

            node.Duration = maxDuration;

            return node;
        }
    }

}