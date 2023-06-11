using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Color answerBoxColor;

    public bool UseInternet { get; set; } = false;

    public string SaveSlotName { get; set; } = null;
    public string Username { get; set; } = null;

    public Color HighlightedColor => highlightedColor;

    public Color AnswerBoxColor => answerBoxColor;

    public static GlobalSettings i { get; private set; }
    private void Awake()
    {
        i = this;
    }
}
