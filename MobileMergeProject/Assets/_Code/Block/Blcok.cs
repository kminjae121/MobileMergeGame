using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public abstract class Blcok : MonoBehaviour, IInstallable
    {
        public abstract IReadOnlyList<Vector2Int> Cells { get; }
        public abstract Sprite BlockSprite { get; }
        public int CellCount => Cells.Count;
    }   
}
