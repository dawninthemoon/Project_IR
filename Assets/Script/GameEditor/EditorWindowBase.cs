using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorWindowBase : MonoBehaviour
{
    public virtual void updateHotKey()
    {

    }

    public virtual void mainUpdate(float deltaTime)
    {

    }

    public void activeWindow(bool value)
    {
        gameObject.SetActive(value);
    }
}
