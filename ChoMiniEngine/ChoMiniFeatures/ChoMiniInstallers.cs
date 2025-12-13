

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    public interface IInstallerResource
    {
    }
    /// 인스톨러
    /// 
    public interface IChoMiniInstaller
    {
        void Bind(IInstallerResource resource);
        List<Transform> InstallTargets();
    };
    public class ChoMiniGameObjectInstallerResource : IInstallerResource
    {
        public Transform Root
        {
            get
            {
                return _root;
            }
        }
        public Transform _root;
        public ChoMiniGameObjectInstallerResource(Transform root)
        {
            _root = root;
        }
    }

    public sealed class ChoMiniGameObjectSourceInstaller
    : IChoMiniInstaller
    {
        private ChoMiniGameObjectInstallerResource _resource;

        public void Bind(IInstallerResource resource)
        {
            _resource = resource as ChoMiniGameObjectInstallerResource
                ?? throw new Exception(
                    "Invalid resource for GameObjectInstaller");
        }

        public List<Transform> InstallTargets()
        {
            if (_resource == null)
                throw new Exception("Installer not bound");

            var list = new List<Transform>();

            foreach (Transform child in
                     _resource.Root.GetComponentsInChildren<Transform>(true))
            {
                if (child != _resource.Root)
                    list.Add(child);
            }

            return list;
        }
    }

}