

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{

    public sealed class ChoMiniGameObjectInstaller
    {
        private GameObject _gameObject;

        public void Bind(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public List<Transform> InstallTargets()
        {
            if (_gameObject == null)
                throw new Exception("Installer not bound");

            var list = new List<Transform>();

            foreach (Transform child in
                     _gameObject.transform.GetComponentsInChildren<Transform>(true))
            {
                if (child != _gameObject)
                    list.Add(child);
            }

            return list;
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