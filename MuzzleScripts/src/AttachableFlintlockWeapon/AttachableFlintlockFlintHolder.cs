using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockFlintHolder : FVRInteractiveObject
    {
        public AttachableFlintlockFlintScrew Screw;

        public GameObject FlintPrefab;

        public AudioEvent AudEvent_Remove;

        public AudioEvent AudEvent_Replace;

        public Transform FlintPos;

        private float TimeTilFlintReplace = 1f;

        public override bool IsInteractable()
        {
            return this.Screw.SState == FlintlockFlintScrew.ScrewState.Unscrewed;
        }

        public override void SimpleInteraction(FVRViveHand hand)
        {
            if (this.Screw.Weapon.HasFlint())
            {
                Vector3 u = this.Screw.Weapon.RemoveFlint();
                this.ExtractFlint(hand, u);
                this.Screw.Weapon.PlayAudioAsHandling(this.AudEvent_Remove, base.transform.position);
            }
            base.SimpleInteraction(hand);
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.TimeTilFlintReplace > 0f)
            {
                this.TimeTilFlintReplace -= Time.deltaTime;
            }
        }

        private void ExtractFlint(FVRViveHand h, Vector3 u)
        {
            this.TimeTilFlintReplace = 1f;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.FlintPrefab, this.FlintPos.position, this.FlintPos.rotation);
            FlintlockFlint component = gameObject.GetComponent<FlintlockFlint>();
            component.m_flintUses = u;
            component.UpdateState();
            h.ForceSetInteractable(component);
            component.BeginInteraction(h);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (this.TimeTilFlintReplace > 0f)
            {
                return;
            }
            if (this.Screw.Weapon.HasFlint())
            {
                return;
            }
            if (other.attachedRigidbody == null)
            {
                return;
            }
            GameObject gameObject = other.attachedRigidbody.gameObject;
            if (gameObject.CompareTag("flintlock_flint"))
            {
                FlintlockFlint component = gameObject.GetComponent<FlintlockFlint>();
                this.Screw.Weapon.AddFlint(component.m_flintUses);
                this.Screw.Weapon.PlayAudioAsHandling(this.AudEvent_Replace, base.transform.position);
                UnityEngine.Object.Destroy(other.gameObject);
            }
        }
    }
}
