using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countTxt;
    [SerializeField] Text priceTxt;

    bool selected;
    int currentCount;

    bool presionadoVertical = false;

    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;
        currentCount = 1;

        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected == true);

        onCountSelected?.Invoke(currentCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = currentCount;

        if ((Input.GetKeyDown(KeyCode.UpArrow) || (SimpleInput.GetAxisRaw("Vertical") > 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            ++currentCount;
        }
        else if ((Input.GetKeyDown(KeyCode.DownArrow) || (SimpleInput.GetAxisRaw("Vertical") < 0)) && !presionadoVertical)
        {
            presionadoVertical = true;
            --currentCount;
        }

        if (SimpleInput.GetAxisRaw("Vertical") == 0)
        {
            presionadoVertical = false;
        }

        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != prevCount)
            SetValues();

        if (Input.GetKeyDown(KeyCode.Z) || CrossPlatformInputManager.GetButtonDown("ButtonA"))
            selected = true;
    }

    void SetValues()
    {
        countTxt.text = "x " + currentCount;
        priceTxt.text = "S/." + pricePerUnit * currentCount;
    }
}
