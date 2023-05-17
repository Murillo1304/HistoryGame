using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objetos/Crear una nueva pokeball")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;
    [SerializeField] Sprite inGameSprite;

    public Sprite InGameSprite => inGameSprite;

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CathRateModifier => catchRateModifier;
}
