using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButtons : MonoBehaviour
{
    [SerializeField] GameObject login;
    [SerializeField] GameObject options;
    [SerializeField] GameObject slotList;

    string[] slots;
    
    public void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
            Application.targetFrameRate = 120;
        else
            Application.targetFrameRate = 240;
    }

    public void GetSlots()
    {
        slots = OptionsUI.i.GetSaveSlotsWithUsername(GlobalSettings.i.FirstName, GlobalSettings.i.Lastname);
        GlobalSettings.i.slots = slots;
    }

    public void Access()
    {
        if (GlobalSettings.i.UseInternet)
            StartCoroutine(Login());
        else
        {
            //Sin internet
            GlobalSettings.i.FirstName = "Usuario";
            GlobalSettings.i.Lastname = "Invitado";
            ActivateOptions();
        }
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

            yield return ConfigurationDecoder.i.GetConfiguration(GlobalSettings.i.Username);
            yield return new WaitUntil(() => ConfigurationDecoder.i.requestResponse == true);
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
        //Setear configuracion
        if (GlobalSettings.i.UseInternet)
        {
            ConfigurationDecoder.i.SetConfiguration();
        }
        else
        {
            //Setear configuracion estandar
            GlobalSettings.i.goLake = true;
            GlobalSettings.i.actCave = Actividad.Actividad02;
        }
        AsignNameNewSaveSlot();
        Loader.Load(Scene.Gameplay);
        Destroy(gameObject);
    }

    public void AsignNameNewSaveSlot()
    {
        string newSlotName = "saveSlot-" + GlobalSettings.i.FirstName + GlobalSettings.i.Lastname + "-" + (slots.Count() + 1).ToString() + "-" + GlobalSettings.i.CreateCodeConfiguration();
        GlobalSettings.i.SaveSlotName = newSlotName;
        Debug.Log("Nuevo slot: " + newSlotName);
    }


    public void LoadGame()
    {
        login.SetActive(false);
        options.SetActive(false);
        slotList.SetActive(true);
    }

    public void Exit()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
