using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FlowManager : MonoBehaviour
{
    public Button button;
    public static FlowManager instance { get; private set; }

    private bool _isProcessing = false;          // ��Ŭ ����


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
        // root�� Ž���Ͽ� Ʈ�� ������Ʈ�� ������ ��
        // ��带 �ν����Ͽ� ��� ����Ʈ�� ��ȯ�Ѵ�
        // testNodes = YoruUtilitys.NodeInstaller(root);

        HideRoot();
        button.onClick.AddListener(OnClickEvent);
        RunAsync(testNodes);
    }
    // ��Ʈ ��ü ���Ƽ��
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
            await RunAfterAutoDelay(playable); // ���� �� 2�� ������
            return _skipRequested; // ���� ���� ������ skip Ȯ��
        }
        else
        {
            node._gameObject.SetActive(true);
            return _skipRequested;
        }
    }
    // ���� ����
    public async void RunAsync(List<NodeLegacy> nodes)
    {
        _skipRequested = false;

        for (int i = 0; i < nodes.Count; i++)
        {
            Debug.Log(i);
            // ��Ƽ���� �÷��̾�� �б�aptj
            bool skipped = await HandleNodeAsync(nodes[i]);

            if (skipped) // ��ŵ��û�� ���Դٸ� ���� ��� �Ϸ�
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
        Debug.LogWarning("��ŵ ��û");
        _skipRequested = true;
    }


    // ��� ��� �ð���ŭ ��� �� ������� ��� 
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

    // �ӽ������� PlayNodeOnce�� ����ϱ� ���� ���� �׽�Ʈ.
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










    IPlayable curruntPlayable;
    void PlayNodeOnce(IPlayable _node)
    {
        Debug.Log("��� ����");
        if (curruntPlayable != null)
        {
            if (curruntPlayable.IsPlaying)
            {
                Debug.Log("�Ϸ� ����");
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
                Debug.Log("��� ����");
                newNode.Play();
            }
        }
    }



}



