using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public enum GameState {FreeRoam, Battle, Dialog, Menu, Cutscene, Paused}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;

    GameState stateBeforePause;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.onBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if(state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StarTrainertBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HangleUpdate();

            if ((Input.GetKeyDown(KeyCode.Return)) || CrossPlatformInputManager.GetButtonDown("ButtonMenu"))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }

            if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
            {
                playerController.Character.SpeedUp();
            }
            else if ((Input.GetKeyUp(KeyCode.X)) || CrossPlatformInputManager.GetButtonUp("ButtonB"))
            {
                playerController.Character.SpeedDown();
            }
        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectItem)
    {
        if (selectItem == 0)
        {
            //Pokemon
        }
        else if (selectItem == 1)
        {
            //Mochila
        }
        else if (selectItem == 2)
        {
            //Guardar
            SavingSystem.i.Save("saveSlot1");
        }
        else if (selectItem == 3)
        {
            //Cargar
            SavingSystem.i.Load("saveSlot1");
        }

        state = GameState.FreeRoam;
    }
}
