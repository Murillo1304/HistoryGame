using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] GameObject ButtonLoad;

    public static OptionsUI i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {   
        var slots = GetSaveSlotsWithUsername(GlobalSettings.i.FirstName, GlobalSettings.i.Lastname);
        if (slots.Count() > 0)
        {
            ButtonLoad.SetActive(true);
        }
        else
        {
            ButtonLoad.SetActive(false);
        }
    }

    public string[] GetSaveSlotsWithUsername(string lastname, string firstname)
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        string pattern = @"saveSlot-" + firstname + lastname + @".*-\d+-\d+";
        List<string> miLista = new List<string>();

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (Regex.IsMatch(fileName, pattern))
            {
                miLista.Add(fileName);
            }
        }

        return miLista.ToArray();
    }
}
