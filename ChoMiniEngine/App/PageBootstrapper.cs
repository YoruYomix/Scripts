using System.Collections.Generic;
using UnityEngine;
using Yoru.ChoMiniEngine;


public class PageBootstrapper : MonoBehaviour
{
    [SerializeField] Transform imageRoot;
    ChoMiniNodeRunner _nodeRunner;
    ChoMiniOrchestrator _orchestrator;
    ChoMiniSequenceFactory _currentFactory;
    GlobalMessageContext _msg;
    ChoMiniLifetimeScope _scope;

    ChoMiniContainer _container;

    public static PageBootstrapper instance { get; private set; }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Configure();
    }

    void Configure()
    {

        // 메시지파이프 컨텍스트 생성
        _msg = new GlobalMessageContext();


        // -------------------------
        // 0. 모든 이미지 비활성화
        // -------------------------
        foreach (Transform child in imageRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child != imageRoot)   // 루트 제외
                child.gameObject.SetActive(false);
        }

        // -------------------------
        // 1. FlowContainer 구성
        // -------------------------
        _container = ChoMiniContainer.Create()
            // 메세지 파이프
            .SubscribeGlobalMessages(_msg)

            // Installer
            .Register<UIRootInstaller>("KR").Using(imageRoot)

            // Providers
            .Register<ImageActionProvider>()
            .Register<DefaultActivationProvider>()

            // Factory
            .RegisterFactory<ChoMiniSequenceFactory>("Default")

            // Orchestrator / Runner / MessagePipe 생략 시 기본값 자동 적용
            .Build();

        // -------------------------
        // 2. SessionOptions 만들기
        // -------------------------
        var options = new FlowSessionOptions
        {
            InstallerKey = "KR",
            FactoryKey = "Default",
            SceneRoot = imageRoot,
            UserData = null
        };

        // -------------------------
        // 3. Scope 생성
        // -------------------------
        _scope = _container.CreateScope(options);



        // 3. UX 구성
        InputView inputView = FindObjectOfType<InputView>();
        inputView.Initialize(_msg);


    }

    private async void Start()
    {
        Debug.Log("🔥 테스트 시작됨");
        await _scope.PlayAsync();
        Debug.Log("🔥 테스트 완전히 종료됨");


        _scope.Dispose();
        _scope = null;
    }


}

