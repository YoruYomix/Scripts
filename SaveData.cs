using System; // Serializable을 위해 필요

[Serializable]
public class SectionSaveData
{
    public bool clear;
    public bool unLocked;
}

[Serializable]
public class SaveData
{
    // 필드 정보
    public int lastSectionSaveData;
    public int lastPageSaveData;

    // 배열 형태의 섹션 세이브 데이터
    // 배열을 직렬화하려면 내부 클래스(SectionSaveData)에도 [Serializable]이 필요합니다.
    public SectionSaveData[] SectionData;

    // 생성자 (선택 사항이지만 초기화에 유용)
    public SaveData(int numSections)
    {
        lastSectionSaveData = 0;
        lastPageSaveData = 0;

        // 배열 크기 초기화
        SectionData = new SectionSaveData[numSections];
        for (int i = 0; i < numSections; i++)
        {
            SectionData[i] = new SectionSaveData { clear = false, unLocked = false };
        }
        // 첫 번째 섹션은 잠금 해제 상태로 시작할 수 있습니다.
        if (numSections > 0)
        {
            SectionData[0].unLocked = true;
        }
    }
}