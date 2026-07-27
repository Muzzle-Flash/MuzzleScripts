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
        public MuzzleLoadingIgnitionSource IgnitionSource;
        public MuzzleLoadedElement BallDefault;
        private List<MuzzleLoadedElement> m_loadedElements = new List<MuzzleLoadedElement>();
        public MuzzleLoadedElement GetLastLoadedElement()
        {
            if (m_loadedElements.Count == 0) return null;
            return m_loadedElements[m_loadedElements.Count - 1];
        }
        private bool CanElementFit(MuzzleLoadedElement element)
        {
            return this.m_loadedElements.Count == 0 || this.GetLastLoadedElement().Position > element.Length;
        }
        public void InsertElement(MuzzleLoadedElement element)
        {
            MuzzleLoadedElement newElement = new MuzzleLoadedElement(element);
            m_loadedElements.Add(newElement);
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
                        if (lastLoadedElement.Type == MuzzleLoadedElement.MuzzleLoadedElementType.Powder && lastLoadedElement.Amount < lastLoadedElement.MaximumAmount)
                        {
                            int amountToAdd = Mathf.Min(element.Amount, (lastLoadedElement.Amount - lastLoadedElement.MaximumAmount));
                            lastLoadedElement.Amount += amountToAdd;
                            //destroy the powder object
                        }
                        else
                        {
                            //create new powder element
                        }
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Ball:
                        InsertElement(element);
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Shot:
                        if(lastLoadedElement.Type==MuzzleLoadedElement.MuzzleLoadedElementType.Shot && lastLoadedElement.Amount < lastLoadedElement.MaximumAmount)
                        {
                            int amountToAdd = Mathf.Min(element.Amount, (lastLoadedElement.Amount - lastLoadedElement.MaximumAmount));
                            lastLoadedElement.Amount += amountToAdd;
                            //destroy the shot object
                        }
                        else
                        {
                            //create new shot element
                        }
                        break;
                    case MuzzleLoadedElement.MuzzleLoadedElementType.Wadding:
                        InsertElement(element);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
