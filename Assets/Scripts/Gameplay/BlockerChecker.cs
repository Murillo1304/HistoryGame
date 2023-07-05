using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockerChecker : MonoBehaviour
{
    [SerializeField] GameObject blocker1;
    [SerializeField] GameObject blocker2;

    private void Start()
    {
        CheckPass();
    }

    private void CheckPass()
    {
        if (GlobalSettings.i.goCave || GlobalSettings.i.goLake || GlobalSettings.i.goForest)
            blocker2.SetActive(false);
        if (GlobalSettings.i.goMountain || GlobalSettings.i.goCity)
            blocker1.SetActive(false);
    }
}
