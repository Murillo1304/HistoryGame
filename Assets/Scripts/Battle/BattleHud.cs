using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text hpText;
    [SerializeField] GameObject expBar;

    [SerializeField] Color envColor;
    [SerializeField] Color queColor;
    [SerializeField] Color dorColor;
    [SerializeField] Color parColor;
    [SerializeField] Color conColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        if (hpText != null) hpText.text = $"{pokemon.HP}/{pokemon.MaxHp}";
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.env, envColor },
            {ConditionID.que, queColor },
            {ConditionID.dor, dorColor },
            {ConditionID.par, parColor },
            {ConditionID.con, conColor },
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if(_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString();
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Nv: " + _pokemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            //Texto animado
            if (hpText != null)
            {
                int oldHp = int.Parse(hpText.text.Split('/')[0]);
                var textAnimation = DOTween.To(() => oldHp, x => hpText.text = $"{Mathf.RoundToInt(x)}/{_pokemon.MaxHp}", _pokemon.HP, 1f);
            }

            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
