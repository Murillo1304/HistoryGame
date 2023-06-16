using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 5)
        {
            player.Character.Animator.IsMoving = false;
            StartCoroutine(GameController.Instance.StartBattle(BattleTrigger.LongGrass));
        }
    }

    public bool TriggerRepeatedly => true;
}
