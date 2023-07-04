using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Color answerBoxColor;
    [SerializeField] Color answerCorrect;
    [SerializeField] Color answerWrong;
    [SerializeField] int timeQuestions = 5;

    //Configuracion Zonas
    public bool goCave { get; set; } = false;
    public Actividad actCave { get; set; }
    public bool goLake { get; set; } = false;
    public Actividad actLake { get; set; }
    public bool goForest { get; set; } = false;
    public Actividad actForest { get; set; }
    public bool goMountain { get; set; } = false;
    public Actividad actMountain { get; set; }
    public bool goCity { get; set; } = false;
    public Actividad actCity { get; set; }

    public bool UseInternet { get; set; } = true;

    public bool Cargar { get; set; } = false;

    public string SaveSlotName { get; set; } = null;
    public string Username { get; set; } = null;
    public string FirstName { get; set; } = null;
    public string Lastname { get; set; } = null;

    public string[] slots { get; set; } = null;

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

    public string CreateCodeConfiguration()
    {
        string code = "";
        if (goCave) code += "1";
        else code += "0";
        code += Convert.ToInt32(actCave);
        if (goLake) code += "1";
        else code += "0";
        code += Convert.ToInt32(actLake);
        if (goForest) code += "1";
        else code += "0";
        code += Convert.ToInt32(actForest);
        if (goMountain) code += "1";
        else code += "0";
        code += Convert.ToInt32(actMountain);
        if (goCity) code += "1";
        else code += "0";
        code += Convert.ToInt32(actCity);

        return code;
    }
}
