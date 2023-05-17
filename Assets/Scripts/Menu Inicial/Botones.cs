using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Botones : MonoBehaviour
{

    public void Start()
    {
        Application.targetFrameRate = 120;
    }

    public void Jugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Destroy(gameObject);
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
