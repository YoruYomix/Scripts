using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PageOnclickActionSequenceText : PageOnclickActionSequenceBase
{
    private Text _text;
    private UniTaskTypewriter writer;

    protected override void InitAction()
    {
        _text = GetComponent<Text>();
        if (_text == null) return;
        if (writer == null)
        {
            writer = gameObject.AddComponent<UniTaskTypewriter>();
        }
        writer.Initialize(_text);
    }

    async public override UniTask PlaySequence(CancellationToken token)
    {
        await base.PlaySequence(token);
        if (writer != null)
            await writer.PlayTextAsync(token);
        CompleteSequence();
    }

    protected override void CompleteAction()
    {
        writer.Stop();
    }

    protected override void CompleteInstantlyAction()
    {
        writer.Stop();
    }

    protected override void StandByAction()
    {
        writer.PlaytextBlink();
    }
}
