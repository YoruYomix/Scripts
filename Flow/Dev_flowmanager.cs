using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;



public class Dev_flowmanager : MonoBehaviour
{
    public Button button;
    public static Dev_flowmanager instance { get; private set; }

    private bool _isProcessing = false;          // 광클 방지

    private CancellationTokenSource _cts;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    List<FlowNodeLegacy> testNodes;

    private void Start()
    {
        testNodes = TestFlowFactory.CreateTestRoots();

        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickEvent);

    }
    int curruntIndex= 0;


    async void OnClickEvent()
    {
        if (_isProcessing) { return; }
        _isProcessing = true; // 광클방지 검사

        if (curruntIndex >= testNodes.Count)
        {
            Debug.Log("모든 트리 재생 완료");
            return;
        }
        // 선택한 하나의 트리를 전체 재생
        await PlayTree(testNodes[curruntIndex]);

        curruntIndex++;
        _isProcessing = false; // 광클방지 끔 
    }



    public async UniTask PlayTree(FlowNodeLegacy root)
    {
        FlowNodeLegacy lastNode = null;

        // 1) 우선 마지막 노드를 알아냄
        foreach (var node in Traverse(root))
            lastNode = node;

        // 2) 이제 진짜 재생 시작
        foreach (var node in Traverse(root))
        {
            var runnable = node.flowAction;
            await TransitionTo(runnable);

            // ★ 마지막 노드면 루프 실행
            if (node == lastNode)
            {
                _ = runnable.Loop(_cts.Token);  // 기다리지 않고 실행
            }
        }
    }
    private IEnumerable<FlowNodeLegacy> Traverse(FlowNodeLegacy node)
    {
        yield return node;

        foreach (var child in node.Children)
            foreach (var n in Traverse(child))
                yield return n;
    }


    UniTask _runningTask = UniTask.CompletedTask;
    private async UniTask TransitionTo(FlowAction runnable)
    {
        _cts?.Cancel();  // 이전 노드 강제 종료
        _cts?.Dispose();

        if (_runningTask.Status == UniTaskStatus.Pending)  // 가 아직 끝나지 않고 실행 중인지 확인하는 조건문이다.
            await _runningTask.SuppressCancellationThrow();  // 이전 작업의 파이널리를 기다림

        _cts = new CancellationTokenSource();
        _runningTask = PlayRunnable(runnable, _cts.Token); // 지금 돌아가는 작업을 기록한다
    }



    public async UniTask PlayRunnable(FlowAction runnable, CancellationToken ct)
    {
        Debug.Log("재생 시작");
        await runnable.Run(ct).SuppressCancellationThrow();
        Debug.Log("재생 완료");
    }
}

