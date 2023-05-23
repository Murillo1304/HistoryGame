using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButtons : MonoBehaviour
{
    string[] slots;
    
    public void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
            Application.targetFrameRate = 120;
        slots = OptionsUI.i.GetSaveSlotsNames();
    }

    public void Play()
    {     
        Loader.Load(Scene.Gameplay);
        Destroy(gameObject);
    }

    public void LoadGame()
    {
        GlobalSettings.i.SaveSlotName = slots[0];
        Loader.Load(Scene.Gameplay);
        Destroy(gameObject);
    }

    public void Exit()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
