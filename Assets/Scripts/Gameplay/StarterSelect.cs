using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterSelect : MonoBehaviour
{
    [SerializeField] GameObject StartedSelectCutscene;

    public void StartedSelected()
    {
        StartedSelectCutscene.SetActive(true);
        PlayerController.i.OnMoveOver();
    }
}
