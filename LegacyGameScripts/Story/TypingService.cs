using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine.UI;

public class TypingService
{
    private Text uiText;
    private string fullText;
    private float delayPerChar;
    private List<string> textList = new List<string>();

    public string FullText => fullText; // 외부에서 읽기용

    public TypingService(Text text, float delay = 0.05f)
    {
        uiText = text;
        fullText = uiText.text;      // 원본 텍스트 저장
        delayPerChar = delay;
        CreateListFromText(fullText);
        // 유아이 초기화 제거 → SRP 준수
    }

    private void CreateListFromText(string input)
    {
        textList.Clear();
        string temp = "";
        Stack<string> openTags = new Stack<string>();
        var regex = new Regex(@"(<.*?>|\n|.)");
        var matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            string piece = match.Value;

            if (piece.StartsWith("<") && piece.EndsWith(">"))
            {
                if (piece.StartsWith("</"))
                {
                    if (openTags.Count > 0) openTags.Pop();
                    temp += piece;
                }
                else
                {
                    openTags.Push(piece);
                    temp += piece;
                }
            }
            else
            {
                temp += piece;
                string displayString = temp;
                foreach (var tag in openTags)
                {
                    string tagName = Regex.Match(tag, @"<(\w+)").Groups[1].Value;
                    displayString += $"</{tagName}>";
                }
                textList.Add(displayString);
            }
        }

        if (textList.Count == 0 || textList[textList.Count - 1] != temp)
            textList.Add(temp);
    }

    public async UniTask PlayAsync(CancellationToken ct)
    {
        uiText.text = "";
        try
        {
            foreach (var str in textList)
            {
                if (ct.IsCancellationRequested) break;
                uiText.text = str;
                await UniTask.Delay(TimeSpan.FromSeconds(delayPerChar), cancellationToken: ct);
            }
        }
        catch (OperationCanceledException)
        {
            uiText.text = fullText;
        }
    }

    public void Reset() => uiText.text = fullText;
}


#region CursorBlinker
public class TextCursorBlinker
{
    private Text uiText;
    private string fullText;
    private float blinkSpeed;

    public TextCursorBlinker(Text text, float cursorBlinkSpeed = 0.5f)
    {
        uiText = text;
        fullText = uiText.text; // 생성 시 fullText 저장
        blinkSpeed = cursorBlinkSpeed;
    }


    public async UniTask BlinkAsync(CancellationToken ct)
    {
        uiText.text = fullText;
        while (!ct.IsCancellationRequested)
        {
            uiText.text = fullText + "\n▼";
            await UniTask.Delay(TimeSpan.FromSeconds(blinkSpeed), cancellationToken: ct);
            uiText.text = fullText;
            await UniTask.Delay(TimeSpan.FromSeconds(blinkSpeed), cancellationToken: ct);
        }
    }

}
#endregion