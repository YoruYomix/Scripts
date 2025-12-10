using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEditor.VersionControl;
using Unity.VisualScripting.Antlr3.Runtime;


public class PageOnClickActionSequence : MonoBehaviour
{
    private Image image;


    public SqStatus status;


    private Text _text;
    private UniTaskTypewriter writer;

    private float duration = 0.2f; // 페이드 시간



    // Start is called before the first frame update

    public void Init()
    {
        status = SqStatus.init;

        InitImage();
        Inittext();
        gameObject.SetActive(false);
    }

    void InitImage()
    {
        image = GetComponent<Image>();
        if (image == null) return;
        var c = image.color;
        c.a = 0f;
        image.color = c;
    }

    void Inittext()
    {
        _text = GetComponent<Text>();
        if (_text == null) return;
        if (writer == null)
        {
            writer = gameObject.AddComponent<UniTaskTypewriter>();
        }
        writer.Initialize(_text);

    }

    public void StandBy()
    {
        if (status == SqStatus.standby) return;

        status = SqStatus.standby;
        if (writer == null)
        {
            return;
        }
        writer.PlaytextBlink();
    }


    public void Complete()
    {
        status = SqStatus.complete;
        if (writer == null)
        {
            return;
        }
        writer.Stop();
    }

    public async UniTask PlaySequence(CancellationToken token)
    {
        if (status == SqStatus.playing) return;
        gameObject.SetActive(true);
        status = SqStatus.playing;
        await FadeIn(token);
        if (writer != null)
        await writer.PlayTextAsync(token);
        Complete();
    }

    public async UniTask FadeIn(CancellationToken token)
    {
        if (image == null) return;

        float time = 0f;

        try
        {
            while (time < duration)
            {
                token.ThrowIfCancellationRequested();
                time += Time.deltaTime;
                float t = time / duration;

                Color c = image.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                image.color = c;

                await UniTask.Yield(); // 다음 프레임까지 기다리기
            }
        }
        catch (OperationCanceledException)
        {
            // 클릭으로 중단됨
        }
        finally
        {
            // 마지막 프레임에서 완전히 보이도록 정리

        }
    }

    void ImageOn()
    {
        if (image == null) return;

        Color finalColor = image.color;
        finalColor.a = 1f;
        image.color = finalColor;
        status = SqStatus.complete;
    }

    public void CompleteInstantly()
    {
        gameObject.SetActive(true);
        ImageOn();
        if (writer != null)
            writer.Stop();
    }


}
