using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] QuizUI quiz;
    [SerializeField] QuizUI quizImage;

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

        bool containsImage = quizList.Any(item => item.ImageName != null);

        if (!containsImage)
            quiz.ShowQuiz(quizList);
        else
            quizImage.ShowQuiz(quizList);
    }

    public void Close()
    {
        quizEnd?.Invoke();
        quiz.gameObject.SetActive(false);
        quizImage.gameObject.SetActive(false);
    }
}
