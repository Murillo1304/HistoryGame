using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class SlotListUI : MonoBehaviour
{
    [SerializeField] GameObject slotList;
    [SerializeField] SlotUI slotUI;

    List<SlotUI> slotUIList;
    int number = 0;

    private void Start()
    {
        UpdateItemList();
    }

    void UpdateItemList()
    {
        //Limpiar todos los slots
        foreach (Transform child in slotList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<SlotUI>();

        number = 0;
        foreach (string slot in GlobalSettings.i.slots)
        {
            var slotUIObj = Instantiate(slotUI, slotList.transform);

            number++;
            string numberText = "Partida " + number.ToString();
            string name = "Nombre: " + GlobalSettings.i.FirstName + " " + GlobalSettings.i.Lastname;
            string configuration = "";

            char delimiter = '-';
            string[] parts = slot.Split(delimiter);
            string lastPart = parts[parts.Length - 1];

            if (lastPart[0] == '1') configuration += generateLine(0, int.Parse(lastPart[1].ToString()));
            if (lastPart[2] == '1') configuration += generateLine(1, int.Parse(lastPart[3].ToString()));
            if (lastPart[4] == '1') configuration += generateLine(2, int.Parse(lastPart[5].ToString()));
            if (lastPart[6] == '1') configuration += generateLine(3, int.Parse(lastPart[7].ToString()));
            if (lastPart[8] == '1') configuration += generateLine(4, int.Parse(lastPart[9].ToString()));

            slotUIObj.SetData(numberText, name, configuration, slot);

            slotUIList.Add(slotUIObj);
        }
    }

    string generateLine(int escenary, int activity)
    {
        string line = "Escenario: ";
        if (escenary == 0) line += "Cuevas";
        else if (escenary == 1) line += "Lago";
        else if (escenary == 2) line += "Bosque";
        else if (escenary == 3) line += "Montaña";
        else if (escenary == 4) line += "Ciudad";

        line += " - Actividad: ";
        if (activity == 0) line += "Edad Moderna";
        else if (escenary == 1) line += "Imperio del Tahuantisuyo";
        else if (escenary == 2) line += "Conquista y Virreinato";
        else if (escenary == 3) line += "Regiones Naturales";
        else if (escenary == 4) line += "Herramientas cartográficas";
        else if (escenary == 5) line += "Prevención de desastres";
        else if (escenary == 6) line += "Agentes económicos";

        line += '\n';

        return line;
    }
}
