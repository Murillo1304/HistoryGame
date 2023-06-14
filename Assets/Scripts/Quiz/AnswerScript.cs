using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect { get; set; } = false;
    public bool coroutineRunning { get; set; } = false;
    [SerializeField] QuizUI quizUI;

    public void Answer()
    {
        if (!coroutineRunning)
        {
            if (isCorrect)
            {
                GetComponent<Image>().color = GlobalSettings.i.AnswerCorrect;
                Debug.Log("Correcto");
                quizUI.Correct();
            }
            else
            {
                GetComponent<Image>().color = GlobalSettings.i.AnswerWrong;
                Debug.Log("Incorrecto");
                quizUI.Wrong();
            }
        }
    }
}
