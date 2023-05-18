using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPatter;
    [SerializeField] float timeBetweenPatter;

    NPCState state;
    float idleTimer = 0f;
    int currentPatter = 0;

    Character character;
    ItemGiver itemGiver;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if(state == NPCState.idle)
        {
            state = NPCState.dialog;
            character.lookTowards(initiator.position);

            if(itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
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
