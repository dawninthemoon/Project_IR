using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleSceneScript : MonoBehaviour
{
    public Animator _titleSceneAnimator;
    public SceneSelector _sceneSelector;
    private bool _active = false;

    public void Activate()
    {
        _active = true;
    }

    public void outTitleScene()
    {
        _titleSceneAnimator.SetTrigger("Out");
    }

    void Update()
    {
        if(_active == false)
            return;

        if(Input.anyKey)
        {
            outTitleScene();
            _active = false;
        }
    }
}
