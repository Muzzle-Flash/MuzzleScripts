using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class MuzzleLoadingBarrel : MonoBehaviour
    {
        public MuzzleLoadingIgnitionSource IgnitionSource;

        [HideInInspector]
        public List<MuzzleLoadedElement> LoadedElements = new List<MuzzleLoadedElement>();
        public List<GameObject> DisplayProxies = new List<GameObject>();

        private static readonly Dictionary<MuzzleLoadedElement, MuzzleLoadingBarrel> _existingMuzzleLoadedElements = new Dictionary<MuzzleLoadedElement, MuzzleLoadingBarrel>();
        public MuzzleLoadedElement GetLastLoadedElement()
        {
            if (this.LoadedElements.Count > 0)
            {
                return this.LoadedElements[this.LoadedElements.Count - 1];
            }
            else
            {
                return null;
            }
        }
        public void AddElement(MuzzleLoadedObject loadedObject)
        {
            MuzzleLoadedElement loadedElement = loadedObject.Element;
            if (loadedElement == null) return;
            if (this.LoadedElements.Count > 0)
            {
                if ((loadedElement.Type == MuzzleLoadedElement.MuzzleLoadedElementType.Powder || loadedElement.Type == MuzzleLoadedElement.MuzzleLoadedElementType.Shot) && loadedElement.Type == this.GetLastLoadedElement().Type)
                {
                    this.GetLastLoadedElement().Amount += loadedElement.Amount;
                }
                else
                {
                    MuzzleLoadedElement element = new MuzzleLoadedElement();
                    element.Type = loadedElement.Type;
                    element.Position = 0;
                    element.Amount = loadedElement.Amount;
                    element.Meshes = (from m in loadedElement.Meshes where m != null select m).ToList();
                    element.Material = loadedElement.Material;
                    this.LoadedElements.Add(element);
                    GameObject elementProxy = new GameObject("Proxy", typeof(MeshRenderer), typeof(MeshFilter));
                    this.DisplayProxies.Add(elementProxy);
                }
            }
            else
            {
                MuzzleLoadedElement element = new MuzzleLoadedElement();
                element.Type = loadedElement.Type;
                element.Position = 0;
                element.Amount = loadedElement.Amount;
                foreach (Mesh mesh in loadedElement.Meshes)
                {
                    element.Meshes.Add(mesh);
                }
                element.Material = loadedElement.Material;
                this.LoadedElements.Add(element);
                GameObject elementProxy = new GameObject("Proxy", typeof(MeshRenderer), typeof(MeshFilter));
                this.DisplayProxies.Add(elementProxy);
            }
        }
        public void OnTriggerEnter(Collider other)
        {
            GameObject gameObject = other.gameObject;
            if (gameObject == null) return;
            MuzzleLoadedElement element = gameObject.GetComponent<MuzzleLoadedElement>();
            if (element != null)
            {
                if (!this.CanElementFit(element)) return;

                return;
            }
            MuzzleLoadingProxyRamRod ramrod = gameObject.GetComponent<MuzzleLoadingProxyRamRod>();
            if (ramrod != null)
            {
                return;
            }
        }

        private bool CanElementFit(MuzzleLoadedElement element)
        {
            return this.LoadedElements.Count == 0 || this.GetLastLoadedElement().Position > this.GetLengthOfElement(element);
        }

        private float GetLengthOfElement(MuzzleLoadedElement element)
        {
            throw new NotImplementedException();
        }
    }
}
