using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterQuiz;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] List<QuestionsAndAnswers> QnA;
    Character character;

    [SerializeField] AudioClip quizAppearsClip;

    //State
    bool used = false;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!used)
        {
            AudioManager.i.PlayMusic(quizAppearsClip);
            yield return GiveQuiz();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterQuiz);
        }
    }

    public IEnumerator GiveQuiz()
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        used = true;
        fov.gameObject.SetActive(false);
        QuizManager.i.Show(QnA);
        yield return new WaitUntil(() => QuizManager.i.finishQuiz == true);
        QuizManager.i.Close();
        AudioManager.i.PlayMusic(GameController.Instance.CurrentScene.SceneMusic, fade: true);
    }

    public bool CanBeQuiz()
    {
        return QnA != null && !used;
    }

    public IEnumerator TriggerQuiz(PlayerController player)
    {
        AudioManager.i.PlayMusic(quizAppearsClip);

        //Mostrar exclamacion
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //Caminar hacia el jugador
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Preguntas
        yield return GiveQuiz();
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;

        if (used)
            fov.gameObject.SetActive(false);
    }
}

