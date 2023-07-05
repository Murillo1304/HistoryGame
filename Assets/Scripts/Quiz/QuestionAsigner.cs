using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Actividad { Actividad01, Actividad02, Actividad03, Actividad04, Actividad05, Actividad06, Actividad07 }
public enum Taxonomia { Entender, Aplicar, Analizar, Evaluar }
public enum Escenario { Cuevas, Lago, Bosque, Montaña, Ciudad }

public class QuestionAsigner : MonoBehaviour, ISavable
{
    Actividad activity;
    [SerializeField] Taxonomia taxonomy;
    [SerializeField] Escenario scenary;
    [SerializeField] int numberQuestions = 1;
    [SerializeField] bool showScore = false;
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

        if (scenary == Escenario.Cuevas)
            activity = GlobalSettings.i.actCave;
        else if (scenary == Escenario.Lago)
            activity = GlobalSettings.i.actLake;
        else if (scenary == Escenario.Bosque)
            activity = GlobalSettings.i.actForest;
        else if (scenary == Escenario.Montaña)
            activity = GlobalSettings.i.actMountain;
        else if (scenary == Escenario.Ciudad)
            activity = GlobalSettings.i.actCity;

        if (activity == Actividad.Actividad01)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act01.evaluar);
        }
        else if (activity == Actividad.Actividad02)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act02.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act02.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act02.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act02.evaluar);
        }
        else if (activity == Actividad.Actividad03)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act03.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act03.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act03.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act03.evaluar);
        }
        else if (activity == Actividad.Actividad04)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act04.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act04.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act04.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act04.evaluar);
        }
        else if (activity == Actividad.Actividad05)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act05.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act05.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act05.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act05.evaluar);
        }
        else if (activity == Actividad.Actividad06)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act06.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act06.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act06.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act06.evaluar);
        }
        else if (activity == Actividad.Actividad07)
        {
            if (taxonomy == Taxonomia.Entender)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act07.entender);
            else if (taxonomy == Taxonomia.Aplicar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act07.aplicar);
            else if (taxonomy == Taxonomia.Analizar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act07.analizar);
            else if (taxonomy == Taxonomia.Evaluar)
                questions = CopyQuestions(QuestionLoader.i.jsonQuestions.act07.evaluar);
        }

        bool lower = questioners.Length * numberQuestions <= questions.Count;

        foreach (var questioner in questioners)
        {
            questioner.taxonomy = taxonomy;
            questioner.showScore = showScore;
            questioner.QuestionsAndAnswersList = GetRandomQuestion(lower);
        }
    }

    private List<QuestionsAndAnswers> GetRandomQuestion(bool remove)
    {
        List<QuestionsAndAnswers> newList = new List<QuestionsAndAnswers>();

        for (int i = 0; i < numberQuestions; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, questions.Count);
            QuestionsAndAnswers randomQuestion = questions[randomIndex];

            if (remove)
                questions.RemoveAt(randomIndex);

            newList.Add(randomQuestion);
        }

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

            // Cambia el índice de respuesta a la respuesta correcta
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
