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
        public float Length;
        public int Amount = 1;
        public int MaximumAmount = 1;
        public List<MeshRenderer> MesheRenderers = new List<MeshRenderer>();
        public Material Material;
        public GameObject ProjectilePrefab;
        public FVRObject ObjectWrapper;
        public enum MuzzleLoadedElementType
        {
            Powder,
            Ball,
            Shot,
            Wadding
        }
        public MuzzleLoadedElement(MuzzleLoadedElement muzzleLoadedElement)
        {
            this.Type = muzzleLoadedElement.Type;
            Position = muzzleLoadedElement.Position;
            Length = muzzleLoadedElement.Length;
            Amount = muzzleLoadedElement.Amount;
            MaximumAmount = muzzleLoadedElement.MaximumAmount;
            MesheRenderers = new List<MeshRenderer>();
            foreach (MeshRenderer renderer in muzzleLoadedElement.MesheRenderers)
            {
                MesheRenderers.Add(renderer);
            }
            Material = muzzleLoadedElement.Material;
            ProjectilePrefab = muzzleLoadedElement.ProjectilePrefab;
            ObjectWrapper = muzzleLoadedElement.ObjectWrapper;
        }
    }
}
