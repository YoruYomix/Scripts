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

    public class ChoMiniRandomFactory : IChoMiniFactory
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

}