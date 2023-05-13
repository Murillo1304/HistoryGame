using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP (float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
        ChangeColor(hpNormalized);
    }

    public IEnumerator SetHPSmooth(float newHp)
    {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHp;

        while(curHP - newHp > Mathf.Epsilon)
        {
            curHP -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHP, 1f);
            ChangeColor(curHP);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);

    }

    void ChangeColor(float hpNormalized)
    {
        if (hpNormalized <= 1 && hpNormalized > 0.50)
        {
            health.GetComponent<Image>().color = new Color32(93, 200, 103, 255);
        }
        else if (hpNormalized <= 0.50 && hpNormalized > 0.20)
        {
            health.GetComponent<Image>().color = new Color32(237, 200, 50, 255);
        }
        else
        {
            health.GetComponent<Image>().color = new Color32(230, 19, 52, 255);
        }
    }
}
