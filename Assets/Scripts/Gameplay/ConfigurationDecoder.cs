using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigurationDecoder : MonoBehaviour
{
    public static ConfigurationDecoder i { get; private set; }

    public JsonConfiguration jsonConfiguration { get; set; }

    private void Awake()
    {
        i = this;
    }

    public bool requestResponse { get; set; } = false;

    string url = "http://localhost:3000";

    public IEnumerator GetConfiguration(string username)
    {
        requestResponse = false;
        if (Application.platform == RuntimePlatform.Android) url = "http://192.168.18.9:3000";
        string urlComplete = url + "/" + username + "/13";

        using (UnityWebRequest request = UnityWebRequest.Get(urlComplete))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                //Debug.Log(jsonResponse);
                string json = "{ \"configurations\":" + jsonResponse + "}";

                jsonConfiguration = JsonUtility.FromJson<JsonConfiguration>(json);
                GlobalSettings.i.Lastname = jsonConfiguration.configurations[0].student.user.last_name;
                GlobalSettings.i.FirstName = jsonConfiguration.configurations[0].student.user.first_name;
                requestResponse = true;
            }
            else
            {
                Debug.LogError("POST request failed. Error: " + request.error);
            }
        }
    }

    public void SetConfiguration()
    {
        if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].is_active)
        {
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[0].is_active)
            {
                //Edad Moderna
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[0].ilo_parameters[1].value, Actividad.Actividad01);
            }
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[1].is_active)
            {
                //Imperio del Tahuantinsuyo
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[1].ilo_parameters[1].value, Actividad.Actividad02);
            }
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[2].is_active)
            {
                //Conquista y Virreynato
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[0].ilos_config[2].ilo_parameters[1].value, Actividad.Actividad03);
            }
        }

        if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].is_active)
        {
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[0].is_active)
            {
                //Regiones Naturales del Perú
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[0].ilo_parameters[1].value, Actividad.Actividad04);
            }
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[1].is_active)
            {
                //Herramientas cartográficas
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[1].ilo_parameters[1].value, Actividad.Actividad05);
            }
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[2].is_active)
            {
                //Prevencion de desastres
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[1].ilos_config[2].ilo_parameters[1].value, Actividad.Actividad06);
            }
        }

        if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[2].is_active)
        {
            if (jsonConfiguration.configurations[0].dgbl_config.ilos_config[2].ilos_config[0].is_active)
            {
                //Agentes economicos
                EvaluateEscenary(jsonConfiguration.configurations[0].dgbl_config.ilos_config[2].ilos_config[0].ilo_parameters[1].value, Actividad.Actividad07);
            }
        }
    }

    public void EvaluateEscenary(string escenary, Actividad activity)
    {
        Debug.Log("Asignando: " + escenary + " Actividad: " + activity.ToString());

        if (escenary == "Cuevas")
        {
            GlobalSettings.i.goCave = true;
            GlobalSettings.i.actCave = activity;
        }
        else if (escenary == "Lago")
        {
            GlobalSettings.i.goLake = true;
            GlobalSettings.i.actLake = activity;
        }
        else if (escenary == "Bosque")
        {
            GlobalSettings.i.goForest = true;
            GlobalSettings.i.actForest = activity;
        }
        else if (escenary == "Montaña")
        {
            GlobalSettings.i.goMountain = true;
            GlobalSettings.i.actMountain = activity;
        }
        else if (escenary == "Ciudad")
        {
            GlobalSettings.i.goCity = true;
            GlobalSettings.i.actCity = activity;
        }
    }
}

[System.Serializable]
public class Ilos_parameters_config
{
    public string label;
    public string value;
    public string type;
    public int game_ilo_config;
}

[System.Serializable]
public class Ilos_config
{
    public int id;
    public string label;
    public bool is_active;
    public List<Ilos_parameters_config> ilo_parameters;
    public List<string> activities_config;
    public List<Ilos_config> ilos_config;
}

[System.Serializable]
public class Dgbl_config
{
    public int id;
    public string label;
    public List<Ilos_config> ilos_config;
}

[System.Serializable]
public class Ilo_parameters
{
    public int id;
    public string label;
    public string description;
    public string parameter_type;
    public string default_value;
    public string min_value;
    public string max_value;
    public string icon;
    public List<string> elements;
    public string icons;
}

[System.Serializable]
public class Ilos
{
    public int id;
    public List<string> topics;
    public string label;
    public string description;
    public string icon;
    public List<string> activities;
    public bool selectable;
    public bool selected;
    public List<Ilos> ilos;
    public List<Ilo_parameters> ilo_parameters;
    public List<string> ilo_aids;
}

[System.Serializable]
public class Learning_areas
{
    public int id;
    public string label;
}

[System.Serializable]
public class Dgbl_features
{
    public int id;
    public string education_level;
    public List<Learning_areas> learning_areas;
    public List<Ilos> ilos;
    public List<string> student_profiles;
}

[System.Serializable]
public class Images
{
    public int id;
    public string label;
    public string description;
    public string url;
    public string image;
}

[System.Serializable]
public class Url
{
    public int id;
    public string label;
    public string description;
    public string url;
}

[System.Serializable]
public class Game_Description
{
    public int id;
    public string label;
    public string short_description;
    public string long_description;
    public string registered_by;
    public string properties;
    public List<Url> urls;
    public List<Images> images;
    public List<string> videos;
}

[System.Serializable]
public class Game
{
    public int id;
    public Game_Description game_description;
    public Dgbl_features dgbl_features;
    public string game_features;
}

[System.Serializable]
public class User
{
    public int id;
    public string email;
    public string first_name;
    public string last_name;
    public string username;
}

[System.Serializable]
public class Student
{
    public User user;
    public string institution;
    public int level;
    public int experience;
    public int experience_to_level_up;
    public string inventory;
}

[System.Serializable]
public class Configuration
{
    public int id;
    public string label;
    public Student student;
    public Game game;
    public Dgbl_config dgbl_config;
    public string game_features_config;
}

[System.Serializable]
public class JsonConfiguration
{
    public List<Configuration> configurations;
}