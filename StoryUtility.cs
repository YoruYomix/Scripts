using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class StoryUtility
{
    // 각 챕터별 섹션 배열
    private static readonly int[] chapter1Sections = { 1,2,3 };
    private static readonly int[] chapter2Sections = { 4,5 };
    private static readonly int[] chapter3Sections = { 6, 7 };

    // 섹션 번호 -> 챕터 이름 매핑
    private static readonly Dictionary<int, string> sectionToChapter;

    // 스태틱 생성자에서 Dictionary 초기화
    static StoryUtility()
    {
        sectionToChapter = new Dictionary<int, string>();

        foreach (var sec in chapter1Sections)
            sectionToChapter[sec] = "Chapter1";

        foreach (var sec in chapter2Sections)
            sectionToChapter[sec] = "Chapter2";

        foreach (var sec in chapter3Sections)
            sectionToChapter[sec] = "Chapter3";
    }

    /// <summary>
    /// 섹션 번호 입력 → 챕터 이름 반환
    /// </summary>
    public static string GetChapterName(int sectionIndex)
    {
        if (sectionToChapter.TryGetValue(sectionIndex, out string chapter))
            return chapter;

        return "Unknown"; // 범위 밖
    }

    public static bool IsSceneLoaded(string sceneName)
    {
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == sceneName)
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsSceneLoaded(int sectionIndex)
    {
        string sceneName = GetChapterName(sectionIndex);
        return IsSceneLoaded(sceneName);
    }
}
