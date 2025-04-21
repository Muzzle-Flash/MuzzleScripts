using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace MuzzleScripts
{
    [Serializable]
    public class MuzzleLoadedElement
    {
        public MuzzleLoadedElementType Type;
        public float Position;
        public int Amount;
        public List<Mesh> Meshes = new List<Mesh>();
        public Material Material;
        public GameObject ProjectilePrefab;
        public GameObject SelfPrefab;
        private GameObject _proxyGameObject;
        private MeshRenderer _proxyMeshRenderer;

        public enum MuzzleLoadedElementType
        {
            Powder,
            Ball,
            Shot,
            Wadding
        }
    }
}
