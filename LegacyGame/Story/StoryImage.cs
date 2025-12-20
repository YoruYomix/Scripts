using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class StoryImage : StoryObj
{
    UIFaderV2 _uIFader;

    public StoryImage(GameObject gameObject) : base(gameObject)
    {
        UIElements uIElements = new UIElements(gameObject.transform);
        _uIFader = new UIFaderV2(uIElements);
        UnityEngine.UI.Image image = _gameObject.GetComponent<UnityEngine.UI.Image>();
    }

    public override void ComplateVIew()
    {
        _gameObject.SetActive(true);
        _uIFader.RestoreOriginal();
    }
    
    public override async UniTask PlayAsync()
    {
        _gameObject.SetActive(true);
        storyState = StoryState.Playing;
        _uIFader.ZeroAlpha();

        try
        {
            // 1. 페이드 인 시작 및 대기. 취소되면 OperationCanceledException 발생
            await _uIFader.FadeInAsync(5f, TokenService.GetCurrentToken());
            LoopStart();
        }
        catch (OperationCanceledException)
        {
            _uIFader.RestoreOriginal();
            throw;
        }
    }
}
