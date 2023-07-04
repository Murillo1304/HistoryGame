using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettingsSaver : MonoBehaviour, ISavable
{
    public static GlobalSettingsSaver i { get;  set; }

    private void Awake()
    {
        i = this;
    }

    public object CaptureState()
    {
        Debug.Log("Guardando Global Settings");

        var saveData = new GlobalSettingsSaveData()
        {
            goCaveSave = GlobalSettings.i.goCave,
            goLakeSave = GlobalSettings.i.goLake,
            goForestSave = GlobalSettings.i.goForest,
            goMountainSave = GlobalSettings.i.goMountain,
            goCitySave = GlobalSettings.i.goCity,
            actCaveSave = GlobalSettings.i.actCave,
            actLakeSave = GlobalSettings.i.actLake,
            actForestSave = GlobalSettings.i.actForest,
            actMountainSave = GlobalSettings.i.actMountain,
            actCitySave = GlobalSettings.i.actCity
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        Debug.Log("Restaurando Global Settings");

        var saveData = state as GlobalSettingsSaveData;
        GlobalSettings.i.goCave = saveData.goCaveSave;
        GlobalSettings.i.goLake = saveData.goLakeSave;
        GlobalSettings.i.goForest = saveData.goForestSave;
        GlobalSettings.i.goMountain = saveData.goMountainSave;
        GlobalSettings.i.goCity = saveData.goCitySave;
        GlobalSettings.i.actCave = saveData.actCaveSave;
        GlobalSettings.i.actLake = saveData.actLakeSave;
        GlobalSettings.i.actForest = saveData.actForestSave;
        GlobalSettings.i.actMountain = saveData.actMountainSave;
        GlobalSettings.i.actCity = saveData.actCitySave;
    }
}

[Serializable]
public class GlobalSettingsSaveData
{
    public bool goCaveSave;
    public bool goLakeSave;
    public bool goForestSave;
    public bool goMountainSave;
    public bool goCitySave;
    public Actividad actCaveSave;
    public Actividad actLakeSave;
    public Actividad actForestSave;
    public Actividad actMountainSave;
    public Actividad actCitySave;
}