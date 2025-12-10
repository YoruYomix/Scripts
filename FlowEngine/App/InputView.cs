using MessagePipe;
using UnityEngine;
using UnityEngine.UI;

public class InputView : MonoBehaviour
{
    public Button skipButton;
    private GlobalMessageContext _msg;

    public void Initialize(GlobalMessageContext msg)
    {
        _msg = msg;
    }

    private void Start()
    {

        skipButton = GetComponent<Button>();
        skipButton.onClick.AddListener(OnSkipClicked);



    }

    private void OnSkipClicked()
    {
        Debug.LogWarning("스킵 클릭됨");
        _msg.SkipAllPublisher.Publish(new SkipAllNodesMessage());

    }
}
