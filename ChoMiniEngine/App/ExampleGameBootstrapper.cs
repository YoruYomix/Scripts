using UnityEngine;
using Yoru.ChoMiniEngine;


public class ExampleGameBootstrapper : MonoBehaviour
{
    [SerializeField] Transform rootKR;
    [SerializeField] Transform rootJP;
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
        foreach (Transform child in rootKR.GetComponentsInChildren<Transform>(true))
        {
            if (child != rootKR)   // 루트 제외
                child.gameObject.SetActive(false);
        }
        foreach (Transform child in rootJP.GetComponentsInChildren<Transform>(true))
        {
            if (child != rootJP)   // 루트 제외
                child.gameObject.SetActive(false);
        }
        var builder = new ChoMiniContainer.Builder();
        // -------------------------
        // 1. FlowContainer 구성
        // -------------------------
        builder.Installer<ChoMiniGameObjectSourceInstaller>()
                .When(() => false).Select("KR")
                .When(() => true).Select("JP");
        builder
            // Providers
            .RegisterProvider<ImageActionProvider>()
            .RegisterProvider<DefaultActivationProvider>()

            // Factory
            .RegisterFactory<ChoMiniSequenceFactory>("Default");

        _container = builder.Build();


        // -------------------------
        // 3. Scope 생성
        // -------------------------
        ChoMiniLifetimeScope _scope = _container.CreateScope();
        _scope
            .Bind<ChoMiniGameObjectSourceInstaller>("KR", new ChoMiniGameObjectInstallerResource(rootKR))
            .Bind<ChoMiniGameObjectSourceInstaller>("JP", new ChoMiniGameObjectInstallerResource(rootJP));

        _scope.BootAsync();
    }

}

