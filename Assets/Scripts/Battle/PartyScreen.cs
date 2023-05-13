using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];

    bool presionadoHorizontal = false;
    bool presionadoVertical = false;

    //PartyScreen puede ser llamado por diferentes estados como ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        //selection = 0;

        for (int i=0; i<memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Elije un Pokemon";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
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

        if ((Input.GetKeyDown(KeyCode.Z)) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
        {
            onSelected?.Invoke();
        }
        else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
        {
            onBack?.Invoke();
        }
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

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
