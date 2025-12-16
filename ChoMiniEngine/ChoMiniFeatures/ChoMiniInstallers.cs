using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniSequenceOrder : MonoBehaviour
    {

        public List<GameObject> sequenceRoots;

    }

    public interface IChoMiniInstaller
    {
        List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options
        );
    }


    public sealed class ChoMiniGameObjectInstaller : IChoMiniInstaller
    {
        private List<GameObject> _gameObjects;

        // ------------------------------
        // Bind
        // ------------------------------
        public void Bind(GameObject root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (root.TryGetComponent(out ChoMiniSequenceOrder order))
            {
                _gameObjects = order.sequenceRoots;
            }
            else
            {
                _gameObjects = TreeToList(root);
            }
        }

        // ------------------------------
        // BuildNodeSources (정식)
        // ------------------------------
        public List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            Debug.Log("[Installer] BuildNodeSources: GameObject");

            if (_gameObjects == null)
                throw new InvalidOperationException(
                    "Bind() must be called before BuildNodeSources()"
                );

            List<List<GameObject>> groups = BuildGameObjectGroups();
            List<NodeSource> result = new();

            foreach (List<GameObject> group in groups)
            {
                List<object> items = new();

                foreach (GameObject go in group)
                {
                    if (go != null)
                        items.Add(go);
                }

                if (items.Count > 0)
                    result.Add(new NodeSource(items));
            }

            Debug.Log("[Installer] NodeSource Steps = " + result.Count);
            return result;
        }

        // ------------------------------
        // 내부: GameObject → 그룹
        // ------------------------------
        private List<List<GameObject>> BuildGameObjectGroups()
        {
            List<List<GameObject>> groups = new();

            foreach (GameObject go in _gameObjects)
            {
                if (go == null)
                    continue;

                if (go.name == "Parallel")
                {
                    List<GameObject> merged = new();

                    foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                    {
                        merged.Add(t.gameObject);
                    }

                    if (merged.Count > 0)
                        groups.Add(merged);
                }
                else
                {
                    groups.Add(new List<GameObject> { go });
                }
            }

            return groups;
        }

        // ------------------------------
        // 내부: Tree → List
        // ------------------------------
        private List<GameObject> TreeToList(GameObject root)
        {
            List<GameObject> list = new() { root };

            Transform t = root.transform;
            int count = t.childCount;

            for (int i = 0; i < count; i++)
            {
                list.Add(t.GetChild(i).gameObject);
            }

            return list;
        }
    }



    // ======================================================
    // String Installer (예문용 단순형)
    // ======================================================
    public sealed class ChoMiniStringInstaller : IChoMiniInstaller
    {
        private string[] _lines;

        // ------------------------------
        // Bind
        // ------------------------------
        public void Bind(string[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            _lines = lines;
        }

        // ------------------------------
        // BuildNodeSources
        // ------------------------------
        public List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            Debug.Log("[Installer] BuildNodeSources: String");

            if (_lines == null)
                throw new InvalidOperationException(
                    "Bind() must be called before BuildNodeSources()"
                );

            List<NodeSource> result = new List<NodeSource>();

            foreach (string line in _lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                // 한 줄 = 한 step
                List<object> items = new List<object>
            {
                line
            };

                result.Add(new NodeSource(items));
            }

            Debug.Log("[Installer] NodeSource Steps = " + result.Count);
            return result;
        }
    }


}