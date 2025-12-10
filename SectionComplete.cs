using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public abstract class SectionComplete : MonoBehaviour
{
    abstract public void Init(SectionObj section);

    abstract public void Open(Transform transform);

    abstract public void CloseComplateUI(Action action);

}
