using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using Yoru.ChoMiniEngine;

public class InputView : MonoBehaviour
{
    public Button skipButton;


    private void Start()
    {

        skipButton = GetComponent<Button>();
        skipButton.onClick.AddListener(OnSkipClicked);



    }

    private void OnSkipClicked()
    {
        ChoMiniCommand.Advance();
        Debug.LogWarning("스킵 클릭됨");

    }
}
