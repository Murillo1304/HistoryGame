using DG.Tweening;
using DG.Tweening.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizUI : MonoBehaviour
{
    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject resultsPanel;
    [SerializeField] GameObject[] options;
    [SerializeField] GameObject timePanel;
    [SerializeField] GameObject timeBar;

    List<QuestionsAndAnswers> QnA;
    int currentQuestion = 0;

    [SerializeField] Text QuestionTxt;
    [SerializeField] Image QuestionImage;
    [SerializeField] Text ScoreText;

    int totalQuestions = 0;
    int score = 0;

    Tween animacion;

    bool showScore = false;

    public void ShowQuiz(List<QuestionsAndAnswers> quiz, Taxonomia taxonomy, bool showScore = false)
    {
        score = 0;
        QnA = quiz;
        totalQuestions = QnA.Count;
        GenerateQuestion();
        gameObject.SetActive(true);
        quizPanel.SetActive(true);
        resultsPanel.SetActive(false);

        if (taxonomy == Taxonomia.Aplicar || taxonomy == Taxonomia.Analizar || taxonomy == Taxonomia.Evaluar)
        {
            timePanel.gameObject.SetActive(true);
            timeBar.transform.localScale = new Vector3(1f, 1f);
            StartCoroutine(StartCountdown());
        }
        else
        {
            timePanel.gameObject.SetActive(false);
        }

        this.showScore = showScore;
    }

    IEnumerator StartCountdown()
    {
        float timeRemaining = GlobalSettings.i.TimeQuestions;
        animacion = timeBar.transform.DOScaleX(0, timeRemaining);
        animacion.OnComplete(() =>
        {
            //Se terminó el tiempo
            options[QnA[currentQuestion].CorrectAnswer - 1].GetComponent<Image>().color = GlobalSettings.i.AnswerCorrect;
            QnA.RemoveAt(currentQuestion);
            StartCoroutine(WaitForNext());
        });
        yield return animacion.WaitForCompletion();
    }

    public void ExitQuiz()
    {
        QuizManager.i.finishQuiz = true;
    }

    public IEnumerator SeeScore()
    {
        quizPanel.SetActive(false);
        resultsPanel.SetActive(true);
        ScoreText.text = "Score: " + score + "/" + totalQuestions;
        yield return new WaitForSeconds(1f);
        QuizManager.i.finishQuiz = true;
    }

    public void Correct()
    {
        score += 1;
        if(animacion != null) animacion.Kill();
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitForNext());
    }

    public void Wrong()
    {
        if (animacion != null) animacion.Kill();
        options[QnA[currentQuestion].CorrectAnswer - 1].GetComponent<Image>().color = GlobalSettings.i.AnswerCorrect;
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitForNext());
    }

    IEnumerator WaitForNext()
    {
        SetRunningCoroutine(true);
        yield return new WaitForSeconds(2f);
        GenerateQuestion();
        SetRunningCoroutine(false);
    }

    void SetRunningCoroutine(bool running)
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswerScript>().coroutineRunning = running;
        }
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
            if (QuestionImage != null && QnA[currentQuestion].ImageName != null)
                SetImageResources(QnA[currentQuestion].ImageName);
            QuestionTxt.text = QnA[currentQuestion].Question;

            SetAnswer();
        }
        else
        {
            if (!showScore)
                ExitQuiz();
            else
                StartCoroutine(SeeScore());
        }
    }

    void SetImageResources(string nombreImagen)
    {
        Sprite imageSprite = Resources.Load<Sprite>((string)nombreImagen);

        if (imageSprite != null)
        {
            QuestionImage.sprite = imageSprite;
        }
        else
        {
            Debug.LogError("No se pudo cargar la imagen: " + nombreImagen);
        }
    }
}
