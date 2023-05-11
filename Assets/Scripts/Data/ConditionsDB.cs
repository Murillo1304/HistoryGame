using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.env,
            new Condition()
            {
                Name = "Envenenado",
                StartMessage = "ha sido envenenado",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"¡El veneno resta PS a {pokemon.Base.Name}!");
                }
            }
        },
        {
            ConditionID.que,
            new Condition()
            {
                Name = "Quemado",
                StartMessage = "ha sido quemado",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"¡La quemadura resta PS a {pokemon.Base.Name}!");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralizado",
                StartMessage = "ha sido paralizado",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} está paralizado! ¡No se puede mover!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.con,
            new Condition()
            {
                Name = "Congelado",
                StartMessage = "ha sido congelado",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} ya no está congelado!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.dor,
            new Condition()
            {
                Name = "Dormido",
                StartMessage = "ha caido dormido",
                OnStart = (Pokemon pokemon) =>
                {
                    //Dormir por 1-3 turnos
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Dormirá por {pokemon.StatusTime} turnos");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} despertó!");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} está dormido!");
                    return false;
                }
            }
        },
        //Estados volatiles
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "ha caido en confusión",
                OnStart = (Pokemon pokemon) =>
                {
                    // COnfundido por 1-4 turnos
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Estará confundido por {pokemon.VolatileStatusTime} turnos");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} ha salido de la confusión!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;

                    // 50% probabilidades de hacer un movimiento
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Herido por la confusion
                    pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} está confundido!");
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"¡Está tan confuso que se hirió a si mismo!");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.dor || condition.Id == ConditionID.con)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.env || condition.Id == ConditionID.que)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, env, que, dor, par, con,
    confusion
}
