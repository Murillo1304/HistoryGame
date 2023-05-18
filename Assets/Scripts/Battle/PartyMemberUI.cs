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
    [SerializeField] Text messageText;

    Pokemon _pokemon;
    Image image;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        SetMessage("");

        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        image = GetComponent<Image>();
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Nv: " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
        hpText.text = $"{_pokemon.HP}/{_pokemon.MaxHp}";
        pokemonSprite.sprite = _pokemon.Base.FrontSprite;
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

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
