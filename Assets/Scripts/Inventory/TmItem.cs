using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objetos/Crear un nuevo TM o HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;

    public override bool Use(Pokemon pokemon)
    {
        //El movimiento aprendido esta siendo captado desde el inventoryUI, si lo ha aprendido retorna true
        return pokemon.HasMove(move);
    }

    public MoveBase Move => move;
}
