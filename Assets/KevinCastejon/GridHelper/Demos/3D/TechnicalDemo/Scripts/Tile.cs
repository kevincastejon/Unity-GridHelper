using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.TechnicalDemo
{
    public class Tile : MonoBehaviour, ITile3D
    {
        public bool IsWalkable { get => false; }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }
    }
}