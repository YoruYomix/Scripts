using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Replay
{
    List<Page> replays;
    // 현재 리플레이에서 바라보는 위치
    private int replayIndex;
    int createIndex;

    public Replay(List<Page> _replayRecode)
    {
        replays = _replayRecode;
        createIndex = replays.Count - 1;
        replayIndex = replays.Count;
    }
    


    public bool IsLastPage
    {
        get
        {
            return (replayIndex <= 0);
        }
    }

    public bool IsFirstPage
    {
        get
        {
            return (replayIndex == createIndex);
        }
    }

    // 가장 최신 페이지부터 과거 페이지로 이동
    public Page PrevReplay()
    {
        if (replayIndex > 0)
        {
            replayIndex--;
            return replays[replayIndex];
        }

        // 더 이전 페이지가 없음
        return null;
    }

    // 과거에서 다시 최신으로 이동
    public Page NextReplay()
    {
        if (replayIndex < replays.Count - 1)
        {
            replayIndex++;
            return replays[replayIndex];
        }

        // 이미 최신임
        return null;
    }


}
