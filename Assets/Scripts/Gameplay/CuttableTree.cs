using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("Este arbol se ve como si se pudiera cortar");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Corte"));

        if (pokemonWithCut != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"¿{pokemonWithCut.Base.Name} usará corte?",
                choices: new List<string>() { "Sí", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                //Sí
                yield return DialogManager.Instance.ShowDialogText($"¡{pokemonWithCut.Base.Name} usa corte!");
                gameObject.SetActive(false);
            }
        }
    }
}
