using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public static class SubSceneLoaderUniTask
{
    private static bool isLoading = false;

    /// <summary>
    /// 서브씬 Additive 로드
    /// </summary>
    public static async UniTask LoadSubSceneAsync(string sceneName)
    {
        if (isLoading) return;

        if (SceneManager.GetSceneByName(sceneName).isLoaded)
            return;

        isLoading = true;

        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!loadOp.isDone)
        {
            await UniTask.Yield(); // 프레임마다 대기
        }

        isLoading = false;
    }

    /// <summary>
    /// 서브씬 언로드
    /// </summary>
    public static async UniTask UnloadSubSceneAsync(string sceneName)
    {
        if (isLoading) return;

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            return;

        isLoading = true;

        var unloadOp = SceneManager.UnloadSceneAsync(sceneName);

        while (!unloadOp.isDone)
        {
            await UniTask.Yield();
        }

        isLoading = false;
    }
}
