using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class FlowManager : MonoBehaviour
{
    public Button button;
    public static FlowManager instance { get; private set; }

    private bool _isProcessing = false;          // 광클 방지


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

    List<NodeLegacy> testNodes;
    [SerializeField] Transform root;
    private void Start()
    {
        // root를 탐색하여 트리 오브젝트를 수집한 뒤
        // 노드를 인스톨하여 노드 리스트를 반환한다
        testNodes = YoruUtilitys.NodeInstaller(root);

        HideRoot();
        button.onClick.AddListener(OnClickEvent);
        RunAsync(testNodes);
    }
    // 루트 전체 언액티브
    void HideRoot()
    {
        foreach (var item in testNodes)
        {
            item._gameObject.SetActive(false);
        }
    }
    int curruntRootIndex = 0;



    private bool _skipRequested = false;
    void OnClickEvent()
    {
        RequestSkip();
    }

    private async UniTask<bool> HandleNodeAsync(NodeLegacy node)
    {
        if (node is IPlayable playable)
        {
            await RunAfterAutoDelay(playable); // 실행 후 2초 딜레이
            return _skipRequested; // 실행 끝난 시점에 skip 확인
        }
        else
        {
            node._gameObject.SetActive(true);
            return _skipRequested;
        }
    }
    // 최초 실행
    public async void RunAsync(List<NodeLegacy> nodes)
    {
        _skipRequested = false;

        for (int i = 0; i < nodes.Count; i++)
        {
            Debug.Log(i);
            // 엠티노드와 플레이어블 분기aptj
            bool skipped = await HandleNodeAsync(nodes[i]);

            if (skipped) // 스킵요청이 들어왔다면 남은 노드 완료
            {
                CompleteRemaining(nodes, i + 1);
                break;
            }
        }
    }
    private void CompleteRemaining(List<NodeLegacy> nodes, int start)
    {
        for (int i = start; i < nodes.Count; i++)
        {
            if (nodes[i] is IPlayable p)
            {
                p.Activate();
                p.Complate();
            }
            else
                nodes[i]._gameObject.SetActive(true);
        }
    }
    public void RequestSkip()
    {
        Debug.LogWarning("스킵 요청");
        _skipRequested = true;
    }


    // 노드 재생 시간만큼 대기 후 다음노드 재생 
    private async UniTask RunAfterAutoDelay(IPlayable playableNode)
    {
        PlayNodeOnce(playableNode);
        float t = 0f;

        while (t < playableNode.Duration && !_skipRequested)
        {
            await UniTask.Yield();
            t += UnityEngine.Time.deltaTime;
        }
    }

    // 임시적으로 PlayNodeOnce를 재생하기 위해 만든 테스트.
    void TestRun()
    {
        NodeLegacy _node = testNodes[curruntRootIndex];
        if (_node is NodeEmpty)
        {
            _node._gameObject.SetActive(true);
            curruntRootIndex++;
            _node = testNodes[curruntRootIndex];
        }
        _node._gameObject.SetActive(true);
        // PlayNodeOnce(_node);
        curruntRootIndex++;
    }










    // List<Node> 에서 클릭에 반응하여 순차적으로 노드하나씩 빼내는 메써드
    // 노드의 추상화 타입을 검사하여 엠티노드면 빼지않고 카운트만 올림
    // 클릭하지 않아도 노드의 재생주기 float에 맞춰서 자동적으로 뺀다
    // 모든 노드들의 재생이 종료되면 노드중 ILoopable 인터페이스 구현체중에 마지막을
    // node.Loop() 시킨다
    // 모든동작이 완료된 후 다음  List<Node> 를 받았는데 
    // 이전 List<Node>의 마지막 ILoopable이 node.IsLooping 이면 CancelLoop()한다

    // 노드 1개 단일재생 

    IPlayable curruntPlayable;
    void PlayNodeOnce(IPlayable _node)
    {
        Debug.Log("재생 진입");
        if (curruntPlayable != null)
        {
            if (curruntPlayable.IsPlaying)
            {
                Debug.Log("완료 시작");
                curruntPlayable.Complate();
            }
        }
        curruntPlayable = _node;
        curruntPlayable.Activate();
        if (curruntPlayable is IInitializableView iInitialiView)
        {
            iInitialiView.InitializeView();
        }

        if (curruntPlayable is IPlayable newNode)
        {
            if (!newNode.IsPlaying)
            {
                Debug.Log("재생 시작");
                newNode.Play();
            }
        }
    }



}



public class AsyncTreeIterator
{
    private readonly IEnumerator<TreeNode> _enumerator;



    /// <summary>
    /// 요청받을 때마다 한 노드 반환 (없으면 null)
    /// </summary>
    public UniTask<TreeNode> NextAsync()
    {
        // 비동기처럼 보이지만 await 지점 없는 빠른 UniTask
        if (_enumerator.MoveNext())
            return UniTask.FromResult(_enumerator.Current);

        return UniTask.FromResult<TreeNode>(null);
    }


}
