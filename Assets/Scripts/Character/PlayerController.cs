using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    public static PlayerController i { get; private set; }
    private Character character;

    public Vector3 positionHealer { get; set; }

    private void Awake()
    {
        i = this;
        character = GetComponent<Character>();
    }

    private void Start()
    {
        positionHealer = transform.position;
    }

    public void HangleUpdate()
    {
        if (!character.isMoving)
        {
            input.x = SimpleInput.GetAxisRaw("Horizontal");
            input.y = SimpleInput.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {
                //Debug.Log("X: " + input.x + "Y: " + input.y);
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if((CrossPlatformInputManager.GetButtonDown("ButtonA")) || (Input.GetKeyDown(KeyCode.Z)))
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interacPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interacPos, 0.1f, GameLayers.i.InteractableLayer | GameLayers.i.WaterLayer);
        if(collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    public void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach(var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;
                
                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
            currentlyInTrigger = null;
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
            positionHealer = new float[] { positionHealer.x, positionHealer.y}
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restaurar posicion
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restaurar party
        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();

        //Restaurar posicion healer
        var posHealer = saveData.positionHealer;
        positionHealer = new Vector3(posHealer[0], posHealer[1]);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
    public float[] positionHealer;
}
