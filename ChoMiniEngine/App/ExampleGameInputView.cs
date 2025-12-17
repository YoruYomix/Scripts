using UnityEngine;
using UnityEngine.UI;
using Yoru.ChoMiniEngine.Utility;

namespace Yoru.App
{
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
            ChoMiniGlobalCommand.Advance();
            Debug.LogWarning("진행 버튼 클릭 됨");
        }
    }
}