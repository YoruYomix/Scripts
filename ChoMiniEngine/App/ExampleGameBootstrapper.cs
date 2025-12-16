using UnityEngine;
using Yoru.ChoMiniEngine;

namespace Yoru.App
{
    public class ExampleGameBootstrapper : MonoBehaviour
    {
        [SerializeField] Transform rootDefaultSkin;
        [SerializeField] Transform rootXmasSkin;
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
        }

        private void Start()
        {
            ChoMiniBootstrapper.Boot();
            Configure();
            PlayScope();
        }

        void Configure()
        {
            // -------------------------
            // 0. 모든 이미지 비활성화
            // -------------------------
            foreach (Transform child in rootDefaultSkin.GetComponentsInChildren<Transform>(true))
            {
                if (child != rootDefaultSkin)   // 루트 제외
                    child.gameObject.SetActive(false);
            }
            foreach (Transform child in rootXmasSkin.GetComponentsInChildren<Transform>(true))
            {
                if (child != rootXmasSkin)   // 루트 제외
                    child.gameObject.SetActive(false);
            }

            _container = ChoMiniContainer.Create()
                .RegisterInstaller<ChoMiniGameObjectInstaller>()
                    .Base()
                    .Override(Skin.Xmas)
                    .End()

                .RegisterInstaller<ChoMiniStringInstaller>()
                    .Base()
                    .Override(Language.CN)
                    .Override(Language.JP)
                    .End()

                .RegisterFactory<IChoMiniFactory>()
                    .Base<ChoMiniSequenceFactory>()
                    .Override<ChoMiniRewindFactory>(PlayMode.Rewind)
                    .Override<ChoMiniRandomFactory>(PlayMode.Random)
                    .End()

                .RegisterProvider<IChoMiniGameObjectActivationProvider>()
                    .Base<ChoMiniGameObjectActivationProvider>()
                    .End()

                .RegisterProvider<IChoMiniImageProvider>()
                    .Base<ChoMiniImageFadeProvider>()
                    .Override<ChoMiniImageFadeProviderSpeed2x>(PlaySpeed.Speed2x)
                    .End()
                    
                .RegisterProvider<IChoMiniTextTypingProvider>()
                    .Base<ChoMiniTextTypingProvider>()
                    .Override<ChoMiniTextTypingProviderSpeed2x>(PlaySpeed.Speed2x)
                    .End()

                .Build();


            _container.DebugPrint();
        }

        void PlayScope()
        {
            string[] scriptKR =
            {
                "……",
                "여긴 어디지?",
                "아무도 없는 것 같다.",
            };

            string[] scriptJP =
            {
                "……",
                "ここはどこだ？",
                "誰もいないようだ。",
            };

            string[] scriptCN =
            {
                "……",
                "这里是哪里？",
                "好像一个人也没有。",
            };


            ChoMiniOptions options = new ChoMiniOptions();
            options.Set(Language.CN);
            options.Set(PlaySpeed.Speed2x);

            ChoMiniLifetimeScope scope = _container.CreateScope(options);

            scope
                .Bind<ChoMiniGameObjectInstaller>(rootDefaultSkin)
                .Bind<ChoMiniGameObjectInstaller>(Skin.Xmas, rootXmasSkin)
                .Bind<ChoMiniStringInstaller>(scriptKR)
                .Bind<ChoMiniStringInstaller>(Language.JP, scriptJP)
                .Bind<ChoMiniStringInstaller>(Language.CN, scriptCN);


            scope.DebugPrint();

            var resolvedStrings = scope.Resolve<ChoMiniStringInstaller, string[]>(Language.CN);

            Debug.Log("[Resolve] String:");
            foreach (var line in resolvedStrings)
            {
                Debug.Log($"  {line}");
            }

            // GameObject (Skin)
            var resolvedRoot =
                scope.Resolve<ChoMiniGameObjectInstaller, Transform>(Skin.Xmas);

            Debug.Log($"[Resolve] GameObject Root: {resolvedRoot.name}");

            scope.Play();
        }
    }
}

public enum Language
{
    CN,
    JP
}
public enum Skin
{
    Default,
    Xmas
}

public enum PlayMode
{
    Rewind,
    Random
}

public enum PlaySpeed
{
    Speed2x
}