using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public enum GameState {FreeRoam, Battle, Dialog, Menu, PartyScreen, ChangeOrder, Bag, Cutscene, Paused, Evolution, Shop , Quiz}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] GameObject buttonMenu;
    [SerializeField] GameObject buttonA;
    [SerializeField] GameObject buttonB;
    [SerializeField] GameObject buttonDpad;
    
    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;
    QuizManager quizManager;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();
        quizManager = GetComponent<QuizManager>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        battleSystem.onBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnDialogFinished += () =>
        {
            if(state == GameState.Dialog)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;

        quizManager.quizStart += () => state = GameState.Quiz;
        quizManager.quizEnd += () =>
        {
            ActivateButtons();
            state = GameState.FreeRoam;
        };

        EvolutionManager.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };

        EvolutionManager.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;

            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FreeRoam;

        partyScreen.OnChangePokemon += () => state = GameState.ChangeOrder;
        partyScreen.OnChangePokemonFinish += () => state = GameState.PartyScreen;



        if (GlobalSettings.i.SaveSlotName != null)
        {
            //Cargar
            SavingSystem.i.Load(GlobalSettings.i.SaveSlotName);
            state = GameState.FreeRoam;
        }
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartCutsceneState()
    {
        state = GameState.Cutscene;
    }

    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void StartBattle(BattleTrigger trigger)
    {
        state = GameState.Battle;
        buttonMenu.SetActive(false);
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(trigger);

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy, trigger);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        buttonMenu.SetActive(false);
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

        partyScreen.SetPartyData();
        
        state = GameState.FreeRoam;
        buttonMenu.SetActive(true);
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        if (won == true)
        {
            var playerParty = playerController.GetComponent<PokemonParty>();
            bool hasEvolutions = playerParty.CheckForEvolutions();

            if (hasEvolutions)
                StartCoroutine(playerParty.RunEvolutions());
            else
                AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        }
        else
        {
            StartCoroutine(Defeat());
        }
    }

    private IEnumerator Defeat()
    {
        yield return Fader.i.FadeIn(0.5f);
        playerController.transform.position = playerController.positionHealer;
        playerController.Character.Animator.SetFacingDirection(FacingDirection.Down);
        AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        var playerParty = playerController.GetComponent<PokemonParty>();
        playerParty.Pokemons.ForEach(p => p.Heal());
        playerParty.PartyUpdated();
        yield return Fader.i.FadeOut(0.5f);
        yield return DialogManager.Instance.ShowDialogText($"¡Tus pokemon se han recuperado!");
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
                playerController.Character.Animator.IsMoving = false;
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
        else if (state == GameState.Cutscene)
        {
            playerController.Character.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            playerController.Character.SpeedDown();
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            playerController.Character.SpeedDown();
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            playerController.Character.SpeedDown();
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //Mostrar Eleccion de accion
                StartCoroutine(partyScreen.SelectPokemon());
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.ChangeOrder)
        {
            Action onSelected = () =>
            {
                //Mostrar Pokemon Seleccionado
                partyScreen.SwitchPokemon();
                state = GameState.PartyScreen;
            };

            Action onBack = () =>
            {
                partyScreen.SetMessageText("Elige un pokemon");
                state = GameState.PartyScreen;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
        else if (state == GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }
        else if (state == GameState.Quiz)
        {
            DeactivateButtons();
        }
    }

    public void DeactivateButtons()
    {
        buttonMenu.SetActive(false);
        buttonA.SetActive(false);
        buttonB.SetActive(false);
        buttonDpad.SetActive(false);
        Fader.i.gameObject.SetActive(false);
    }

    public void ActivateButtons()
    {
        buttonMenu.SetActive(true);
        buttonA.SetActive(true);
        buttonB.SetActive(true);
        buttonDpad.SetActive(true);
        Fader.i.gameObject.SetActive(true);
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
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectItem == 1)
        {
            //Mochila
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectItem == 2)
        {
            //Guardar
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectItem == 3)
        {
            //Cargar
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut=false)
    {
        yield return Fader.i.FadeIn(0.5f);
        
        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (waitForFadeOut)
            yield return Fader.i.FadeOut(0.5f);
        else
            StartCoroutine(Fader.i.FadeOut(0.5f));
    }

    public GameState State => state;
}
