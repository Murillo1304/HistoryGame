using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayAction : CutsceneAction
{
    [SerializeField] float seconds;

    public override IEnumerator Play()
    {
        yield return new WaitForSeconds(seconds);
    }
}
