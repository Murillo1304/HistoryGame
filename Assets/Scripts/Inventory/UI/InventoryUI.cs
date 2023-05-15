using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public enum InventoryUIState { ItemSelection, PartySelection, Busy}

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

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;

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
                presionadoHorizontal= true;
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

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

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
                ItemSelected();

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
    }

    void ItemSelected()
    {
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }

    IEnumerator UseItem()
    {
        var prevState = state;
        state = InventoryUIState.Busy;
        
        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if(!(usedItem is PokeballItem))
                //yield return DialogManager.Instance.ShowDialogText($"Has usado {usedItem.Name}");
                yield return partyScreen.ShowDialogText($"¡{partyScreen.SelectedMember.Base.Name} {usedItem.Message}!");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (prevState == InventoryUIState.PartySelection)
                yield return partyScreen.ShowDialogText($"¡No tiene ningún efecto!");
            else
                yield return DialogManager.Instance.ShowDialogText($"No es momento de usar eso");
        }

        ClosePartyScreen();
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
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
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
        partyScreen.gameObject.SetActive(false);
    }
}
