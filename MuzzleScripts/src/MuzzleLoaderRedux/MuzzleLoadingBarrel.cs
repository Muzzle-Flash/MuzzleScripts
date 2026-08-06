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
        public Transform Muzzle;
        public Transform BarrelOrigin;
        public MuzzleLoadingIgnitionSource IgnitionSource;
        public MuzzleLoadedObject BallDefault;
        public MuzzleLoadedObject PowderDefault;
        public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        private List<MuzzleLoadedElement> m_loadedElements = new List<MuzzleLoadedElement>();
        public bool IsBeingRammed = false;
        public float CurrentRamrodDepth = 0f;
        public MuzzleLoadedElement GetLastLoadedElement()
        {
            if (m_loadedElements.Count == 0) return null;
            return m_loadedElements[m_loadedElements.Count - 1];
        }
        private bool CanElementFit(MuzzleLoadedElement element)
        {
            return this.m_loadedElements.Count == 0 || this.GetLastLoadedElement().Position > element.Length;
        }
        private void CreateNewElement(MuzzleLoadedElement element)
        {
            MuzzleLoadedElement newElement = new MuzzleLoadedElement(element);
            m_loadedElements.Add(newElement);
            meshRenderers.Add(newElement.MeshRenderer);
            newElement.MeshRenderer.enabled = true;
            newElement.MeshRenderer.transform.SetParent(this.transform);
            newElement.MeshRenderer.transform.rotation = element.MeshRenderer.transform.rotation;
            newElement.MeshRenderer.transform.localScale = element.MeshRenderer.transform.localScale;
            newElement.RenderUpdate();
        }
        public void InsertElement(MuzzleLoadedElement element)
        {
            if (this.m_loadedElements.Count > 0)
            {
                MuzzleLoadedElement lastLoadedElement = this.m_loadedElements[this.m_loadedElements.Count - 1];
                if (element.Type == MuzzleLoadedElement.MuzzleLoadedElementType.Powder || element.Type == MuzzleLoadedElement.MuzzleLoadedElementType.Shot)
                {
                    if (lastLoadedElement.Type == element.Type && lastLoadedElement.Amount < lastLoadedElement.MaximumAmount)
                    {
                        lastLoadedElement.Amount += element.Amount;
                        lastLoadedElement.Amount = Mathf.Clamp(lastLoadedElement.Amount, 0, lastLoadedElement.MaximumAmount);
                        lastLoadedElement.RenderUpdate();
                        return;
                    }
                    CreateNewElement(element);
                    return;
                }
                CreateNewElement(element);
                return;
            }
            CreateNewElement(element);
            return;
        }
        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody== null) return;
            float muzzleAngle = Vector3.Angle(this.Muzzle.forward, Vector3.up);
            if (muzzleAngle > 90f) return;
            GameObject gameObject = other.attachedRigidbody.gameObject;
            MuzzleLoadedObject muzzleLoadedObject = gameObject.GetComponent<MuzzleLoadedObject>();
            MuzzleLoadedElement lastLoadedElement = this.GetLastLoadedElement();
            if (muzzleLoadedObject != null)
            {
                MuzzleLoadedElement element = muzzleLoadedObject.Element;
                if (!this.CanElementFit(element)) return;
                switch (element.Type)
                {
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Powder:
                        InsertElement(PowderDefault.Element);
                        //play audio
                        UnityEngine.Object.Destroy(other.gameObject);
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Ball:
                        InsertElement(element);
                        //play audio
                        UnityEngine.Object.Destroy(other.gameObject);
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Shot:
                        InsertElement(element);
                        //play audio
                        UnityEngine.Object.Destroy(other.gameObject);
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Wadding:
                        InsertElement(element);
                        //play audio
                        UnityEngine.Object.Destroy(other.gameObject);
                        break;
                    default:
                        break;
                }
            }
            else if (gameObject.CompareTag("flintlock_powdergrain"))
            {
                if (CanElementFit(PowderDefault.Element))
                {
                    InsertElement(PowderDefault.Element);
                    UnityEngine.Object.Destroy(other.gameObject);
                }
            }
            else if (gameObject.CompareTag("flintlock_shot"))
            {
                if (CanElementFit(BallDefault.Element))
                {
                    InsertElement(BallDefault.Element);
                    UnityEngine.Object.Destroy(other.gameObject);
                }
            }
        }
        private int ExpelElement(MuzzleLoadedElement loadedElement)
        {
            MuzzleLoadedElement.MuzzleLoadedElementType type = loadedElement.Type;
            int amount = loadedElement.Amount;
            if (type == MuzzleLoadedElement.MuzzleLoadedElementType.Powder || type == MuzzleLoadedElement.MuzzleLoadedElementType.Shot)
            {
                Vector3 Position = this.Muzzle.position + this.Muzzle.forward * loadedElement.Length * 0.75f;
                UnityEngine.Object.Instantiate<GameObject>(loadedElement.ObjectWrapper.GetGameObject(), Position, this.Muzzle.rotation); //spawn instance of element out of the barrel
                return 0;
            }
            int result = amount - 1;
            Vector3 Position2 = this.Muzzle.position + UnityEngine.Random.onUnitSphere * 0.005f + this.Muzzle.forward * loadedElement.Length * 0.75f;
            UnityEngine.Object.Instantiate<GameObject>(loadedElement.ObjectWrapper.GetGameObject(), Position2, this.Muzzle.rotation); //spawn instance of element out of the barrel
            return result;

        }
        public void SimulateBarrelContents()
        {
            if (this.m_loadedElements.Count == 0) return;
            float barrelAngle = Vector3.Angle(this.Muzzle.forward, Vector3.up);
            float BarrelLength = Vector3.Distance(Muzzle.position, BarrelOrigin.position);
            if (this.IsBeingRammed)
            {
                float totalElementsLength = 0f;
                for (int i = 0; i < this.m_loadedElements.Count; i++)
                {
                    totalElementsLength += this.m_loadedElements[i].Length;
                }
                float maxRamrodDepth = BarrelLength - totalElementsLength;
                float clampedRamrodDepth = Mathf.Clamp(this.CurrentRamrodDepth, 0f, maxRamrodDepth);
                MuzzleLoadedElement topmostElement = this.m_loadedElements[this.m_loadedElements.Count - 1];
                if (clampedRamrodDepth > topmostElement.Position)
                {
                    topmostElement.Position = clampedRamrodDepth;
                }
            }
            if (barrelAngle > 90f)
            {
                for (int i = this.m_loadedElements.Count - 1; i >= 0; i--)
                {
                    MuzzleLoadedElement loadedElement = this.m_loadedElements[i];
                    float min = 0f;
                    float max = BarrelLength;
                    bool isTopmost = (i == this.m_loadedElements.Count - 1);

                    if (i + 1 < this.m_loadedElements.Count)
                    {
                        min = this.m_loadedElements[i + 1].Position + this.m_loadedElements[i + 1].Length;
                    }
                    if (i - 1 >= 0)
                    {
                        max = this.m_loadedElements[i - 1].Position - loadedElement.Length;
                    }
                    bool isPlug = (loadedElement.Type != MuzzleLoadedElement.MuzzleLoadedElementType.Powder && loadedElement.Type != MuzzleLoadedElement.MuzzleLoadedElementType.Shot);
                    float newPosition = loadedElement.Position;

                    if (!isPlug)
                    {
                        newPosition -= ((barrelAngle - 90f) / 90f * 2f) * Time.deltaTime;
                    }
                    if (isTopmost && newPosition <= 0 && !isPlug)
                    {
                        int remainingAmount = this.ExpelElement(loadedElement);
                        if (remainingAmount <= 0f)
                        {
                            UnityEngine.Object toDestroy = loadedElement.MeshRenderer.gameObject;
                            this.m_loadedElements.RemoveAt(i);
                            Destroy(toDestroy);
                        }
                        else
                        {
                            loadedElement.Amount = remainingAmount;
                        }
                    }
                    else
                    {
                        newPosition = Mathf.Clamp(newPosition, min, max);
                        loadedElement.Position = newPosition;
                    }
                }
            }
            else
            {
                for (int j = 0; j < this.m_loadedElements.Count; j++)
                {
                    MuzzleLoadedElement loadedElement = this.m_loadedElements[j];
                    float min = 0f;
                    float max = BarrelLength - loadedElement.Length;

                    if (j + 1 < this.m_loadedElements.Count)
                    {
                        min = this.m_loadedElements[j + 1].Position + this.m_loadedElements[j + 1].Length;
                    }
                    if (j - 1 >= 0)
                    {
                        max = this.m_loadedElements[j - 1].Position - loadedElement.Length;
                    }

                    bool isPlug = (loadedElement.Type != MuzzleLoadedElement.MuzzleLoadedElementType.Powder && loadedElement.Type != MuzzleLoadedElement.MuzzleLoadedElementType.Shot);
                    float newPosition = loadedElement.Position;

                    if (!isPlug)
                    {
                        newPosition += ((1f - barrelAngle / 90f) * 2f) * Time.deltaTime;
                    }

                    newPosition = Mathf.Clamp(newPosition, min, max);
                    loadedElement.Position = newPosition;
                }
            }
        }
        public void DrawBarrelContents()
        {
            foreach (MuzzleLoadedElement element in m_loadedElements)
            {
                element.MeshRenderer.transform.position = this.Muzzle.position - this.Muzzle.forward * element.Position;
                element.RenderUpdate();
            }
        }
        public void Update()
        {
            SimulateBarrelContents();
            DrawBarrelContents();
        }
    }
}
