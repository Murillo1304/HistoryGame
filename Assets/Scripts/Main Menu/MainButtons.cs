using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButtons : MonoBehaviour
{
    [SerializeField] GameObject login;
    [SerializeField] GameObject options;

    string[] slots;
    
    public void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
            Application.targetFrameRate = 120;
    }

    public void GetSlots()
    {
        slots = OptionsUI.i.GetSaveSlotsNames();
        Debug.Log(slots[0]);
    }

    public void Access()
    {
        if (GlobalSettings.i.UseInternet)
            StartCoroutine(Login());
        else
            ActivateOptions();
    }

    public IEnumerator Login()
    {
        yield return LoginUI.i.Login();
        yield return new WaitUntil(() => LoginUI.i.requestResponse == true);

        bool canLog = LoginUI.i.getLoginStatus();
        if (canLog)
        {
            GlobalSettings.i.Username = LoginUI.i.username;
            ActivateOptions();
        }
    }

    public void ActivateOptions()
    {
        login.SetActive(false);
        options.SetActive(true);
        GetSlots();
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
