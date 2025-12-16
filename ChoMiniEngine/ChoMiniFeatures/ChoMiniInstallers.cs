

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniSequenceOrder : MonoBehaviour
    {

        public List<GameObject> sequenceRoots;

    }

    public sealed class ChoMiniGameObjectInstaller
    {
        private List<GameObject> _gameObjects;

        public void Bind(GameObject root)
        {
            if (root.TryGetComponent<ChoMiniSequenceOrder>(out var order))
            {
                _gameObjects = order.sequenceRoots;
            }
            else
            {
                _gameObjects = TreeToList(root);
            }
 
        }
        private List<GameObject> TreeToList(GameObject root)
        {
            var groups = new List<GameObject>();


            if (root == null)
                return groups;

            // 1) 자기 자신 포함
            groups.Add(root);

            // 2) 1차 자식들만 추가
            var transform = root.transform;
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                groups.Add(transform.GetChild(i).gameObject);
            }


            return groups;
        }

        public List<object> InstallObjectGroups()
        {
            if (_gameObjects == null)
                throw new InvalidOperationException(
                    "Bind() must be called before InstallObjectGroups()"
                );

            var groups = new List<object>();

            foreach (GameObject child in _gameObjects)
            {
                if (child.name == "Parallel")
                {
                    var merged = new List<GameObject>();

                    foreach (Transform t in child.GetComponentsInChildren<Transform>(true))
                    {
                        // Parallel 자신 포함 여부는 현재 설계 유지
                        merged.Add(t.gameObject);
                    }

                    groups.Add(merged);
                }
                else
                {
                    // 단일도 List<GameObject>로 감싸서 형태 통일
                    groups.Add(new List<GameObject>
                {
                    child
                });
                }
            }

            return groups;
        }

    }


    public sealed class ChoMiniStringInstaller
    {
        private string _text;

        public void Bind(string text)
        {
            _text = text;
        }

        
    }

}