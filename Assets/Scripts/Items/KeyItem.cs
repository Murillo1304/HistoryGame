using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objetos/Crear un nuevo objeto clave")]
public class KeyItem : ItemBase
{
    public override bool CanUseInBattle => false;
    public override bool CanUseOutsideBattle => false;
}
