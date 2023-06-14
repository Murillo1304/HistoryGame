using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassCheckerFov : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterPassCheckerView(GetComponentInParent<CheckPassController>());
    }

    public bool TriggerRepeatedly => false;
}
