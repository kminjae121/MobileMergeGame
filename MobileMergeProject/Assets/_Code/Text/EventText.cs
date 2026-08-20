using System;
using System.Collections.Generic;
using _Code.SO;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

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
    }

    private void EnterTxt()
    {
        
    }
}