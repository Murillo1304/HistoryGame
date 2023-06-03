using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] QuizUI quiz;

    public event Action quizStart;
    public event Action quizEnd;

    public bool finishQuiz { get; set; } = false;

    public static QuizManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public void Show(List<QuestionsAndAnswers> quizList)
    {
        finishQuiz = false;
        quizStart?.Invoke();
        quiz.ShowQuiz(quizList);
    }

    public void Close()
    {
        quizEnd?.Invoke();
        quiz.gameObject.SetActive(false);
    }
}
