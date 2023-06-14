using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPassController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] KeyItem pass;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] GameObject blocker;
    Character character;

    //State
    bool passCheck = false;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        yield return CheckPass();

    }

    public IEnumerator TriggerPassChecker(PlayerController player)
    {
        character.LookTowards(player.transform.position);

        //Mostrar exclamacion
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        yield return CheckPass();
    }

    private IEnumerator CheckPass()
    {
        if (!passCheck)
        {
            var inventory = PlayerController.i.GetComponent<Inventory>();
            if (inventory.HasItem(pass))
            {
                yield return DialogManager.Instance.ShowDialogText($"¡Tienes el {pass.Name}, puedes pasar!");
                passCheck = true;
                fov.gameObject.SetActive(false);
                blocker.gameObject.SetActive(false);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText($"¡Necesitas el {pass.Name} para poder pasar!");
            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"¡Tienes el {pass.Name}, puedes pasar!");
        }
        GameController.Instance.StartFreeRoamState();
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
        return passCheck;
    }

    public void RestoreState(object state)
    {
        passCheck = (bool)state;

        if (passCheck)
            fov.gameObject.SetActive(false);
    }
}
