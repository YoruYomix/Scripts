using UnityEngine;
using System.IO;

public static class SaveLoadManager
{
    private static string saveFileName = "gameSave.json";
    private static string savePath => Path.Combine(Application.persistentDataPath, saveFileName);

    // 저장 메서드
    public static void SaveGame(SaveData dataToSave)
    {
        // 1. SaveData 객체를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(dataToSave);

        try
        {
            // 2. 파일에 JSON 문자열 쓰기
            File.WriteAllText(savePath, json);
            Debug.Log($"저장비동기 성공： {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장비동기 실패： {e.Message}");
        }
    }

    // 불러오기 메서드
    public static SaveData LoadGame()
    {
        // 1. 파일이 존재하는지 확인
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"저장비동기 파일이 존재하지 않습니다： {savePath}");
            // 파일이 없으면 기본 데이터로 초기화하여 반환 (예： 섹션 10개)
            return new SaveData(10);
        }

        try
        {
            // 2. 파일에서 JSON 문자열 읽기
            string json = File.ReadAllText(savePath);

            // 3. JSON 문자열을 SaveData 객체로 변환
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"불러오기 성공： {savePath}");
            return loadedData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"불러오기 실패： {e.Message}");
            // 불러오기 실패 시 기본 데이터 반환 (데이터 손상 방지)
            return new SaveData(10);
        }
    }

    // 초기화 메서드 (선택 사항)
    public static void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"세이브 파일 삭제됨： {savePath}");
        }
    }
}