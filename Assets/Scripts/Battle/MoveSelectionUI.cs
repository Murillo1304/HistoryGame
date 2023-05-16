using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum MoveSelectionState { MoveSelection, Busy }

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color highlightedColor;

    int currentSelection = 0;

    bool presionadoVertical = false;

    public MoveSelectionState state { get; set; } = MoveSelectionState.MoveSelection;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; ++i)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (state == MoveSelectionState.MoveSelection)
        {

            if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                ++currentSelection;
            }
            else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                --currentSelection;
            }

            if (SimpleInput.GetAxisRaw("Vertical") == 0)
                presionadoVertical = false;

            currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfMoves);

            UpdateMoveSelection(currentSelection);

            if (Input.GetKeyDown(KeyCode.Z) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
                onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves + 1; i++)
        {
            if (i == selection)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;
        }
    }
}
