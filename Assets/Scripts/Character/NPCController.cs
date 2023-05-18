using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
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

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if(state == NPCState.idle)
        {
            state = NPCState.dialog;
            character.lookTowards(initiator.position);

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
}

public enum NPCState { idle, walking, dialog}
