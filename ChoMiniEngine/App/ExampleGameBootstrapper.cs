using UnityEngine;
using Yoru.ChoMiniEngine;


public class ExampleGameBootstrapper : MonoBehaviour
{
    [SerializeField] Transform imageRoot;
    ChoMiniContainer _container;

    public static ExampleGameBootstrapper instance { get; private set; }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        ChoMiniEngine.Boot();
        Configure();
    }

    void Configure()
    {
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
 
            // Installer
            .Register<ChoMiniGameObjectSourceInstaller>("KR").Using(imageRoot)

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
        ChoMiniLifetimeScope _scope = _container.CreateScope(options);
    }

}

