using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] GameObject TextIncorrect;

    public static LoginUI i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        passwordField.contentType = TMP_InputField.ContentType.Password;
        passwordField.inputType= TMP_InputField.InputType.Password;
    }

    string url = "http://localhost:3000";
    public string username { get; set; }
    string password;
    bool successLogin = false;
    public bool requestResponse { get; set; } = false;

    public void GetTextFields()
    {
        username = usernameField.text;
        password = passwordField.text;
    }

    public bool getLoginStatus()
    {
        return successLogin;
    }

    public IEnumerator Login()
    {
        requestResponse = false;
        GetTextFields();
        
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        if (Application.platform == RuntimePlatform.Android) url = "http://192.168.18.9:3000";

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                TokenResponse response = JsonUtility.FromJson<TokenResponse>(jsonResponse);

                if (response.token != "")
                {
                    Debug.Log("Correcto");
                    successLogin = true;
                    requestResponse = true;
                }
                else
                {
                    Debug.Log("Incorrecto");
                    TextIncorrect.SetActive(true);
                    successLogin = false;
                    requestResponse = true;
                }
            }
            else
            {
                Debug.LogError("POST request failed. Error: " + request.error);
            }
        }
    }
}

[System.Serializable]
public class TokenResponse
{
    public string token;
}
