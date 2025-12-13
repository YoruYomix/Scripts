

using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    /// 인스톨러
    /// 
    public interface IChoMiniInstaller
    {
        public List<Transform> InstallTargets();
    };
    public class ChoMiniGameObjectSourceInstaller : IChoMiniInstaller
    {
        private readonly Transform _root;
        public ChoMiniGameObjectSourceInstaller(Transform root)
        {
            _root = root;
        }

        public List<Transform> InstallTargets()
        {
            List<Transform> result = new List<Transform>();

            foreach (var t in YoruUtilitys.GetAllTransforms(_root))
            {
                if (t == _root) continue;
                result.Add(t);
            }
            return result;
        }
    }

}