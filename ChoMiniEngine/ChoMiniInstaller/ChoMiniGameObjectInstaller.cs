using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Yoru.ChoMiniEngine
{
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
        // BuildNodeSources
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

            // ⭐ 텍스트 컴포넌트를 가진 마지막 GameObject 찾기
            GameObject lastTextObject = FindLastTextGameObject(groups);

            foreach (List<GameObject> group in groups)
            {
                List<object> items = new();

                foreach (GameObject go in group)
                {
                    if (go != null)
                        items.Add(go);
                }

                if (items.Count == 0)
                    continue;

                bool isLastTextNode =
                    lastTextObject != null &&
                    group.Contains(lastTextObject);

                if (isLastTextNode)
                    result.Add(new NodeSource(items, "last-textNode"));
                else
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

        // ------------------------------
        // 내부: 마지막 텍스트 GameObject 찾기
        // ------------------------------
        private GameObject FindLastTextGameObject(List<List<GameObject>> groups)
        {
            GameObject last = null;

            foreach (var group in groups)
            {
                foreach (var go in group)
                {
                    if (go == null)
                        continue;

                    if (go.GetComponent<Text>() != null ||
                        go.GetComponent<TMP_Text>() != null)
                    {
                        last = go;
                    }
                }
            }

            return last;
        }
    }
}
