using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    string saveSlotName;

    public string SaveSlotName { get; set; } = null;

    public Color HighlightedColor => highlightedColor;

    public static GlobalSettings i { get; private set; }
    private void Awake()
    {
        i = this;
    }
}
