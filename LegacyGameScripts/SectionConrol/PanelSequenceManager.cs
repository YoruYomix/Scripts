using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Animancer;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


#region Fader
public class Fader
{
    private CanvasGroup canvasGroup;
    private float originalAlpha;

    public Fader(CanvasGroup cg)
    {
        canvasGroup = cg;
        originalAlpha = cg.alpha;
    }

    public async UniTask FadeAsync(float duration, float fromAlpha, float toAlpha, CancellationToken ct)
    {
        float elapsed = 0f;
        while (elapsed < duration && !ct.IsCancellationRequested)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            await UniTask.Yield(ct);
        }
        if (!ct.IsCancellationRequested)
            canvasGroup.alpha = toAlpha;
    }

    public void ForceSetAlphaToOriginal() => canvasGroup.alpha = originalAlpha;
    public void Reset() => canvasGroup.alpha = originalAlpha;
}
#endregion





#region TintFlasher
public class TintFlasher
{
    private Graphic graphic;
    private Color originalColor;
    private CancellationTokenSource loopCts;

    public TintFlasher(Graphic g)
    {
        graphic = g;
        originalColor = g.color;
    }

    public void StartLoop(Color targetColor, float interval)
    {
        loopCts?.Cancel();
        loopCts = new CancellationTokenSource();
        _ = LoopTintAsync(targetColor, interval, loopCts.Token);
    }

    public async UniTask LoopTintAsync(Color targetColor, float interval, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            graphic.color = targetColor;
            await UniTask.Delay((int)(interval * 1000), cancellationToken: ct);
            graphic.color = originalColor;
            await UniTask.Delay((int)(interval * 1000), cancellationToken: ct);
        }
    }

    public void Reset()
    {
        loopCts?.Cancel();
        graphic.color = originalColor;
    }
}
#endregion

#region ComicPanel
public abstract class ComicPanel
{
    public enum State { Waiting, Playing, Looping, Completed }
    public State currentState { get; protected set; } = State.Waiting;

    protected CancellationTokenSource playCts;
    protected CancellationTokenSource loopCts;

    public async UniTask PlayAsync()
    {
        if (currentState != State.Waiting) return;

        currentState = State.Playing;
        playCts = new CancellationTokenSource();
        loopCts = new CancellationTokenSource();

        try
        {
            await PlayVisualAsync(playCts.Token);
        }
        catch (OperationCanceledException) { }

        currentState = State.Looping;
        StartLoopVisual();
    }

    protected abstract UniTask PlayVisualAsync(CancellationToken ct);
    protected abstract UniTask LoopVisualAsync(CancellationToken ct);
    protected abstract void ResetVisual();

    protected void StartLoopVisual()
    {
        loopCts?.Cancel();
        loopCts = new CancellationTokenSource();
        _ = LoopVisualAsync(loopCts.Token);
    }

    public virtual void Complete()
    {
        playCts?.Cancel();
        loopCts?.Cancel();
        ResetVisual();
        currentState = State.Completed;
    }

    public virtual void ForceLoop()
    {
        playCts?.Cancel();
        StartLoopVisual();
        currentState = State.Looping;
    }
}

#endregion

#region DialoguePanel
public class DialoguePanel : ComicPanel
{
    private GameObject panelObject;
    private TypingService typer;
    private TextCursorBlinker cursorBlinker;

    public DialoguePanel(GameObject go, Text text, CanvasGroup cg, float cursorBlinkSpeed = 0.5f, float typingDelay = 0.05f)
    {
        panelObject = go;// 게임오브젝트 지정
        panelObject.SetActive(false); // 비활성화
        typer = new TypingService(text, typingDelay); // 타이핑 로직 로드
        cursorBlinker = new TextCursorBlinker(text, cursorBlinkSpeed);  // 커서 깜짝임 로직 로드
    }
    // 플레이 상태 연출
    protected override async UniTask PlayVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true); // 게임오브젝트 활성화
        typer.Reset();           // Play 시작 전 UI 초기화 → Panel 책임
        // cursorBlinker.StopBlink(); //커서 깜빡임은 끈다
        await typer.PlayAsync(ct); // 플레이 시작
    }
    // 루핑 상태 연출
    protected override async UniTask LoopVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true); // 게임오브젝트 액티브
        // cursorBlinker.StartBlink(); // 커서 깜빡임 시작
        while (!ct.IsCancellationRequested) // 캔슬레이션 토큰을 대기
            await UniTask.Yield(ct); // 토큰을 받으면 루핑 종료
    }

    // 비쥬얼 초기화 (텍스트를 원본상태로 만들기)
    protected override void ResetVisual()
    {
        if (currentState == State.Waiting) // 웨이팅 상태라면 게임오브젝트 언액티브
            panelObject.SetActive(false);
        typer.Reset(); // 타이핑 중지, 원래 텍스트 표시
        // cursorBlinker.StopBlink(); // 커서 깜빡임 중지
    }
}
#endregion

#region ArtPanel
public class ArtPanel : ComicPanel
{
    private GameObject panelObject;
    private Fader fader;

    public ArtPanel(GameObject go, Image image, CanvasGroup cg)
    {
        panelObject = go;
        panelObject.SetActive(false);
        fader = new Fader(cg);
    }

    protected override async UniTask PlayVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true);
        await fader.FadeAsync(1f, 0f, 1f, ct);
    }

    protected override async UniTask LoopVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true);
        while (!ct.IsCancellationRequested)
            await UniTask.Yield(ct);
    }

    protected override void ResetVisual()
    {
        if (currentState == State.Waiting)
            panelObject.SetActive(false);
        fader.Reset();
    }

    public override void ForceLoop()
    {
        playCts?.Cancel();

        // 🔹 루프 시작 전 알파를 원본 상태로 복귀
        fader.ForceSetAlphaToOriginal();

        StartLoopVisual();
        currentState = State.Looping;
    }

    public override void Complete()
    {
        playCts?.Cancel();
        loopCts?.Cancel();

        fader.ForceSetAlphaToOriginal();

        currentState = State.Completed;
    }
}

