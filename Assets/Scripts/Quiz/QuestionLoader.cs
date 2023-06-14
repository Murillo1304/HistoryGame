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
        string rutaArchivo = Application.streamingAssetsPath + "/" + nombreArchivo;

        // Carga el contenido del archivo JSON como un string
        string json = System.IO.File.ReadAllText(rutaArchivo);

        // Convierte el string JSON en una lista de objetos Pregunta
        jsonQuestions = JsonUtility.FromJson<PreguntasData>(json);

    }
}

[System.Serializable]
public class PreguntaEntender
{
    public string pregunta;
    public string[] opciones;
    public int respuesta;
}

[System.Serializable]
public class PreguntaAplicar
{
    public string pregunta;
    public string imagen;
    public string[] opciones;
    public int respuesta;
}

[System.Serializable]
public class Act01
{
    public List<PreguntaEntender> entender;
    public List<PreguntaAplicar> aplicar;
}

[System.Serializable]
public class PreguntasData
{
    public Act01 act01;
}
