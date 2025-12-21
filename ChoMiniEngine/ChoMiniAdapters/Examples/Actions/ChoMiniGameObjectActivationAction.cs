using UnityEngine;

namespace Yoru.ChoMiniEngine.Examples
{
    public class ChoMiniGameObjectActivationAction : IChoMiniNodeAction
    {
        public float GetRequiredDuration() => 0f;
        public readonly GameObject _target;

        public GameObject GameObject => _target;

        public ChoMiniGameObjectActivationAction(GameObject target)
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




}