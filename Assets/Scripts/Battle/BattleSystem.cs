using DG.Tweening;
using SimpleInputNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver}
public enum BattleAction {Move, SwitchPokemon, UseItem, Run}

public enum BattleTrigger { LongGrass, Water}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSrite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    public event Action<bool> onBattleOver;

    BattleState state;
    
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    bool presionadoHorizontal = false;
    bool presionadoVertical = false;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    BattleTrigger battleTrigger;

    List<Pokemon> pokemonShareExp;
    Pokemon pokemonForgetMove;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        currentAction = 0;
        currentMove = 0;
        pokemonShareExp = new List<Pokemon>();
        pokemonForgetMove = null;

        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        battleTrigger = trigger;

        StartCoroutine(SetupBattle());
    }

    public void StarTrainertBattle(PokemonParty playerParty, PokemonParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        currentAction = 0;
        currentMove = 0;
        pokemonShareExp = new List<Pokemon>();
        pokemonForgetMove = null;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        backgroundImage.sprite = (battleTrigger == BattleTrigger.LongGrass) ? grassBackground : waterBackground;

        if (!isTrainerBattle)
        {
            //Pokemon salvaje
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Pokemon.Base.Name} salvaje apareció!");
        }
        else
        {
            //Batalla

            //Mostrar entrenadores
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"¡A luchar contra {trainer.Name}!");

            //Enviar primer pokemon de entrenador
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"¡{trainer.Name} envía a {enemyPokemon.Base.Name}!");

            //Enviar primer pokemon del jugador
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"¡Vamos {playerPokemon.Base.Name}!");
            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
        }

        pokemonShareExp.Add(playerUnit.Pokemon);
        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        onBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Elige una acción");
        dialogBox.EnableActionSelector(true);
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
        dialogBox.EnableActionSelector(false);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"¡{trainer.Name} va a enviar a {newPokemon.Base.Name}! ¿Deseas cambiar de pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Elige el movimiento que deseas olvidar.");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Checkear quien va primero
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //Primer Turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            //yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                //Segundo Turno
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
            yield return RunAfterTurn(firstUnit);
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                //Esto esta manejado desde la pantalla de Items, no hace nada y pasa al turno enemigo
                dialogBox.EnableActionSelector(false);
            }
            else if(playerAction == BattleAction.Run)
            {
                dialogBox.EnableActionSelector(false);
                yield return TryToEscape();
            }

            //Turno enemigo
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        string enemigo = "";
        if (!sourceUnit.IsPlayerUnit) enemigo = " enemigo";
        yield return dialogBox.TypeDialog($"¡{sourceUnit.Pokemon.Base.Name}{enemigo} usó {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {

            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            if (move.Base.Category == MoveCategory.Estado)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target, sourceUnit.IsPlayerUnit);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target, sourceUnit.IsPlayerUnit);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"¡Falló el ataque de {sourceUnit.Pokemon.Base.Name}{enemigo}!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget, bool isPlayerUnit)
    {
        //Boosteo stats
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts, isPlayerUnit);
            else
                target.ApplyBoosts(effects.Boosts, !isPlayerUnit);
        }

        //Estados
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status, !isPlayerUnit);
        }

        //Estados volatiles
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus, !isPlayerUnit);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(()=> state == BattleState.RunningTurn);

        //Estados como quemadura o envenenamiento dañaran al pokemon despues de cada turno
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Precision];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f /3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        string enemy = "";
        if (!faintedUnit.IsPlayerUnit) enemy = " enemigo";

        yield return dialogBox.TypeDialog($"¡{faintedUnit.Pokemon.Base.Name}{enemy} se debilitó!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                battleWon = trainerParty.GetHealthyPokemon() == null;

            if (battleWon)
                AudioManager.i.PlayMusic(battleVictoryMusic);
            
            //Ganar experiencia
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            Debug.Log("Experiencia total: " + expGain);

            //Repartir Experiencia
            int expGainShare = Mathf.FloorToInt(expGain / pokemonShareExp.Count);
            Debug.Log("Pokemon Total Repartir: " + pokemonShareExp.Count);
            foreach (var pokemon in pokemonShareExp)
            {
                if (pokemon == playerUnit.Pokemon)
                    continue;

                pokemon.Exp += expGainShare;
                yield return dialogBox.TypeDialog($"{pokemon.Base.Name} ganó {expGainShare} de Exp.");

                while (pokemon.CheckForLevelUp())
                {
                    yield return dialogBox.TypeDialog($"¡{pokemon.Base.Name} subió al nivel {pokemon.Level}!");

                    //Aprender un nuevo ataque
                    var newMove = pokemon.GetLearnableMoveAtCurrLevel();
                    if (newMove != null)
                    {
                        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                        {
                            pokemon.LearnMove(newMove.Base);
                            yield return dialogBox.TypeDialog($"¡{pokemon.Base.Name} aprendió {newMove.Base.Name}!");
                        }
                        else
                        {
                            //Olvidar movimiento
                            pokemonForgetMove = pokemon;
                            yield return dialogBox.TypeDialog($"{pokemon.Base.Name} intenta aprender {newMove.Base.Name}.");
                            yield return dialogBox.TypeDialog($"Pero {pokemon.Base.Name} no puede aprender más de {PokemonBase.MaxNumOfMoves} movimientos.");
                            yield return ChooseMoveToForget(pokemon, newMove.Base);
                            yield return new WaitUntil(() => state != BattleState.MoveToForget);
                            yield return new WaitForSeconds(2f);
                        }
                    }
                }
            }

            playerUnit.Pokemon.Exp += expGainShare;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} ganó {expGainShare} de Exp.");
            yield return playerUnit.Hud.SetExpSmooth();

            //Subir de nivel
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"¡{playerUnit.Pokemon.Base.Name} subió al nivel {playerUnit.Pokemon.Level}!");

                //Aprender un nuevo ataque
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"¡{playerUnit.Pokemon.Base.Name} aprendió {newMove.Base.Name}!");
                        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        //Olvidar movimiento
                        pokemonForgetMove = playerUnit.Pokemon;
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} intenta aprender {newMove.Base.Name}.");
                        yield return dialogBox.TypeDialog($"Pero {playerUnit.Pokemon.Base.Name} no puede aprender más de {PokemonBase.MaxNumOfMoves} movimientos.");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                    dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            //Muere un pokemon del jugador
            pokemonShareExp.Remove(faintedUnit.Pokemon);
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                //Muere el pokemon salvaje
                BattleOver(true);
            }
            else
            {
                //Muere un pokemon de entrenador rival
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    StartCoroutine(AboutToUse(nextPokemon));
                else
                    BattleOver(true);
            }
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };
            
            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //No aprender nuevo movimiento
                    StartCoroutine(dialogBox.TypeDialog($"{pokemonForgetMove.Base.Name} no aprendió {moveToLearn.Name}."));
                }
                else
                {
                    //Olvidar el movimiento y aprender el nuevo
                    var selectedMove = pokemonForgetMove.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{pokemonForgetMove.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}."));

                    pokemonForgetMove.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Mochila
                //StartCoroutine(RunTurns(BattleAction.UseItem));
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Huir
                StartCoroutine(RunTurns(BattleAction.Run));
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
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;
            
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                //partyScreen.SetMessageText($"No puedes sacar un pokemon debilitado");
                StartCoroutine(partyScreen.SetMessageAndReset("No puedes sacar un pokemon debilitado"));
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                //partyScreen.SetMessageText($"No puedes cambiar al mismo pokemon");
                StartCoroutine(partyScreen.SetMessageAndReset("No puedes cambiar al mismo pokemon"));
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                currentAction = 0;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                //partyScreen.SetMessageText("Debes elegir un pokemon para continuar");
                StartCoroutine(partyScreen.SetMessageAndReset("Debes elegir un pokemon para continuar"));
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };
        
        partyScreen.HandleUpdate(onSelected, onBack);

    }

    void HandleAboutToUse()
    {
        if (((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) || (Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0))) && !presionadoVertical)
        {
            presionadoVertical = true;
            aboutToUseChoice = !aboutToUseChoice;
        }

        if (SimpleInput.GetAxisRaw("Vertical") == 0)
        {
            presionadoVertical = false;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if ((Input.GetKeyDown(KeyCode.Z)) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
        {
            dialogBox.EnableChoiceBox(false);
            if(aboutToUseChoice == true)
            {
                //Sí
                OpenPartyScreen();
            }
            else
            {
                //No
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"¡Regresa {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        if (!pokemonShareExp.Contains(newPokemon)) pokemonShareExp.Add(newPokemon);
        dialogBox.SetMovesNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"¡Vamos {newPokemon.Base.Name}!");

        if(isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"¡{trainer.Name} envía a {nextPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            dialogBox.EnableActionSelector(false);
            yield return ThrowPokeball((PokeballItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"¡No puedes robar pokemon de entrenadores!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"¡{player.Name} usó {pokeballItem.Name}!");

        var pokeballObj = Instantiate(pokeballSrite, playerUnit.transform.position - new Vector3(2,-2), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.InGameSprite;

        //Animaciones
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 4.6f), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 0.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);
        
        for (int i=0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            //Captura
            yield return dialogBox.TypeDialog($"¡Ya está! ¡{enemyUnit.Pokemon.Base.Name} atrapado!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"¡{enemyUnit.Pokemon.Base.Name} ha sido añadido al equipo!");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //Rompe pokeball
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"¡{enemyUnit.Pokemon.Base.Name} escapó!");
            else
                yield return dialogBox.TypeDialog($"¡Vaya! ¡Parecía que los habías atrapado!");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeballItem.CathRateModifier * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"¡No puedes huir de una batalla con un entrenador!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"¡{player.Name} escapó del combate!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"¡{player.Name} escapó del combate!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"¡No puedes escapar!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
