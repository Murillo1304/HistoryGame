using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class CollectItems : MonoBehaviour, ISavable
{
    [SerializeField] GameObject blocker;
    Pickup[] pickups;
    int count;
    int collect = 0;
    bool Complete = false;

    private void Awake()
    {
        pickups = GetComponentsInChildren<Pickup>();
        count = pickups.Length;
    }

    public void checkCompleteItems()
    {
        collect += 1;
        if (collect == count)
        {
            blocker.SetActive(false);
            StartCoroutine(DialogManager.Instance.ShowDialogText($"¡El paso se ha desbloqueado!"));
        }
    }

    public object CaptureState()
    {
        return Complete;
    }

    public void RestoreState(object state)
    {
        Complete = (bool)state;
        if (Complete)
        {
            blocker.SetActive(false);
        }
    }
}
