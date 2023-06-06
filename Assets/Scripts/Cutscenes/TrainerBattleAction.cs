using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerBattleAction : CutsceneAction
{
    [SerializeField] TrainerController trainer;
    [SerializeField] bool CanLoseBattle = true;
    
    public override IEnumerator Play()
    {
        yield return trainer.CutsceneBattle();
        GameController.Instance.battleCanLose = CanLoseBattle;
        yield return new WaitUntil(() => GameController.Instance.trainer == null);
    }
}
