using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteQuestAction : CutsceneAction
{
    [SerializeField] QuestBase questToComplete;

    public override IEnumerator Play()
    {
        var quest = new Quest(questToComplete);
        yield return quest.CompleteQuest();
        questToComplete = null;

        Debug.Log($"{quest.Base.Name} completado");
    }
}
