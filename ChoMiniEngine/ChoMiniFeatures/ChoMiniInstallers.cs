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
        List<List<object>> BuildPayload(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options
        );
    }


    // ======================================================
    // GameObject Installer
    // ======================================================
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

            ChoMiniSequenceOrder order;

            if (root.TryGetComponent<ChoMiniSequenceOrder>(out order))
            {
                _gameObjects = order.sequenceRoots;
            }
            else
            {
                _gameObjects = TreeToList(root);
            }
        }

        // ------------------------------
        // BuildPayload (엔진 경계)
        // ------------------------------
        public List<List<object>> BuildPayload(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            Debug.Log("[Installer] BuildPayload: GameObject");

            if (_gameObjects == null)
                throw new InvalidOperationException(
                    "Bind() must be called before BuildPayload()"
                );

            List<List<GameObject>> groups = BuildGameObjectGroups();
            var payload = new List<List<object>>();

            foreach (var group in groups)
            {
                var step = new List<object>();

                foreach (var go in group)
                {
                    if (go != null)
                        step.Add(go);
                }

                if (step.Count > 0)
                    payload.Add(step);
            }

            Debug.Log("[Installer] Payload Steps = " + payload.Count);
            return payload;
        }

        // ------------------------------
        // 내부: GameObject → 그룹
        // ------------------------------
        private List<List<GameObject>> BuildGameObjectGroups()
        {
            var groups = new List<List<GameObject>>();

            foreach (GameObject go in _gameObjects)
            {
                if (go == null)
                    continue;

                if (go.name == "Parallel")
                {
                    var merged = new List<GameObject>();

                    foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                    {
                        merged.Add(t.gameObject);
                    }

                    if (merged.Count > 0)
                        groups.Add(merged);
                }
                else
                {
                    var single = new List<GameObject>();
                    single.Add(go);
                    groups.Add(single);
                }
            }

            return groups;
        }

        // ------------------------------
        // 내부: Tree → List
        // ------------------------------
        private List<GameObject> TreeToList(GameObject root)
        {
            var list = new List<GameObject>();

            list.Add(root);

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
        // BuildPayload
        // ------------------------------
        public List<List<object>> BuildPayload(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            Debug.Log("[Installer] BuildPayload: String");

            if (_lines == null)
                throw new InvalidOperationException(
                    "Bind() must be called before BuildPayload()"
                );

            var payload = new List<List<object>>();

            foreach (string line in _lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var step = new List<object>();
                step.Add(line);
                payload.Add(step);
            }

            Debug.Log("[Installer] Payload Steps = " + payload.Count);
            return payload;
        }
    }

}