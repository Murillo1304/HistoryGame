using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // Update is called once per frame
    public void HangleUpdate()
    {
        if (! character.isMoving)
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
            Interact();
    }

    void Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interacPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interacPos, 0.3f, GameLayers.i.InteractableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);

        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        foreach(var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                character.Animator.isMoving = false;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
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
