

using System.Collections.Generic;

using UnityEngine;

/// 인스톨러
/// 
public interface IInstaller
{
    public List<Transform> InstallTargets();
};
public class UIRootInstaller : IInstaller
{
    private readonly Transform _root;
    public UIRootInstaller(Transform root)
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

