using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SectionStatus
{
    Incomplete,
    Clear,
    Ending,
    Locked
}

public class Section
{
    public int Index { get; private set; }
    public string Chapter { get; private set; }
    private List<int> _choices = new List<int>();
    public IReadOnlyList<int> Choices => _choices; // 외부에서 읽기 전용

    private bool isClear; // 선택지 까지 진행을 했다
    private bool isUnlocked; // 선택된 적이 있다
    public int PreviousSection = -1;
    private Dictionary<int, Section> sectionLookup;
    public readonly SectionData sectionDefinition;

    public bool IsClear
    {
        get
        {
            return isClear ;
        }
        set
        {
            Debug.Log(isClear + value.ToString());
            if (isClear == value)
            {
                Debug.Log("isClear저장안함: " + isClear);
                return;
            }
            Debug.Log("isClear저장함: " + isClear);
            isClear = value;
            GameManager.Instance.currentSaveData.SectionData[Index].clear = isClear;
            GameManager.Instance.SaveCurrentStateToDisk();
        }
    }

    public bool IsUnlocked
    {
        get
        {
            return isUnlocked;
        }
        set
        {
            if (isUnlocked == value)
            {
                Debug.Log("isUnlocked 저장비동기 안함: " + isClear);
                return;
            }
            Debug.Log("isUnlocked 저장함: " + isClear);
            isUnlocked = value;
            GameManager.Instance.currentSaveData.SectionData[Index].unLocked = isUnlocked;
            GameManager.Instance.SaveCurrentStateToDisk();
        }
    }


    public Section(int index, int chapterIndex, Dictionary<int, Section> lookup, SectionData _sectionData, bool isUnlocked = false)
    {
        Index = index;
        Chapter = "chapter" + chapterIndex;
        this.isUnlocked = isUnlocked;
        sectionLookup = lookup;
        sectionDefinition = _sectionData;
    }

    public void LoadSaveData(bool _isClaer, bool _isUnlocked)
    {
        isClear = _isClaer;
        isUnlocked = _isUnlocked;
    }



    // Choices에 값 추가는 클래스 내부에서만 가능
    public void AddChoice(int choice)
    {
        _choices.Add(choice);
    }

    public SectionStatus GetStatus()
    {
        // 현재 섹션 상태 판단
        if (isClear)
        {
            if (Choices.Count == 0)
                return SectionStatus.Ending;
            else
                return SectionStatus.Clear;
        }
        if (isUnlocked)
        {
            return SectionStatus.Incomplete;
        }
        return SectionStatus.Locked;
    }
}

public static class SectionManager
{
    public static Dictionary<int, Section> sections = new Dictionary<int, Section>();
    public static int CurrentSectionIndex { get; private set; } = 1;

    // 섹션 정보를 이름 없이 순서대로 튜플로 정의
    private static (int, int, int[])[] sectionDefinitions = new (int, int, int[])[]
    {
        (1, 0, new int[]{1,2}),
        (1, 1, new int[]{3,4}),
        (1, 2, new int[]{}), // ????
        (2, 3, new int[]{5}),
        (2, 4, new int[]{}),
        (2, 5, new int[]{})
    };
    static Dictionary<int, SectionData> sectionDataLookup;
    static SectionManager()
    {

        // 1. SectionData 에셋들을 로드합니다.
        SectionData[] sectionDataArray = Resources.LoadAll<SectionData>("SectionDefinitions");

        // 2. LINQ의 ToDictionary를 사용하여 딕셔너리를 생성합니다.
        // Key: 에셋의 이름(name)을 int.Parse()로 변환
        // Value: 해당 SectionData 객체
        try
        {
            sectionDataLookup = sectionDataArray.ToDictionary(
                data => int.Parse(data.name),
                data => data
            );

            Debug.Log($"총 {sectionDataLookup.Count}개의 SectionData를 딕셔너리에 로드했습니다.");
        }
        catch (System.FormatException e)
        {
            // 에셋 이름이 정수 형태가 아닐 경우의 예외 처리
            Debug.LogError("일부 SectionData 에셋의 이름이 정수 형태로 되어 있지 않습니다： " + e.Message);
        }


        // 섹션 생성
        foreach (var def in sectionDefinitions)
        {
            SectionData sectionData = sectionDataLookup[def.Item2];
            Section section = new Section(def.Item2, def.Item1, sections, sectionData, def.Item2 == 0);//섹션 0이면 생성시부터 언락
            foreach (var choice in def.Item3)
                section.AddChoice(choice);
            sections.Add(def.Item2, section);
        }


        // PreviousSection 자동 연결
        foreach (var kvp in sections)
        {
            int parentIndex = kvp.Key;
            Section parent = kvp.Value;

            foreach (int childIndex in parent.Choices)
            {
                if (sections.ContainsKey(childIndex))
                {
                    Section child = sections[childIndex];
                    if (child.PreviousSection == -1)
                        child.PreviousSection = parentIndex;
                }
            }
        }
    }

    public static Section GetCurrentSection()
    {
        if (sections.TryGetValue(CurrentSectionIndex, out Section section))
            return section;
        return null;
    }

    public static void Choose(int choiceIndex) // 선택지를 선택한다
    {
        Section current = GetCurrentSection();// 현재 섹션을 받음 
        if (current == null)
        {
            Console.WriteLine("???? ?????? ???????.");
            return;
        }

        if (!current.Choices.Contains(choiceIndex)) // 현재 섹션이 선택지의 인덱스를 가지고 있지 않으면 중단 
        {
            Console.WriteLine("??????? ???? ??????????.");
            return;
        }

        current.IsClear = true; // 현재 섹션을 클리어 처리 

        if (sections.ContainsKey(choiceIndex))
        {
            Section next = sections[choiceIndex];
            next.IsUnlocked = true; // 선택된 섹션을 오픈 처리
            CurrentSectionIndex = next.Index; // 현재 섹션 인덱스를 선택된 섹션으로 변경
        }
        else
        {
            Console.WriteLine("???? ?????? ???????? ??????.");
        }
    }

    public static void PrintCurrentSection()
    {
        Section current = GetCurrentSection();
        if (current == null) return;

        Console.WriteLine($"???? {current.Index} ({current.Chapter}) - ????: {current.GetStatus()}");
        Console.WriteLine("??????: " + (current.Choices.Count == 0 ? "????" : string.Join(", ", current.Choices)));
        Console.WriteLine("???? ????: " + (current.PreviousSection == -1 ? "????" : current.PreviousSection.ToString()));
    }
}
