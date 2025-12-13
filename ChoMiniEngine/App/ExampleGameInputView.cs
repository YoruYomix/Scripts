using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using Yoru.ChoMiniEngine;

public class ExampleGameInputView : MonoBehaviour
{
    public Button _advanceButton;


    private void Start()
    {

        _advanceButton = GetComponent<Button>();
        _advanceButton.onClick.AddListener(OnSkipClicked);



    }

    private void OnSkipClicked()
    {
        ChoMiniCommand.Advance();
        Debug.LogWarning("진행 버튼 클릭 됨");

    }
}
