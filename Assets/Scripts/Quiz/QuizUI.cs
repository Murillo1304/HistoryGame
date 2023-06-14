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
    [SerializeField] Image QuestionImage;
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
        SetRunningCoroutine(true);
        yield return new WaitForSeconds(1);
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
                SetImage(QnA[currentQuestion].ImageName);
            QuestionTxt.text = QnA[currentQuestion].Question;

            SetAnswer();
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }

    void SetImage(string nombreImagen)
    {
        Debug.Log("Asignando Imagen");

        string absolutePath = System.IO.Path.Combine(Application.streamingAssetsPath, nombreImagen);

        Texture2D texture = LoadTextureFromStreamingAssets(absolutePath);

        if (texture != null)
        {
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            QuestionImage.sprite = newSprite;

            QuestionImage.preserveAspect = true;
        }
    }

    private Texture2D LoadTextureFromStreamingAssets(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        return texture;
    }
}
