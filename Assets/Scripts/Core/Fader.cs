using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader i { get; private set; }
    Image image;

    private void Awake()
    {
        i = this;
        image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
        yield return image.DOFade(1f, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0f, time).WaitForCompletion();
    }

    public IEnumerator BlinkEffect()
    {
        float blinkDuration = 0.25f;
        int blinkCount = 5;

        var blinkSequence = DOTween.Sequence();
        for (int i = 0; i < blinkCount; i++)
        {
            blinkSequence.Append(image.DOFade(1f, blinkDuration / 2));
            blinkSequence.Append(image.DOFade(0f, blinkDuration / 2));
        }

        blinkSequence.Append(image.DOFade(1f, blinkDuration / 2));

        yield return blinkSequence.WaitForCompletion();
    }
}
