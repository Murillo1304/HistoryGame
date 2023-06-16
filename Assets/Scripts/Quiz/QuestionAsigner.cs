using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Actividad { Actividad01, Actividad02, Actividad03, Actividad04, Actividad05, Actividad06, Actividad07 }
public enum Taxonomia { Entender, Aplicar, Analizar, Evaluar }

public class QuestionAsigner : MonoBehaviour, ISavable
{
    [SerializeField] Actividad activity;
    [SerializeField] Taxonomia taxonomy;
    QuestionerController[] questioners;

    List<QuestionsAndAnswers> questions;

    public bool Asigned { get; set; } = false;

    private void Start()
    {
        if (!Asigned)
        {
            AsignQuestions();
        }
    }

    public void AsignQuestions()
    {
        Debug.Log("Asignando preguntas random");
        Asigned = true;
        questioners = GetComponentsInChildren<QuestionerController>();

        if (activity == Actividad.Actividad01)
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.evaluar);

        foreach (var questioner in questioners)
        {
            questioner.taxonomy = taxonomy;
            questioner.QuestionsAndAnswersList = GetRandomQuestion();
        }
    }

    private List<QuestionsAndAnswers> GetRandomQuestion()
    {
        int randomIndex = UnityEngine.Random.Range(0, questions.Count);
        QuestionsAndAnswers randomQuestion = questions[randomIndex];

        questions.RemoveAt(randomIndex);

        List<QuestionsAndAnswers> newList = new List<QuestionsAndAnswers>();
        newList.Add(randomQuestion);

        return newList;
    }

    private void PrintQuestions(List<QuestionsAndAnswers> listQuestions)
    {
        foreach (var pregunta in listQuestions)
        {
            Debug.Log("Pregunta: " + pregunta.Question);

            foreach (var respuesta in pregunta.Answers)
            {
                Debug.Log("Alt: " + respuesta);
            }

            Debug.Log("Rspt. Correcta: " + pregunta.CorrectAnswer);
        }
    }

    public List<QuestionsAndAnswers> CopyQuestions(List<Pregunta> original)
    {
        List<QuestionsAndAnswers> copiaPreguntas = new List<QuestionsAndAnswers>();

        foreach (Pregunta preguntaOriginal in original)
        {
            QuestionsAndAnswers preguntaCopia = new QuestionsAndAnswers();
            preguntaCopia.Question = preguntaOriginal.pregunta;

            if(preguntaOriginal.imagen != "")
                preguntaCopia.ImageName = preguntaOriginal.imagen;

            // Combina las opciones al azar
            preguntaCopia.Answers = preguntaOriginal.opciones.OrderBy(opcion => UnityEngine.Random.value).ToArray();

            // Cambia el �ndice de respuesta a la respuesta correcta
            int indiceRespuestaCorrecta = Array.IndexOf(preguntaCopia.Answers, preguntaOriginal.opciones[preguntaOriginal.respuesta - 1]);
            preguntaCopia.CorrectAnswer = indiceRespuestaCorrecta + 1;

            copiaPreguntas.Add(preguntaCopia);
        }

        return copiaPreguntas;
    }

    public object CaptureState()
    {
        return Asigned;
    }

    public void RestoreState(object state)
    {
        Asigned = (bool)state;
    }
}
