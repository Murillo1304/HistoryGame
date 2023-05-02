using SimpleInputNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum BattleState {Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> onBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    public void StartBattle()
    {
       StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Pokemon.Base.Name} salvaje apareció!");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Elije una acción"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"¡{playerUnit.Pokemon.Base.Name} usó {move.Base.Name}!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"¡{enemyUnit.Pokemon.Base.Name} enemigo se debilitó!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            onBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return dialogBox.TypeDialog($"¡{enemyUnit.Pokemon.Base.Name} enemigo usó {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"¡{playerUnit.Pokemon.Base.Name} enemigo se debilitó!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            onBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("¡Un golpe crítico!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("¡Es muy eficaz!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("No es muy eficaz...");
    }

    public void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if ((Input.GetKeyDown(KeyCode.Z)) || (CrossPlatformInputManager.GetButtonDown("ButtonA")))
        {
            if (currentAction == 0)
            {
                //Luchar
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Huir
            }
        }
    }

    void HandleMoveSelection()
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow)))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
            {
                Debug.Log("currenMove right: " + currentMove);
                currentMove++;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.LeftArrow)))
        {
            if (currentMove > 0)
            {
                Debug.Log("currenMove left: " + currentMove);
                --currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
