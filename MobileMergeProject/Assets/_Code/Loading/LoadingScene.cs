using System;
using System.Collections;
using System.Collections.Generic;
using Code.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Image barImg;
    [SerializeField] private float loadingSec;
    [SerializeField] private TextMeshProUGUI loadingTxt;
    [SerializeField] private List<string> loadingStrs; 
        
    private bool isLoading = true;

    
    private void Awake()
    {
        StartLoading();
    }

    private void StartLoading()
    {
        StartCoroutine(LoadingTxt());
        
        barImg.DOFillAmount(1, loadingSec).SetEase(Ease.InOutCirc)
            .OnComplete(() =>
            {
                isLoading = false;
                gameObject.SetActive(false);
            });
    }


    private IEnumerator LoadingTxt()
    {
        int idx = 0;
        
        while (isLoading)
        {
            if (idx > loadingStrs.Count - 1)
                idx = 0;
            
            yield return new WaitForSeconds(0.4f);

            loadingTxt.text = loadingStrs[idx];

            idx += 1;
        }
    }
}
