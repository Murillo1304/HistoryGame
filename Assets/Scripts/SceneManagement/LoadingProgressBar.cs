using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressBar : MonoBehaviour
{
    Image image;

    private void Awake()
    {
        image = transform.GetComponent<Image>();
    }
    private void Update()
    {
        image.transform.localScale = new Vector3(Loader.GetLoadingProgress(), 1f);
    }
}
