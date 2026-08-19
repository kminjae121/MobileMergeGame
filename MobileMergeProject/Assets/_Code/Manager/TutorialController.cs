using System;
using System.Collections.Generic;
using System.Linq;
using _Code.Block;
using _Code.SO;
using TMPro;
using UnityEngine;

namespace _Code.Manager
{
    public sealed class TutorialController : MonoBehaviour
    {
        private List<TutorialSO> _tutoList =  new List<TutorialSO>();

        private void Awake()
        {
            _tutoList = _tutoList
                .OrderBy(tuto => tuto.TutorialNum)
                .ToList();
        }
    }
}
