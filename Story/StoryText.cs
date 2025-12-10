using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StoryText : StoryObj , ILoopableStoryObj
{
    private Text _text;
    private UniTaskTypewriter writer;
    TypingService typingService;
    TextCursorBlinker cursorBlinker;
    string fullText;
    public StoryText(GameObject gameObject) : base(gameObject)
    {
        _text = _gameObject.GetComponent<Text>();
        fullText = _text.text;
        typingService = new TypingService(_text);
        cursorBlinker = new TextCursorBlinker(_text);
    }
    public override async UniTask PlayAsync()
    {
        _gameObject.SetActive(true);
        storyState = StoryState.Playing;
        _text.text = string.Empty;

        try
        {
            // 1. 페이드 인 시작 및 대기. 취소되면 OperationCanceledException 발생
            await typingService.PlayAsync(TokenService.GetCurrentToken());
            LoopStart();
        }
        catch (OperationCanceledException)
        {
            _text.text = fullText;
            throw;
        }

    }
    public async UniTask LoopVisual()
    {
        try
        {
            await cursorBlinker.BlinkAsync(TokenService.GetCurrentToken());
        }
        finally
        {
            ComplateVIew(); // 시각적인 역할만 할 뿐임. 알아보기 쉽게 매써드 이름변경 
        }
    }

    public override void ComplateVIew()
    {
        _gameObject.SetActive(true);
        _text.text = fullText;
    }
}
