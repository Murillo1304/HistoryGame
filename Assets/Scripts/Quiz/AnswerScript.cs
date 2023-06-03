using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect { get; set; } = false;
    [SerializeField] QuizUI quizUI;

    public Color startColor {get; set;}

    public void Answer()
    {
        if (isCorrect)
        {
            GetComponent<Image>().color = Color.green;
            Debug.Log("Correcto");
            quizUI.Correct();
        }
        else
        {
            GetComponent<Image>().color = Color.red;
            Debug.Log("Incorrecto");
            quizUI.Wrong();
        }
    }
}
