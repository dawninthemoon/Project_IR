using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgressBase
{
    public void Initialize();
    public bool Progress(float deltaTime);
    public void Release();

}
