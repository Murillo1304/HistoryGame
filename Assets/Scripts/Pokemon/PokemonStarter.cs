using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PokemonStarter : MonoBehaviour, Interactable
{
    [SerializeField] Pokemon pokemonToGive;
    [SerializeField] StarterSelect starterSelect;

    public IEnumerator Interact(Transform initiator)
    {
        int selectedChoice = 0;

        StarterManager.i.Show(pokemonToGive);

        yield return DialogManager.Instance.ShowDialogText($"�Te gustar�a elegir a {pokemonToGive.Base.Name}?",
            choices: new List<string>() { "S�", "No" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //S�
            StarterManager.i.Close();
            pokemonToGive.Init();
            var player = initiator.GetComponent<PlayerController>();
            player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);

            AudioManager.i.PlaySfx(AudioId.PokemonObtained, pauseMusic: true);

            string dialogText = $"�{player.Name} recibi� {pokemonToGive.Base.Name}!";

            yield return DialogManager.Instance.ShowDialogText(dialogText);

            starterSelect.StartedSelected();
        }
        else if (selectedChoice == 1)
        {
            //No
            StarterManager.i.Close();
        }
    }
}
