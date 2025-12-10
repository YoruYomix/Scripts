using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class UniTaskTypewriter : MonoBehaviour
{
    public Text uiText;
    public string fullText;
    public float delay = 0.002f;
    public float cursorBlinkSpeed = 0.5f;

    private List<string> textList = new List<string>();
    private bool isPlaying = false;

    public void Initialize(Text text)
    {
        if (fullText != null)
        {
            Reload();
            return;
        }
        uiText = text;
        fullText = uiText.text;
        if (uiText != null)
        {
            CreateListFromText(fullText);
        }
        uiText.text = "";
    }

    void Reload()
    {
        uiText.text = "";
    }

    void CreateListFromText(string input)
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

    // 타자기 재생
    public async UniTask PlayTextAsync(CancellationToken token)
    {
        try
        {
            foreach (var str in textList)
            {
                uiText.text = str;
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            // 클릭으로 중단됨
            // Stop();
        }
    }

    // 커서 깜빡임
    public async void PlaytextBlink()
    {
        isPlaying = true;
        while (isPlaying)
        {
            uiText.text = fullText + "\n▼";
            await UniTask.Delay(System.TimeSpan.FromSeconds(cursorBlinkSpeed));
            uiText.text = fullText;
            await UniTask.Delay(System.TimeSpan.FromSeconds(cursorBlinkSpeed));
        }
    }

    // 재생 종료
    public void Stop()
    {
        isPlaying = false;
        uiText.text = fullText;
    }


}
