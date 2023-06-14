using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveItemAction : CutsceneAction
{
    [SerializeField] List<GiveItem> items;

    public override IEnumerator Play()
    {
        foreach (GiveItem itemGive in items)
        {
            PlayerController.i.GetComponent<Inventory>().AddItem(itemGive.Item, itemGive.Count);

            AudioManager.i.PlaySfx(AudioId.ItemObtained, pauseMusic: true);

            string dialogText = $"¡{PlayerController.i.Name} recibió {itemGive.Item.Name}!";
            if (itemGive.Count > 1)
                dialogText = $"¡{PlayerController.i.Name} recibió {itemGive.Count} {itemGive.Item.Name}!";

            yield return DialogManager.Instance.ShowDialogText(dialogText);
        }
    }
}

[Serializable]
public class GiveItem
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}
