using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButtons : MonoBehaviour
{
    public void Start()
    {
        Application.targetFrameRate = 120;
    }

    public void Jugar()
    {
        Loader.Load(Scene.Gameplay);
        Destroy(gameObject);
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
