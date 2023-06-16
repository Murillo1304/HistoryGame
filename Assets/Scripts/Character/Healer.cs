using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] string Dialog = "¡Luces cansado! ¿Te gustaría tomarte un descanso?";

    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        
        yield return DialogManager.Instance.ShowDialogText(Dialog, 
            choices: new List<string>() { "Sí", "No"},
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Sí
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText($"¡Tus hatun tienen toda la vida ahora!");
        }
        else if (selectedChoice == 1)
        {
            //No
            yield return DialogManager.Instance.ShowDialogText($"¡Esta bien! ¡Vuelve si cambias de opinion!");
        }
    }

    public void SetPositionHealer(PlayerController player)
    {
        player.positionHealer = player.transform.position;
        Debug.Log("Posicion guardada x: " + player.positionHealer.x + " posicion guardada y:" + player.positionHealer.y);
    }
}
