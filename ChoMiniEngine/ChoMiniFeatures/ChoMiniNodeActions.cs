using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yoru.ChoMiniEngine
{
    public class ActivationAction : IChoMiniNodeAction
    {
        public float GetRequiredDuration() => 0f;
        public readonly GameObject _target;

        public GameObject GameObject => _target;

        public ActivationAction(GameObject target)
        {
            _target = target;
        }

        public void Play()
        {
            _target.SetActive(true);
        }

        public void Complete() 
        { 
        
        }



        public void Pause()
        {

        }

        public void Resume()
        {

        }

        public void Recovery(float time)
        {

        }
    }


    public class FadeInAction : IChoMiniNodeAction
    {
        public float GetRequiredDuration() => _fadeDuration;
        private readonly Image _image;
        private readonly Color _originalColor;
        private Tween _tween;
        private float _fadeDuration = 2f;
        public GameObject GameObject
        {
            get
            {
                return _image.gameObject;
            }
        }

        public FadeInAction(Image image)
        {
            _image = image;
            _originalColor = image.color;
        }

        public void Play()
        {
            // 알파 0으로 초기화
            Color c = _originalColor;
            c.a = 0f;
            _image.color = c;

            // 완료 시 자동으로 원본 색상으로 도달함
            _tween = _image.DOFade(_originalColor.a, _fadeDuration);
        }

        public void Complete()
        {
            // 스킵 → Kill(true) 로 바로 최종 값으로 보정됨
            if (_tween != null && _tween.IsActive())
                _tween.Kill(true);  // true = "완료 상태로 처리"
        }



        public void Pause()
        {

        }

        public void Resume()
        {

        }

        public void Recovery(float time)
        {

        }
    }
}