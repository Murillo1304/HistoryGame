using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Color answerBoxColor;
    [SerializeField] Color answerCorrect;
    [SerializeField] Color answerWrong;
    [SerializeField] int timeQuestions = 5;

    //Configuracio Zonas
    public bool goCave { get; set; } = true;
    public bool goLake { get; set; } = true;
    public bool goForest { get; set; } = false;
    public bool goMountain { get; set; } = false;
    public bool goCity { get; set; } = false;

    public bool UseInternet { get; set; } = false;

    public string SaveSlotName { get; set; } = null;
    public string Username { get; set; } = null;

    public Color HighlightedColor => highlightedColor;

    public Color AnswerBoxColor => answerBoxColor;

    public Color AnswerCorrect => answerCorrect;
    public Color AnswerWrong => answerWrong;

    public int TimeQuestions => timeQuestions;

    public static GlobalSettings i { get; private set; }
    private void Awake()
    {
        i = this;
    }
}
