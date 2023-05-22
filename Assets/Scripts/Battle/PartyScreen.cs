using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum PartyState { Free, Dialog }

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] int letterPerSecond;
    [SerializeField] ChoiceBox choiceBox;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    PokemonParty party;

    int selection = 0;
    int selectionChangeOrder = 0;

    public Pokemon SelectedMember => pokemons[selection];

    bool presionadoHorizontal = false;
    bool presionadoVertical = false;

    public event Action OnChangePokemon;
    public event Action OnChangePokemonFinish;

    //PartyScreen puede ser llamado por diferentes estados como ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }
    public PartyState state = PartyState.Free;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = PokemonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = party.Pokemons;
        //selection = 0;

        for (int i=0; i<memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Elige un Pokemon";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if (state == PartyState.Free)
        {
            var prevSelection = selection;

            if ((Input.GetKeyDown(KeyCode.RightArrow) || (SimpleInput.GetAxisRaw("Horizontal") > 0)) && !presionadoHorizontal)
            {
                presionadoHorizontal = true;
                ++selection;
            }
            else if ((Input.GetKeyDown(KeyCode.LeftArrow) || (SimpleInput.GetAxisRaw("Horizontal") < 0)) && !presionadoHorizontal)
            {
                presionadoHorizontal = true;
                --selection;
            }
            else if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                selection += 2;
            }
            else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                selection -= 2;
            }

            if (SimpleInput.GetAxisRaw("Horizontal") == 0)
            {
                presionadoHorizontal = false;
            }

            if (SimpleInput.GetAxisRaw("Vertical") == 0)
            {
                presionadoVertical = false;
            }

            selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

            if (selection != prevSelection)
                UpdateMemberSelection(selection);

            if ((Input.GetKeyDown(KeyCode.Z) || CrossPlatformInputManager.GetButtonDown("ButtonA")))
            {
                onSelected?.Invoke();
            }
            else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
            {
                onBack?.Invoke();
            }
        }
    }

    public IEnumerator SelectPokemon()
    {
        int selectedChoice = 0;

        yield return ShowDialogText("Selecciona una acción",
            choices: new List<string>() { "Cambiar", "Salir" }, 
            waitForInput: false,
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Sí
            messageText.text = "Cambiar el orden de tu pokemon";
            OnChangePokemon?.Invoke();
            selectionChangeOrder = selection;

        }
        else if (selectedChoice == 1)
        {
            //No
            messageText.text = "Elige un pokemon";
        }
    }

    public void SwitchPokemon()
    {
        party.SwitchOrder(selectionChangeOrder, selection);
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "PUEDE" : "NO PUEDE";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public IEnumerator SetMessageAndReset(string message)
    {
        messageText.text = message;
        yield return new WaitForSeconds(1f);
        messageText.text = "Elige un pokemon";
    }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        state = PartyState.Dialog;

        yield return TypeDialog(text);

        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || CrossPlatformInputManager.GetButtonDown("ButtonA"));
            messageText.text = "Elige un pokemon";
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        state = PartyState.Free;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        messageText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            messageText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }

        yield return new WaitForSeconds(1f / 3);
    }
}
