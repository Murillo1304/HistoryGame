using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] Text slotNumberText;
    [SerializeField] Text nameText;
    [SerializeField] Text configurationText;

    string slotName;

    public void SetData(string number, string name, string configuration, string slot)
    {
        slotNumberText.text = number;
        nameText.text = name;
        configurationText.text = configuration;
        slotName = slot;
    }

    public void LoadConfiguration()
    {
        GlobalSettings.i.SaveSlotName = slotName;
        GlobalSettings.i.Cargar = true;
        Loader.Load(Scene.Gameplay);
    }
}
