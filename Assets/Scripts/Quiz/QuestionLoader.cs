using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionLoader : MonoBehaviour
{
    string nombreArchivo = "preguntas.json"; // Nombre del archivo JSON que contiene las preguntas

    public PreguntasData jsonQuestions { get; set; }

    public static QuestionLoader i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("preguntas");

        if (jsonAsset != null)
        {
            string jsonContent = jsonAsset.text;
            jsonQuestions = JsonUtility.FromJson<PreguntasData>(jsonContent);

            Debug.Log("Entender: " + jsonQuestions.act01.entender.Count);
            Debug.Log("Aplicar: " + jsonQuestions.act01.aplicar.Count);
            Debug.Log("Analizar: " + jsonQuestions.act01.analizar.Count);
            Debug.Log("Evaluar: " + jsonQuestions.act01.evaluar.Count);
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo JSON");
        }

    }
}

[System.Serializable]
public class Pregunta
{
    public string pregunta;
    public string imagen;
    public string[] opciones;
    public int respuesta;
}

[System.Serializable]
public class Act01
{
    public List<Pregunta> entender;
    public List<Pregunta> aplicar;
    public List<Pregunta> analizar;
    public List<Pregunta> evaluar;
}

[System.Serializable]
public class PreguntasData
{
    public Act01 act01;
}
