using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MoveActorAction : CutsceneAction
{
    [SerializeField] CutsceneActor actor;
    [SerializeField] List<Vector2> movePatterns;

    public override IEnumerator Play()
    {
        var character = actor.GetCharacter();
        
        foreach (var moveVec in movePatterns)
        {
            yield return character.Move(moveVec, checkCollision: false);
            if (actor.GetIsPlayer())
                PlayerController.i.OnMoveOver();
        }
    }
}

[System.Serializable]
public class CutsceneActor
{
    [SerializeField] bool isPlayer;
    [SerializeField] Character character;

    public bool GetIsPlayer() => isPlayer;
    public Character GetCharacter() => (isPlayer) ? PlayerController.i.Character : character;
}
