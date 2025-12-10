using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.WSA;

public class FlowNodeLegacy  // 노드 효과를 담고 있는 박스
{
    public FlowAction flowAction;  // 감정·상황에 따른 Run 후보들
    public List<FlowNodeLegacy> Children;       // 자식 노드들(트리)
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
        Debug.Log($"{name}: 번 노드 루프 시작");
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await LoopEffect(ct);
            }
        }
        finally
        {
            Debug.Log($"{name}: 번 노드 루프 완료");
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

        // 알파 0으로 만든 뒤
        Color c = _originalColor;
        c.a = 0f;
        image.color = c;

        // 원본 알파까지 페이드인
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
        _fadeTween?.Kill();           // 진행 중이던 페이드 중단
        image.color = _originalColor; // 원본 색/알파 복구
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
        Debug.Log($"{name}: 메인 시작");
        await UniTask.Delay(2000, cancellationToken: ct);
        Debug.Log($"{name}: 메인 완료");
    }

    protected override UniTask CancelEffect()
    {
        Debug.Log($"{name}: 캔슬 효과");
        return UniTask.CompletedTask;
    }

    protected override UniTask CompleteEffect()
    {
        Debug.Log($"{name}: 완료 효과");
        return UniTask.CompletedTask;
    }

    protected override async UniTask LoopEffect(CancellationToken ct)
    {
        Debug.Log($"{name}: 루프 반복");
        await UniTask.Delay(1000, cancellationToken: ct);
    }
}

public static class TestFlowFactory
{
    // 루트 20개짜리 트리 생성
    public static List<FlowNodeLegacy> CreateTestRoots()
    {
        var roots = new List<FlowNodeLegacy>();

        for (int i = 0; i < 20; i++)
        {
            // 이름은 1부터 시작: "1", "2", ... "20"
            string name = (i + 1).ToString();
            var root = CreateNode(
                path: name,
                indexInLevel: i,   // 이 레벨에서의 순서(0~19)
                depth: 0
            );
            roots.Add(root);
        }

        return roots;
    }

    private static FlowNodeLegacy CreateNode(string path, int indexInLevel, int depth)
    {
        // 이 노드의 액션 하나 생성
        var action = new TestLogAction(indexInLevel, path);

        var children = new List<FlowNodeLegacy>();

        // depth 0,1까지만 자식 3개씩 생성 (총 3레벨: 0,1,2)
        if (depth < 2)
        {
            for (int i = 0; i < 3; i++)
            {
                // path: "1-1", "1-2", "1-3" ... "2-1-3" 이런 식
                string childPath = $"{path}-{i + 1}";
                var child = CreateNode(
                    path: childPath,
                    indexInLevel: i,      // 이 레벨 안에서의 순서(0,1,2)
                    depth: depth + 1
                );
                children.Add(child);
            }
        }

        // 네가 만든 생성자 시그니처에 맞춰 생성
        return new FlowNodeLegacy(action, children);
    }
}


