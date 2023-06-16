using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerAction : CutsceneAction
{
    public override IEnumerator Play()
    {
        yield return Fader.i.FadeIn(0.5f);

        var playerParty = PlayerController.i.GetComponent<PokemonParty>();
        playerParty.Pokemons.ForEach(p => p.Heal());
        playerParty.PartyUpdated();

        yield return Fader.i.FadeOut(0.5f);

        yield return DialogManager.Instance.ShowDialogText($"¡Tu hatun tienen toda la vida ahora!");
    }
}
