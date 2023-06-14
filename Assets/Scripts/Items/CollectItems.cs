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
    int Collect = 0;

    private void Awake()
    {
        pickups = GetComponentsInChildren<Pickup>();
        count = pickups.Length;
    }

    public void checkCompleteItems()
    {
        Collect += 1;
        if (Collect == count)
        {
            blocker.SetActive(false);
            StartCoroutine(DialogManager.Instance.ShowDialogText($"¡El paso se ha desbloqueado!"));
        }
    }

    public object CaptureState()
    {
        return Collect;
    }

    public void RestoreState(object state)
    {
        Collect = (int)state;
        if (Collect == count)
        {
            blocker.SetActive(false);
        }
    }
}
