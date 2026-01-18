using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class FlowNodeLegacy  // ��� ȿ���� ��� �ִ� �ڽ�
{
    public FlowAction flowAction;  // ��������Ȳ�� ���� Run �ĺ���
    public List<FlowNodeLegacy> Children;       // �ڽ� ����(Ʈ��)
    public FlowNodeLegacy(FlowAction _flowAction, List<FlowNodeLegacy> _Children)
    {
        flowAction = _flowAction;
        Children = _Children;
    }
}

public abstract class FlowAction
{
    protected int index;
    protected string name;
    public FlowAction(int index, string name)
    {
        this.index = index;
        this.name = name;
    }
    public async UniTask Run(CancellationToken ct)
    {
        try
        { await MainEffect(ct); }
        finally
        {
            if (ct.IsCancellationRequested)
                await CancelEffect();
            else
                await CompleteEffect();
        }
    }

    public async UniTask Loop(CancellationToken ct)
    {
        Debug.Log($"{name}: �� ��� ���� ����");
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await LoopEffect(ct);
            }
        }
        finally
        {
            Debug.Log($"{name}: �� ��� ���� �Ϸ�");
        }
    }

    protected abstract UniTask MainEffect(CancellationToken ct);
    protected abstract UniTask CancelEffect();
    protected abstract UniTask CompleteEffect();
    protected abstract UniTask LoopEffect(CancellationToken ct);
}


public abstract class NodeLegacy
{
    int _index;
    string _name;
    public GameObject _gameObject;
    public List<NodeLegacy> children;
    public bool isPlaying = false;

    public NodeLegacy(int index, string name, GameObject gameObject)
    {
        _index = index;
        _name = name;
        _gameObject = gameObject;
    }
}
public class NodeEmpty : NodeLegacy
{
    public NodeEmpty(int _index, string _name,GameObject gameObject) : base(_index, _name, gameObject)
    {
    }
}

public interface IInitializableView
{
    void InitializeView();
}
public interface IActivatable { void Activate(); }

public interface IPlayable : IActivatable
{
    public float Duration {  get; }
    public void Play();
    public void Complate();
    public bool IsPlaying { get; }

}

public interface ILoopable { public void Loop(); }
public class NodeImage : NodeLegacy , IPlayable , IInitializableView
{

    Image image;
    private Color _originalColor;
    private Tween _fadeTween;
    float _fadeDuration = 2f;

    public bool IsPlaying =>
        _fadeTween != null && _fadeTween.active && _fadeTween.IsPlaying();

    public float Duration => _fadeDuration;

    public NodeImage(int _index, string _name,Image _image,GameObject gameObject) : base(_index, _name, gameObject)
    {
        image = _image;
        _originalColor = image.color;
    }
    public void InitializeView()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }


    public void Play()
    {
        isPlaying = true;
        _fadeTween?.Kill();

        // ���� 0���� ���� ��
        Color c = _originalColor;
        c.a = 0f;
        image.color = c;

        // ���� ���ı��� ���̵���
        _fadeTween = image
            .DOFade(_originalColor.a, 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isPlaying = false;
            });
    }
    public void Complate()
    {
        _fadeTween?.Kill();           // ���� ���̴� ���̵� �ߴ�
        image.color = _originalColor; // ���� ��/���� ����
    }

    public void Activate()
    {
        _gameObject.SetActive(true);
    }
}





public class TestLogAction : FlowAction
{
    public TestLogAction(int index, string name) : base(index, name) { }

    protected override async UniTask MainEffect(CancellationToken ct)
    {
        Debug.Log($"{name}: ���� ����");
        await UniTask.Delay(2000, cancellationToken: ct);
        Debug.Log($"{name}: ���� �Ϸ�");
    }

    protected override UniTask CancelEffect()
    {
        Debug.Log($"{name}: ĵ�� ȿ��");
        return UniTask.CompletedTask;
    }

    protected override UniTask CompleteEffect()
    {
        Debug.Log($"{name}: �Ϸ� ȿ��");
        return UniTask.CompletedTask;
    }

    protected override async UniTask LoopEffect(CancellationToken ct)
    {
        Debug.Log($"{name}: ���� �ݺ�");
        await UniTask.Delay(1000, cancellationToken: ct);
    }
}

public static class TestFlowFactory
{
    // ��Ʈ 20��¥�� Ʈ�� ����
    public static List<FlowNodeLegacy> CreateTestRoots()
    {
        var roots = new List<FlowNodeLegacy>();

        for (int i = 0; i < 20; i++)
        {
            // �̸��� 1���� ����: "1", "2", ... "20"
            string name = (i + 1).ToString();
            var root = CreateNode(
                path: name,
                indexInLevel: i,   // �� ���������� ����(0~19)
                depth: 0
            );
            roots.Add(root);
        }

        return roots;
    }

    private static FlowNodeLegacy CreateNode(string path, int indexInLevel, int depth)
    {
        // �� ����� �׼� �ϳ� ����
        var action = new TestLogAction(indexInLevel, path);

        var children = new List<FlowNodeLegacy>();

        // depth 0,1������ �ڽ� 3���� ���� (�� 3����: 0,1,2)
        if (depth < 2)
        {
            for (int i = 0; i < 3; i++)
            {
                // path: "1-1", "1-2", "1-3" ... "2-1-3" �̷� ��
                string childPath = $"{path}-{i + 1}";
                var child = CreateNode(
                    path: childPath,
                    indexInLevel: i,      // �� ���� �ȿ����� ����(0,1,2)
                    depth: depth + 1
                );
                children.Add(child);
            }
        }

        // �װ� ���� ������ �ñ״�ó�� ���� ����
        return new FlowNodeLegacy(action, children);
    }
}


