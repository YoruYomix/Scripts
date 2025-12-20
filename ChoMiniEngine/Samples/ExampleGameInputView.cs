using UnityEngine;
using UnityEngine.UI;
using Yoru.ChoMiniEngine.Utility;

namespace Yoru.ChoMiniEngine.Samples
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
            Debug.Log("버튼 클릭 됨");
            ChoMiniGlobalCommand.Advance();
        }
    }
}