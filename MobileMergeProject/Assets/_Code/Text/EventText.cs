using System;
using System.Collections.Generic;
using _Code.SO;
using Code.Core.Events.Bus;
using Code.Core.Events.Bus.TextEvent;
using Code.Core.Managers;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EventText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI eventTxt;
    [SerializeField] private List<TextSO> txtList;

    private Dictionary<TextType, TextSO> txtDict;

    private void Awake()
    {
        foreach (var txt in txtList)
        {
            txtDict.Add(txt.EventType, txt);
        }
        
        Bus<EventTxtEvent>.Subscribe(EnterTxt);
    }

    private void OnDestroy()
    {
        Bus<EventTxtEvent>.Subscribe(EnterTxt);
    }

    private void EnterTxt(EventTxtEvent evt)
    {
        if (txtDict.TryGetValue(evt.TxtTypeType, out TextSO txt))
        {
            eventTxt.DOKill();
            eventTxt.text = "";

            eventTxt.DoText(txt.Text, 0.8f)
                .OnComplete(() => eventTxt.text = "");
        }
    }
}