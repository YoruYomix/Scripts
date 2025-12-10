using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    Button targetButton;


    public void Init()
    {
        targetButton = GetComponent<Button>();

        // 기본 onClick 삭제 (원하면 유지 가능)
        targetButton.onClick.RemoveAllListeners();
    }


    // 좌·우 클릭 모두 감지
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.inputManager.lockInput > 0)
        {
            return;
        }
        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Left();
        }

        // 우클릭
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Right();
        }
    }

    public void Left()
    {
        GameManager.Instance.inputManager.ScreenClickAction();
    }

    public void Right()
    {
        GameManager.Instance.inputManager.RightCilckAction();
    }
}
