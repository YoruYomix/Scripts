using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

/// <summary>
/// 여러 종류의 게임 오브젝트를 이름 기반으로 로드/언로드하고,
/// 런타임에서 메모리까지 안전하게 관리하는 범용 매니저
/// </summary>
public class ObjectMemoryManager : MonoBehaviour
{
    [Header("Inspector에서 다양한 오브젝트 프리팹 다중 할당")]
    [SerializeField] private List<GameObject> objectPrefabs;

    // 런타임에서 생성된 오브젝트 인스턴스를 이름으로 관리
    private Dictionary<string, GameObject> objectInstances = new Dictionary<string, GameObject>();

    /// <summary>
    /// 이름으로 오브젝트 로드 (중복 로드 방지)
    /// </summary>
    public async UniTask LoadObjectAsync(string objectName)
    {
        // 이미 로드된 오브젝트가 있으면 중복 로드 방지
        if (objectInstances.ContainsKey(objectName))
        {
            Debug.Log($"Object '{objectName}' is already loaded!");
            return;
        }

        // 인스펙터에 등록된 프리팹 중 이름으로 찾기
        GameObject prefab = objectPrefabs.Find(p => p != null && p.name == objectName);
        if (prefab == null)
        {
            Debug.LogError($"Object prefab '{objectName}' not found in Inspector!");
            return;
        }

        // 인스턴스 생성
        GameObject instance = Instantiate(prefab);
        objectInstances[objectName] = instance;

        await UniTask.Yield(); // 다음 프레임까지 대기
        Debug.Log($"Object '{objectName}' instantiated!");
    }

    /// <summary>
    /// 이름으로 오브젝트 언로드 및 메모리 해제
    /// </summary>
    public async UniTask UnloadObjectAsync(string objectName)
    {
        if (objectInstances.TryGetValue(objectName, out GameObject instance) && instance != null)
        {
            // 씬에서 제거
            Destroy(instance);
            objectInstances.Remove(objectName);

            // 사용되지 않는 리소스 해제
            await Resources.UnloadUnusedAssets().ToUniTask();
            GC.Collect();

            Debug.Log($"Object '{objectName}' memory cleared!");
        }
        else
        {
            Debug.Log($"Object '{objectName}' is not loaded, nothing to unload.");
        }
    }

    /// <summary>
    /// 모든 오브젝트 언로드
    /// </summary>
    public async UniTask UnloadAllObjectsAsync()
    {
        foreach (var kvp in new Dictionary<string, GameObject>(objectInstances))
        {
            await UnloadObjectAsync(kvp.Key);
        }
    }

    /// <summary>
    /// 특정 오브젝트 로드 여부 확인
    /// </summary>
    public bool IsObjectLoaded(string objectName)
    {
        return objectInstances.ContainsKey(objectName);
    }
}
