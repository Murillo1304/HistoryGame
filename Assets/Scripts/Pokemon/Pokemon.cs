using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base
    {
        get { return _base; }
    }
    public int Level
    {
        get { return level; }
    }
    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        //Generar movimientos
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if(Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges= new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Ataque, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defensa, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAtaque, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefensa, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Velocidad, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + Level + 10;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Ataque,0},
            {Stat.Defensa,0},
            {Stat.SpAtaque,0},
            {Stat.SpDefensa,0},
            {Stat.Velocidad,0},
            {Stat.Precision,0},
            {Stat.Evasion,0},
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //Boosteo
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if(boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts, bool isPlayerUnit)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            string enemigo = "";
            if (!isPlayerUnit) enemigo = " enemigo";

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if(boost > 0)
                StatusChanges.Enqueue($"�{stat} de {Base.Name}{enemigo} subi�!");
            else
                StatusChanges.Enqueue($"�{stat} de {Base.Name}{enemigo} baj�!");

            Debug.Log($"{stat} se ha boosteado {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if(Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack
    {
        get { return GetStat(Stat.Ataque); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defensa); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAtaque); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefensa); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Velocidad); }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Especial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Especial) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        Debug.Log("da�o: " + damage);

        DecreaseHP(damage);
        
        return damageDetails;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
        HpChanged = true;
    }

    public void IncreaseHP(int amouth)
    {
        HP = Mathf.Clamp(HP + amouth, 0, MaxHp);
        OnHPChanged?.Invoke();
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId, bool isPlayerUnit)
    {
        if (Status != null) return;
        
        string enemy = "";
        if (!isPlayerUnit) enemy = " enemigo";

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"�{Base.name}{enemy} {Status.StartMessage}!");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId, bool isPlayerUnit)
    {
        if (VolatileStatus != null) return;

        string enemigo = "";
        if (!isPlayerUnit) enemigo = " enemigo";

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"�{Base.name}{enemigo} {VolatileStatus.StartMessage}!");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if(Status?.OnBeforeMove != null)
        {
            if(!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}
