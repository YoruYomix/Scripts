using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StoryLoopImage : StoryObj , ILoopableStoryObj
{
    UIFaderV2 _uIFader;
    TintFlasher tintFlasher;


    public StoryLoopImage(GameObject myGameObj):base(myGameObj)
    {
        UIElements uIElements = new UIElements(myGameObj.transform);
        _uIFader = new UIFaderV2(uIElements);
        UnityEngine.UI.Image image = _gameObject.GetComponent<UnityEngine.UI.Image>();
        tintFlasher = new TintFlasher(image);
    }

    public override void ComplateVIew()
    {
        _gameObject.SetActive(true);
        _uIFader.RestoreOriginal();
    }

    public async UniTask LoopVisual()
    {
        await tintFlasher.LoopTintAsync(Color.red,0.2f, TokenService.GetCurrentToken());
        ComplateVIew();
    }

    public override async UniTask PlayAsync()
    {
        _gameObject.SetActive(true);
        storyState = StoryState.Playing;
        _uIFader.ZeroAlpha();
        await _uIFader.FadeInAsync(5f, TokenService.GetCurrentToken());
        LoopStart();
    }
}
