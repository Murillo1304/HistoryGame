using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text hpText;
    [SerializeField] Image pokemonSprite;
    [SerializeField] Sprite memberNoSelect;
    [SerializeField] Sprite memberSelect;

    Pokemon _pokemon;
    Image image;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        image = GetComponent<Image>();
        nameText.text = pokemon.Base.Name;
        levelText.text = "Nv: " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        hpText.text = $"{pokemon.HP}/{pokemon.MaxHp}";
        pokemonSprite.sprite = pokemon.Base.FrontSprite;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            //nameText.color = GlobalSettings.i.HighlightedColor;
            //levelText.color = GlobalSettings.i.HighlightedColor;
            image.sprite = memberSelect;

        }
        else
        {
            //nameText.color = Color.white;
            //levelText.color = Color.white;
            image.sprite = memberNoSelect;
        }
    }
}
