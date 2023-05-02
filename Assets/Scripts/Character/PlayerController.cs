using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    private Vector2 input;

    private Character character;

    public event Action OnEncountered;

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
                StartCoroutine(character.Move(input, CheckForEncounters));
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
            collider.GetComponent<NPCController>()?.Interact(transform);

        }
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.0f, GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.isMoving = false;
                OnEncountered();
            }
        }
    }
}
