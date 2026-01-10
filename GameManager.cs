using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // static으로 어디서든 접근 가능
    public ChapterObj chapter;
    public EndingUI endingUI;
    public Page lastSelection;

    public ManuUI manuUI;

    public SaveData currentSaveData;
    private int numberOfTotalSections = 10; // 전체 섹션 수

    public SceneInfomation sceneInfomation;
    public ReplayManager replayManager;
    public StoryManager storyManager;
    public InputManager inputManager;
    ObjectMemoryManager objectMemoryManager;
    public TitleMenu titleMenu;

    언어설정 language;
    public Action<언어설정> languageSwapEvent;

    public 언어설정 CurrntLangage
    {
        get
        {
            return language;
        }
        set
        {
            language = value;
            languageSwapEvent?.Invoke(language);
        }
    }

    [SerializeField] GameObject ActiveOnPlay;

    GameModeLegacy _modeLegacy;

    public GameModeLegacy GameModeLegacy
    {
        get
        {
            return _modeLegacy;
        }
        set
        {
            _modeLegacy = value;
            switch (_modeLegacy)
            {
                case GameModeLegacy.Story:
                    inputManager.inputTable = inputManager.inputTableStorymode;
                    break;
                case GameModeLegacy.ManuOpen:
                    inputManager.inputTable = inputManager.inputTableManuOpen;
                    break;
                case GameModeLegacy.Replay:
                    inputManager.inputTable = inputManager.inputTableReplayMode;
                    break;
                case GameModeLegacy.LockUI:
                    inputManager.inputTable = inputManager.inputTableLockUI;
                    break;
            }
        }
    }





    public bool isReplayMode
    {
        get
        {
            return (GameModeLegacy == GameModeLegacy.Replay);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;       // 자기 자신을 Instance로 등록
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);   // 이미 Instance가 있으면 중복 제거
        }

    }

    public void Start()
    {

        /////////// 게임 정보 초기화 /////////
        objectMemoryManager = FindObjectOfType<ObjectMemoryManager>(); // 프리팹 생성기
        titleMenu = FindObjectOfType<TitleMenu>(); // 타이틀 화면
        titleMenu.Init();
        ActiveOnPlay?.SetActive(true); // 게임 시작시 하이드 된 것 액티브
        sceneInfomation = FindAnyObjectByType<SceneInfomation>(); // 신 정보
        sceneInfomation.replayUI.SetActive(false);
        storyManager = FindAnyObjectByType<StoryManager>();
        inputManager = FindAnyObjectByType<InputManager>();
        inputManager.Init();
        replayManager = FindAnyObjectByType<ReplayManager>();
        replayManager.Init(sceneInfomation.replayPageParent.transform);
        manuUI = FindObjectOfType<ManuUI>();
        manuUI.Init();



        /////////// 게임 실행 ////////////

        GameModeLegacy = GameModeLegacy.LockUI;

        ///////// 테스트용 챕터가 있는지 확인, 있다면 개발 테스트 모드 실행  //////
        chapter = FindObjectOfType<ChapterObj>();
        if (chapter != null)
        {
            chapter.Init();
            storyManager.EnterStoryMode(chapter.nowSectionObj.FirstPage);
            return;
        }

        //////////// 테스트용 챕터가 없다면 게임 정상 실행 //////////////


        // 게임 시작 시 저장된 데이터를 불러옵니다.
        LoadSaveData();


        titleMenu.CallTitleMenu();  // 타이틀 화면 열기
        // LoadChapter(StoryUtility.GetChapterName(1), () => { storyManager.EnterStoryMode(chapter.nowSection.FirstPage); }); // 프리팹으로 로드
        LoadScene(StoryUtility.GetChapterName(1), () => {  // 챕터에 맞는 애디티브 씬을 로드
            storyManager.EnterStoryMode(chapter.nowSectionObj.FirstPage);  // 챕터의 첫 페이지로 스토리 모드 시작
        }); 
    }


    void LoadSaveData()
    {
        currentSaveData = SaveLoadManager.LoadGame();

        // 불러온 데이터가 null이거나 섹션 수가 맞지 않으면 초기화 로직을 추가할 수 있습니다.
        if (currentSaveData.SectionData.Length != numberOfTotalSections)
        {
            Debug.LogWarning("저장된 데이터의 섹션 수가 일치하지 않아 초기화합니다.");
            currentSaveData = new SaveData(numberOfTotalSections);
        }

        // 마지막 저장 지점 조회
        int lastSection = currentSaveData.lastSectionSaveData;
        int lastPage = currentSaveData.lastPageSaveData;

        Debug.Log($"마지막으로 저장된 위치： Section {lastSection}의 Page {lastPage}");

        // 게임이 시작되며 로드한 데이터를 섹션정보에 업데이트
        for (int i = 0; i < currentSaveData.SectionData.Length; i++) 
        {
            if (SectionManager.sections.ContainsKey(i))
            {
                bool _isclear = currentSaveData.SectionData[i].clear;
                bool _unLocked = currentSaveData.SectionData[i].unLocked;
                SectionManager.sections[i].LoadSaveData(_isclear, _unLocked);
            }
        }

        return;


        //섹션 데이터 저장 조회 예제
        // SaveDataExample();

    }
     
    void SaveDataExample() //섹션 데이터 저장 조회 예제
    {
        int targetSection = 2; // 조회하고자 하는 섹션 번호 (0부터 시작)
        Debug.Log(SectionManager.sections[targetSection].IsClear); //저장 전 데이터를 조회
        Debug.Log(SectionManager.sections[targetSection].IsUnlocked);
        Debug.Log(SectionManager.sections[targetSection].GetStatus());

        SectionManager.sections[targetSection].IsClear = true;  // 값 입력시 자동 저장 
        SectionManager.sections[targetSection].IsUnlocked = true; // 값 입력시 자동 저장

        Debug.Log(SectionManager.sections[targetSection].IsClear); //저장 후 데이터를 조회
        Debug.Log(SectionManager.sections[targetSection].IsUnlocked);
        Debug.Log(SectionManager.sections[targetSection].GetStatus());
    }

    // 메모리의 세이브 데이터를 로컬디스크에 저장
    public void SaveCurrentStateToDisk()
    {
        // 현재 메모리에 있는 currentSaveData 객체를 저장합니다.
        Debug.Log("데이터 저장");
        SaveLoadManager.SaveGame(currentSaveData);
    }

    async void LoadChapter(string chapterName, Action _action)
    {
        await objectMemoryManager.LoadObjectAsync(chapterName);
        chapter = FindObjectOfType<ChapterObj>();
        chapter.Init();
        _action?.Invoke();
    }

    async void UnLoadChapte(string chapterName)
    {
        if (!objectMemoryManager.IsObjectLoaded(chapterName)) return;
        chapter = null;
        storyManager.ReturnPage();
        replayManager.ResetReplay();
        await objectMemoryManager.UnloadObjectAsync(chapterName);

    }
    async void LoadScene(string chapterName, Action _action)
    {
        if (StoryUtility.IsSceneLoaded(chapterName)) return;
        await SubSceneLoaderUniTask.LoadSubSceneAsync(chapterName);
        chapter = FindObjectOfType<ChapterObj>();
        chapter.Init();
        _action?.Invoke();
    }
    async void UnLoadScene(string chapterName)
    {
        if (!StoryUtility.IsSceneLoaded(chapterName)) return;
        storyManager.ReturnPage();
        replayManager.ResetReplay();
        await SubSceneLoaderUniTask.UnloadSubSceneAsync(chapterName);

    }


}


public enum GameModeLegacy
{
    Story,
    ManuOpen,
    Replay,
    LockUI
}