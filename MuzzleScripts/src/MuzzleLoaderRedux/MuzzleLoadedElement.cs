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
        public List<Mesh> Meshes = new List<Mesh>();
        public Material Material;
        public GameObject ProjectilePrefab;
        public FVRObject ObjectWrapper;
        public MeshRenderer MeshRenderer;
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
            Meshes = new List<Mesh>();
            foreach (Mesh mesh in muzzleLoadedElement.Meshes)
            {
                Meshes.Add(mesh);
            }
            Material = muzzleLoadedElement.Material;
            ProjectilePrefab = muzzleLoadedElement.ProjectilePrefab;
            ObjectWrapper = muzzleLoadedElement.ObjectWrapper;
            GameObject RendererObject = new GameObject("Renderer");
            MeshRenderer = RendererObject.AddComponent<MeshRenderer>();
            MeshRenderer.material = Material;
            MeshFilter meshFilter = RendererObject.AddComponent<MeshFilter>();
            RenderUpdate();
        }
        public void RenderUpdate()
        {
            if (this.MeshRenderer != null && this.Meshes.Count>1)
            {
                float floatIndex = Mathf.InverseLerp(0f, MaximumAmount, Amount);
                floatIndex *= Meshes.Count;
                int MeshIndex = Mathf.FloorToInt(floatIndex);
                MeshIndex = Mathf.Clamp(MeshIndex, 0, Meshes.Count - 1);
                this.MeshRenderer.GetComponent<MeshFilter>().sharedMesh = Meshes[MeshIndex];
            }
        }
    }
}
