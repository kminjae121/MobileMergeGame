using System;
using System.Collections.Generic;
using _Code.SO;
using Code.Core.Events.Bus;
using Code.Core.Events.Bus.TextEvent;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EventText : MonoBehaviour
{
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
        txtDict.TryGetValue(evt.TxtTypeType, out TextSO txt);

        if (txt != null)
        {
            //텍스트 작업하기     
        }
    }
}