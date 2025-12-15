using UnityEngine;
using Yoru.ChoMiniEngine;

namespace Yoru.ExampleGame
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] Transform rootKR;
        [SerializeField] Transform rootJP;
        ChoMiniContainer _container;

        public static GameBootstrapper instance { get; private set; }
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            ChoMiniBootstrapper.Boot();
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

            _container = ChoMiniContainer.Create()
                .RegisterInstaller<ChoMiniGameObjectInstaller>()
                    .Base()
                    .Override(Skin.Xmas)
                    .End()

                .RegisterInstaller<ChoMiniStringInstaller>()
                    .Base()
                    .Override(Language.KR)
                    .Override(Language.JP)
                    .End()

                .RegisterFactory<IChoMiniFactory>()
                    .Base<ChoMiniSequenceFactory>()
                    .Override<ChoMiniRewindFactory>(PlayMode.Rewind)
                    .Override<ChoMiniRandomFactory>(PlayMode.Random)
                    .End()

                .Build();

            _container.DebugPrint();
        }

    }


}

public enum Language
{
    KR,
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