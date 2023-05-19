using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }
    private void Awake()
    {
        i = this;
    }
    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"¡{pokemon.Base.Name} está evolucionando!");
        
        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);

        //Efecto evolucion
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(pokemonImage.DOColor(Color.black, 0.7f));
        for (int i = 0;i < 6; i++)
        {
            fadeSequence.AppendCallback(() => pokemonImage.sprite = pokemon.Base.FrontSprite);
            fadeSequence.AppendInterval(0.2f);
            fadeSequence.AppendCallback(() => pokemonImage.sprite = oldPokemon.FrontSprite);
            fadeSequence.AppendInterval(0.2f);
        }

        fadeSequence.AppendCallback(() => pokemonImage.sprite = pokemon.Base.FrontSprite);
        fadeSequence.Append(pokemonImage.DOColor(Color.white, 0.7f));

        yield return fadeSequence.WaitForCompletion();

        //pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"¡{oldPokemon.Name} evolucionó en {pokemon.Base.Name}!");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
