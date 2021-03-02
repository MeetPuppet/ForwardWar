using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using util;

public class InputManager : Monosingleton<InputManager>
{
    public enum TouchStatus : int
    {
        None = 0,
        Down = 1,
        Up = 2,
        Press = 4
    }

    KeyCode key = default;
    public Action keyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;

    bool pressed = false;

    public void OnUpdate()
    {
        if (MouseAction != null)
        {
            if (Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press);
                Debug.Log("down");
                pressed = true;
            }
            else
            {
                if (pressed)
                {
                    Debug.Log("Press");
                    MouseAction.Invoke(Define.MouseEvent.Click);
                }

                Debug.Log("not Press");
                pressed = false;
            }
        }
        switch (key)
        {
            case KeyCode.None:
                break;
            case KeyCode.Backspace:
                break;
            case KeyCode.Delete:
                break;
            case KeyCode.Space:
                break;
            case KeyCode.UpArrow:
                break;
            case KeyCode.DownArrow:
                break;
            case KeyCode.RightArrow:
                break;
            case KeyCode.LeftArrow:
                break;
            case KeyCode.W:
                break;
            case KeyCode.S:
                break;
            case KeyCode.A:
                break;
            case KeyCode.D:
                break;
            default:
                break;
        }
    }

}
