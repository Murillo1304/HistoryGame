using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerAction : CutsceneAction
{
    [SerializeField] Vector2 position;

    public override IEnumerator Play()
    {
       yield return PlayerController.i.transform.position = position;
    }
}
