using SimpleInputNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum BattleState {Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> onBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    bool presionadoHorizontal = false;
    bool presionadoVertical = false;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Pokemon.Base.Name} salvaje apareció!");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Elije una acción");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
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
        move.PP--;
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
        move.PP--;
        yield return dialogBox.TypeDialog($"¡{enemyUnit.Pokemon.Base.Name} enemigo usó {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"¡{playerUnit.Pokemon.Base.Name} se debilitó!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(1f);

            var nextPokemon = playerParty.GetHealthyPokemon();

            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                onBattleOver(false);
            }
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
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow) || (SimpleInput.GetAxisRaw("Horizontal") > 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            ++currentAction;
        }
        else if ((Input.GetKeyDown(KeyCode.LeftArrow) || (SimpleInput.GetAxisRaw("Horizontal") < 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            --currentAction;
        }
        else if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentAction += 2;
        }
        else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        if (SimpleInput.GetAxisRaw("Horizontal") == 0)
        {
            presionadoHorizontal = false;
        }

        if (SimpleInput.GetAxisRaw("Vertical") == 0)
        {
            presionadoVertical = false;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
        {
            if (currentAction == 0)
            {
                //Luchar
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Mochila
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Huir
            }
        }
    }

    void HandleMoveSelection()
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow) || (SimpleInput.GetAxisRaw("Horizontal") > 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            ++currentMove;
        }
        else if ((Input.GetKeyDown(KeyCode.LeftArrow) || (SimpleInput.GetAxisRaw("Horizontal") < 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            --currentMove;
        }
        else if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentMove += 2;
        }
        else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        if (SimpleInput.GetAxisRaw("Horizontal") == 0)
        {
            presionadoHorizontal = false;
        }

        if (SimpleInput.GetAxisRaw("Vertical") == 0)
        {
            presionadoVertical = false;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if ((Input.GetKeyDown(KeyCode.Z)) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection()
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow) || (SimpleInput.GetAxisRaw("Horizontal") > 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            ++currentMember;
        }
        else if ((Input.GetKeyDown(KeyCode.LeftArrow) || (SimpleInput.GetAxisRaw("Horizontal") < 0)) && !presionadoHorizontal)
        {
            presionadoHorizontal = true;
            --currentMember;
        }
        else if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentMember += 2;
        }
        else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        if (SimpleInput.GetAxisRaw("Horizontal") == 0)
        {
            presionadoHorizontal = false;
        }

        if (SimpleInput.GetAxisRaw("Vertical") == 0)
        {
            presionadoVertical = false;
        }

        partyScreen.UpdateMemberSelection(currentMember);

        if ((Input.GetKeyDown(KeyCode.Z)) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes sacar un pokemon debilitado");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("No puedes cambiar con el mismo pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Regresa {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);
        dialogBox.SetMovesNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"¡Vamos {newPokemon.Base.Name}!");

        StartCoroutine(EnemyMove());
    }
}
