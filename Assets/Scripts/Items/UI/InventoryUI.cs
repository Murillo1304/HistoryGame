using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] Color noItemColor;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;

    MoveBase moveToLearn;

    bool presionadoVertical = false;
    bool presionadoHorizontal = false;
    InventoryUIState state;

    const int itemsInViewport = 7;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        itemIcon.sprite = null;
        itemDescription.text = null;

        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        //Limpiar todos los items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj =  Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed= onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                ++selectedItem;
            }
            else if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
            {
                presionadoVertical = true;
                --selectedItem;
            }
            else if ((Input.GetKeyDown(KeyCode.RightArrow) || (SimpleInput.GetAxisRaw("Horizontal") > 0)) && !presionadoHorizontal)
            {
                presionadoHorizontal = true;
                ++selectedCategory;
            }
            else if ((Input.GetKeyDown(KeyCode.LeftArrow) || (SimpleInput.GetAxisRaw("Horizontal") < 0)) && !presionadoHorizontal)
            {
                presionadoHorizontal = true;
                --selectedCategory;
            }

            if (SimpleInput.GetAxisRaw("Vertical") == 0)
            {
                presionadoVertical = false;
            }

            if (SimpleInput.GetAxisRaw("Horizontal") == 0)
            {
                presionadoHorizontal = false;
            }

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            var countItems = inventory.GetSlotsByCategory(selectedCategory).Count;
            if (countItems > 0)
                selectedItem = Mathf.Clamp(selectedItem, 0, countItems - 1);
            else
                selectedItem = -1;

            if(prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            } 
            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if ((Input.GetKeyDown(KeyCode.Z)) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
            {
                if (selectedItem > -1)
                    StartCoroutine(ItemSelected());
            }
            else if ((Input.GetKeyDown(KeyCode.X)) || CrossPlatformInputManager.GetButtonDown("ButtonB"))
                onBack?.Invoke();
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                //Usar item en el pokemon
                StartCoroutine(UseItem());
                
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSlected = (int moveIndex) =>
            {
                moveSelectionUI.state = MoveSelectionState.Busy;
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };
            
            moveSelectionUI.HandleMoveSelection(onMoveSlected);
        }
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameController.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }
        
        if(GameController.Instance.State == GameState.Battle)
        {
            //En batalla
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"No puedes usar esto en batalla");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            //Fuera de batalla
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"No puedes usar esto fuera de una batalla");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            var playerParty = PlayerController.i.GetComponent<PokemonParty>();
            if (playerParty.Pokemons.Count > 0)
            {
                OpenPartyScreen();

                if (item is TmItem)
                    partyScreen.ShowIfTmIsUsable(item as TmItem);
            }
            else
            {
                StartCoroutine(DialogManager.Instance.ShowDialogText($"¡No tienes ningún pokemon!"));
                state = InventoryUIState.ItemSelection;
            }
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var pokemon = partyScreen.SelectedMember;

        //Objetos Evolucionadores
        if (item is EvolutionItem)
        {
            var evolution = pokemon.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(pokemon, evolution);
            }
            else
            {
                yield return partyScreen.ShowDialogText($"¡No tiene ningún efecto!");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if(usedItem is RecoveryItem)
                yield return partyScreen.ShowDialogText($"¡{partyScreen.SelectedMember.Base.Name} {usedItem.Message}!");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.Items)
                yield return partyScreen.ShowDialogText($"¡No tiene ningún efecto!");
        }

        ClosePartyScreen();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
           yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return partyScreen.ShowDialogText($"¡{pokemon.Base.Name} ya sabe {tmItem.Move.Name}!");
            yield break;
        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return partyScreen.ShowDialogText($"¡{pokemon.Base.Name} no puede aprender {tmItem.Move.Name}!");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return partyScreen.ShowDialogText($"¡{pokemon.Base.Name} aprendió {tmItem.Move.Name}!");
        }
        else
        {
            yield return partyScreen.ShowDialogText($"{pokemon.Base.Name} intenta aprender {tmItem.Move.Name}.");
            yield return partyScreen.ShowDialogText($"Pero {pokemon.Base.Name} no puede aprender más de {PokemonBase.MaxNumOfMoves} movimientos.");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return partyScreen.ShowDialogText($"Elige el movimiento que deseas olvidar.");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (slots.Count > 0)
        {
            itemIcon.color = Color.white;
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;

        }
        else
        {
            itemIcon.color = noItemColor;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;
        
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection()
    {
        selectedItem = 0;

        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;

        partyScreen.ClearMemberSlotMessages();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            //No aprender nuevo movimiento
            yield return partyScreen.ShowDialogText($"{pokemon.Base.Name} no aprendió {moveToLearn.Name}.");
        }
        else
        {
            //Olvidar el movimiento y aprender el nuevo
            var selectedMove = pokemon.Moves[moveIndex].Base;
            yield return partyScreen.ShowDialogText($"{pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}.");

            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
        moveSelectionUI.state = MoveSelectionState.MoveSelection;
    }
}
