using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveKeyItemAction : CutsceneAction
{
    [SerializeField] KeyItem passCave;
    [SerializeField] KeyItem passLake;
    [SerializeField] KeyItem passForest;
    [SerializeField] KeyItem passMountain;
    [SerializeField] KeyItem passCity;

    List<KeyItem> items;

    public override IEnumerator Play()
    {
        CreatePassList();
        foreach (ItemBase itemGive in items)
        {
            Debug.Log(itemGive.Name);
            PlayerController.i.GetComponent<Inventory>().AddItem(itemGive, 1);

            AudioManager.i.PlaySfx(AudioId.ItemObtained, pauseMusic: true);

            string dialogText = $"¡{PlayerController.i.Name} recibió {itemGive.Name}!";

            yield return DialogManager.Instance.ShowDialogText(dialogText);
        }
    }

    public void CreatePassList()
    {
        items = new List<KeyItem>();
        if (GlobalSettings.i.goCave)
            items.Add(passCave);
        if (GlobalSettings.i.goLake)
            items.Add(passLake);
        if (GlobalSettings.i.goForest)
            items.Add(passForest);
        if (GlobalSettings.i.goMountain)
            items.Add(passMountain);
        if (GlobalSettings.i.goCity)
            items.Add(passCity);
    }
}
