using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        var slots = GetSaveSlotsNames();
        if (slots.Count() > 0)
        {
            ButtonLoad.SetActive(true);
        }
        else
        {
            ButtonLoad.SetActive(false);
        }
    }

    public string[] GetSaveSlotsNames()
    {
        string directoryPath = Application.persistentDataPath;
        string[] saveSlotFiles = GetFilesWithSaveSlot(directoryPath);
        return saveSlotFiles;
    }

    private static string[] GetFilesWithSaveSlot(string directoryPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        FileInfo[] files = directoryInfo.GetFiles("*saveSlot*");
        string[] fileNames = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            fileNames[i] = files[i].Name;
        }

        return fileNames;
    }
}
