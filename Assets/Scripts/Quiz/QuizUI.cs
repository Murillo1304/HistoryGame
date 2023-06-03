using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizUI : MonoBehaviour
{
    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject resultsPanel;
    [SerializeField] GameObject[] options;

    List<QuestionsAndAnswers> QnA;
    int currentQuestion = 0;

    [SerializeField] Text QuestionTxt;
    [SerializeField] Text ScoreText;

    int totalQuestions = 0;
    int score = 0;

    public void ShowQuiz(List<QuestionsAndAnswers> quiz)
    {
        score= 0;
        QnA = quiz;
        totalQuestions = QnA.Count;
        GenerateQuestion();
        gameObject.SetActive(true);
        quizPanel.SetActive(true);
        resultsPanel.SetActive(false);
    }

    public IEnumerator GameOver()
    {
        quizPanel.SetActive(false);
        resultsPanel.SetActive(true);
        ScoreText.text = "Score: " + score + "/" + totalQuestions;
        yield return new WaitForSeconds(1.5f);
        QuizManager.i.finishQuiz = true;
    }

    public void Correct()
    {
        score += 1;
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitForNext());
    }

    public void Wrong()
    {
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitForNext());
    }

    IEnumerator WaitForNext()
    {
        yield return new WaitForSeconds(1);
        GenerateQuestion();
    }

    void SetAnswer()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].GetComponent<Image>().color = GlobalSettings.i.AnswerBoxColor;
            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];

            if (QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }
        }
    }

    void GenerateQuestion()
    {
        if (QnA.Count > 0)
        {
            currentQuestion = Random.Range(0, QnA.Count);

            QuestionTxt.text = QnA[currentQuestion].Question;
            SetAnswer();
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }
}
