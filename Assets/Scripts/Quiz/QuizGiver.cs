using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuizGiver : MonoBehaviour, ISavable
{
    [SerializeField] List<QuestionsAndAnswers> QnA;
    [SerializeField] Dialog dialog;
    [SerializeField] Taxonomia taxonomy;

    bool used = false;

    public IEnumerator GiveQuiz()
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        used = true;
        QuizManager.i.Show(QnA);
        yield return new WaitUntil(() => QuizManager.i.finishQuiz == true);
        QuizManager.i.Close();
    }

    public bool CanBeQuiz()
    {
        return QnA != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
