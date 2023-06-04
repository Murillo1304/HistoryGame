using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarterManager : MonoBehaviour
{
    [SerializeField] GameObject starterUI;
    [SerializeField] Image pokemonImage;

    public static StarterManager i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    public void Show (Pokemon pokemon)
    {
        starterUI.SetActive(true);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
    }

    public void Close()
    {
        starterUI.SetActive(false);
    }
}
