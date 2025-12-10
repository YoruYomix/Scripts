using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputTable : MonoBehaviour
{
    abstract public void OnScreenClick();
    abstract public void OnRightClick();

    abstract public void OnEnterPressed();
    abstract public void OnSpacePressed();
    abstract public void OnBackspacePressed();
    abstract public void OnEscPressed();

    abstract public void OnPrevBtnCLick();
    abstract public void OnNextBtnCLick();
    abstract public void OnHideBtnCLick();
    abstract public void OnCloseBtnCLick();

}
