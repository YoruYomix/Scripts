using Cysharp.Threading.Tasks;
using UnityEngine;



namespace Yoru.ChoMiniEngine
{
    // 단일 노드를 받아서 엔진의 작동 트리거를 구독한다.
    public class ChoMiniNodeRunner
    {
        public async UniTask RunNode(ChoMiniNode node)
        {
            foreach (var effect in node.Actions)
            {
                effect.Play();  // 노드의 이펙트들이 재생을 시작
            }
            float time = 0f;
            while (time < node.Duration)
            {
                time += Time.deltaTime;
                await UniTask.Yield();
            }
            foreach (var effect in node.Actions)  // 노드의 이펙트들이 재생을 종료
                effect.Finish();
        }
    }
}