#endregion

#region AnimationPanel

public class AnimationPanel : ComicPanel
{
    private GameObject panelObject;
    private AnimInstaller animInstaller;
    private int clipIndex;

    public AnimationPanel(GameObject go, AnimInstaller installer, int clipIndex = 0)
    {
        panelObject = go;
        panelObject.SetActive(false);

        animInstaller = installer;
        this.clipIndex = clipIndex;
    }

    protected override async UniTask PlayVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true);
        await animInstaller.Play(clipIndex, ct); // 1회 재생 클립이면 완료 대기, 루프면 그냥 재생
    }

    protected override async UniTask LoopVisualAsync(CancellationToken ct)
    {
        panelObject.SetActive(true);

        // 루프 클립은 import 설정으로 자동 루프
        var clip = animInstaller.GetClip(clipIndex);
        if (clip != null)
            animInstaller.Animancer.Play(clip);

        // 루프 종료는 토큰으로
        try
        {
            while (!ct.IsCancellationRequested)
                await UniTask.Yield(ct);
        }
        catch (OperationCanceledException)
        {
            animInstaller.StopAll();
        }
    }

    protected override void ResetVisual()
    {
        if (currentState == State.Waiting)
            panelObject.SetActive(false);

        animInstaller.StopAll();
    }

    public override void ForceLoop()
    {
        panelObject.SetActive(true);
        playCts?.Cancel();

        var clip = animInstaller.GetClip(clipIndex);
        if (clip != null)
            animInstaller.Animancer.Play(clip);

        currentState = State.Looping;
    }

    public override void Complete()
    {
        panelObject.SetActive(true);
        animInstaller.StopAll();
        currentState = State.Completed;
    }
}


#endregion


#region PanelSequenceManager
public class PanelSequenceManager : MonoBehaviour
{
    [SerializeField] private GameObject root; // Root만 지정
    private List<ComicPanelGroup> panelGroups = new List<ComicPanelGroup>();
    private int currentIndex = 0;

    private void Awake()
    {
        // root 아래에 있는 그룹 단위로 자동 생성
        panelGroups = PanelFactory.CreateGroups(root);
    }

    public void OnButtonClick()
    {
        if (currentIndex >= panelGroups.Count) return;

        var currentGroup = panelGroups[currentIndex];

        switch (currentGroup.currentState)
        {
            case ComicPanelGroup.State.Waiting:
                _ = currentGroup.PlayAsync();  // 그룹 내부 패널 순차 실행
                break;

            case ComicPanelGroup.State.Playing:
                currentGroup.ForceLoop();      // 마지막 패널 루프 시작
                break;

            case ComicPanelGroup.State.Looping:
                currentGroup.Complete();        // 그룹 완료
                currentIndex++;
                if (currentIndex < panelGroups.Count)
                    _ = panelGroups[currentIndex].PlayAsync();
                break;

            case ComicPanelGroup.State.Completed:
                break;
        }
    }
}


#endregion




public class ComicPanelGroup
{
    public enum State { Waiting, Playing, Looping, Completed }

    public State currentState { get; private set; } = State.Waiting;
    private List<ComicPanel> panels;
    private int currentIndex = 0;

    public ComicPanelGroup(List<ComicPanel> panelList)
    {
        panels = panelList;
        currentIndex = 0;
    }

    // 그룹 Play 시작
    public async UniTask PlayAsync()
    {
        if (currentState != State.Waiting) return;

        currentState = State.Playing;

        for (currentIndex = 0; currentIndex < panels.Count; currentIndex++)
        {
            var panel = panels[currentIndex];
            await panel.PlayAsync();
            panel.Complete(); // Play 후 바로 Complete
        }

        // 마지막 패널 루프 상태
        currentState = State.Looping;
        panels[^1].ForceLoop();
    }

    // 플레이 중 클릭 시 루프
    public void ForceLoop()
    {
        if (currentState != State.Playing && currentState != State.Looping) return;

        panels[currentIndex].Complete();
        panels[^1].ForceLoop();
        currentState = State.Looping;
    }

    // 그룹 완료 처리
    public void Complete()
    {
        foreach (var panel in panels)
            panel.Complete();

        currentState = State.Completed;
    }
}


#region PanelFactory

public static class PanelFactory
{
    public static List<ComicPanelGroup> CreateGroups(GameObject root)
    {
        var groupList = new List<ComicPanelGroup>();

        foreach (Transform groupTf in root.transform)
        {
            var panelList = new List<ComicPanel>();

            foreach (Transform panelTf in groupTf)
            {

                var go = panelTf.gameObject;
                var library = go.GetComponentInChildren<AnimInstaller>();
                var text = go.GetComponentInChildren<UnityEngine.UI.Text>();
                var image = go.GetComponentInChildren<UnityEngine.UI.Image>();
                var cg = go.GetComponent<CanvasGroup>();
                if (cg == null) cg = go.AddComponent<CanvasGroup>();

                if (library != null)
                    panelList.Add(new AnimationPanel(go, library));
                else if (text != null)
                    panelList.Add(new DialoguePanel(go, text, cg));
                else if (image != null)
                    panelList.Add(new ArtPanel(go, image, cg));
                else
                    Debug.LogWarning($"[PanelFactory] {go.name}에 Panel 생성 불가");
            }

            if (panelList.Count > 0)
                groupList.Add(new ComicPanelGroup(panelList));
        }

        return groupList;
    }
}


#endregion