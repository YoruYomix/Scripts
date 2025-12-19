using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// Executes a single node within a living Scope.
    /// Disposing this runner means the execution is terminated permanently.
    /// </summary>
    public sealed class ChoMiniNodeRunner : IDisposable
    {
        private ChoMiniNode _currentNode;
        private bool _paused;
        private bool _disposed;
        private bool _stopped;

        public void Pause()
        {
            ThrowIfDisposed();

            _paused = true;

            if (_currentNode == null)
                return;

            foreach (var action in _currentNode.Actions)
            {
                action.Pause();
            }
        }

        public void Resume()
        {
            ThrowIfDisposed();

            _paused = false;

            if (_currentNode == null)
                return;

            foreach (var action in _currentNode.Actions)
            {
                action.Resume();
            }
        }

        public async UniTask RunNode(ChoMiniNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            ThrowIfDisposed();

            // Guard: already running
            if (_currentNode != null)
                throw new InvalidOperationException(
                    "ChoMiniNodeRunner is already running a node.");

            // Guard: paused state
            if (_paused)
                throw new InvalidOperationException(
                    "Cannot start RunNode while runner is paused.");

            _currentNode = node;

            try
            {
                foreach (var action in node.Actions)
                {
                    action.Play();
                }

                float time = 0f;

                while (time < node.Duration)
                {
                    if (_disposed)
                        return; // Dispose = 즉시 종료

                    if (!_paused)
                        time += Time.deltaTime;

                    await UniTask.Yield();
                }

                if (_disposed)
                    return;

                foreach (var action in node.Actions)
                {
                    action.Complete();
                }
            }
            finally
            {
                _currentNode = null;
            }
        }
        public void Stop()
        {
            if (_disposed) return;
            if (_stopped) return;

            _stopped = true;
            _paused = false;

            if (_currentNode != null)
            {
                // ⭐ Stop에서는 Complete 금지
                foreach (var action in _currentNode.Actions)
                {
                    if (action is IChoMiniStoppableAction stoppable)
                        stoppable.Stop();
                }

                _currentNode = null;
            }
        }
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _paused = false;

            if (_currentNode != null)
            {
                CompleteCurrentNode();
                _currentNode = null;
            }
        }
        private void CompleteCurrentNode()
        {
            foreach (var action in _currentNode.Actions)
            {
                action.Complete();
            }
        }
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new InvalidOperationException("Runner already disposed.");
        }
    }
}
