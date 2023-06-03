using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movimiento")]
    [SerializeField] List<Vector2> movementPatter;
    [SerializeField] float timeBetweenPatter;

    NPCState state;
    float idleTimer = 0f;
    int currentPatter = 0;
    Quest activeQuest;

    Character character;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;
    QuizGiver quizGiver;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
        quizGiver = GetComponent<QuizGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if(state == NPCState.idle)
        {
            state = NPCState.dialog;
            character.LookTowards(initiator.position);
            var party = initiator.GetComponent<PokemonParty>();

            if (questToComplete!= null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
                questToComplete = null;

                Debug.Log($"{quest.Base.Name} completado");
            }

            if(itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (pokemonGiver != null && pokemonGiver.CanBeGiven())
            {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            }
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }
            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
                }
            }
            else if (healer != null && party.Pokemons.Count > 0)
            {
                healer.SetPositionHealer(initiator.GetComponent<PlayerController>());
                yield return healer.Heal(initiator, dialog);
            }
            else if (merchant != null)
            {
                yield return merchant.Trade();
            }
            else if (quizGiver != null && quizGiver.CanBeQuiz())
            {
                yield return quizGiver.GiveQuiz();
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }

            idleTimer = 0f;
            state = NPCState.idle;
        }
    }

    private void Update()
    {
        if(state == NPCState.idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPatter)
            {
                idleTimer = 0f;
                if(movementPatter.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.walking;

        var oldPosition = transform.position;

        yield return character.Move(movementPatter[currentPatter]);

        if(transform.position != oldPosition)
            currentPatter = (currentPatter + 1) % movementPatter.Count;

        state = NPCState.idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();

        if (questToComplete != null)
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;

            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { idle, walking, dialog}
