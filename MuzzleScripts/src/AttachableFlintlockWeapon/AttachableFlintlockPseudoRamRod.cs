using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockPseudoRamRod : FVRInteractiveObject
    {
        public Transform Root;

        public FlintlockPseudoRamRod.RamRodState RState;

        public Transform Point_Lower_Rear;

        public Transform Point_Lower_Forward;

        private float lastHandZ;

        private float m_ramZ;

        private float m_minZ_lower;

        private float m_maxZ_lower;

        public GameObject RamRodPrefab;

        private float m_minZ_barrel;

        private float m_maxZ_barrel;

        private AttachableFlintlockBarrel m_curBarrel;

        [Header("Audio")]
        public AudioEvent AudEvent_Grab;

        public AudioEvent AudEvent_ExtractHolder;

        public AudioEvent AudEvent_ExtractBarrel;

        public AudioEvent AudEvent_InsertHolder;

        public AudioEvent AudEvent_InsertBarrel;

        private float m_curHandRodOffsetZ;

        public AttachableFlintlockBarrel GetCurBarrel()
        {
            return this.m_curBarrel;
        }

        public override void Awake()
        {
            base.Awake();
            this.m_ramZ = base.transform.localPosition.z;
            this.m_minZ_lower = this.Point_Lower_Rear.localPosition.z;
            this.m_maxZ_lower = this.Point_Lower_Forward.localPosition.z;
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
            Vector3 vector = Vector3.zero;
            if (this.RState == FlintlockPseudoRamRod.RamRodState.Lower)
            {
                vector = this.Root.InverseTransformPoint(hand.Input.Pos);
            }
            else
            {
                vector = this.m_curBarrel.Muzzle.InverseTransformPoint(hand.Input.Pos);
            }
            SM.PlayGenericSound(this.AudEvent_Grab, base.transform.position);
            this.lastHandZ = vector.z;
            this.m_curHandRodOffsetZ = base.transform.InverseTransformPoint(hand.Input.Pos).z;
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            Vector3 vector = Vector3.zero;
            if (this.RState == FlintlockPseudoRamRod.RamRodState.Lower)
            {
                vector = this.Root.InverseTransformPoint(hand.Input.Pos);
            }
            else
            {
                vector = this.m_curBarrel.Muzzle.InverseTransformPoint(hand.Input.Pos);
            }
            float z = vector.z;
            float delta = z - this.lastHandZ;
            delta = base.transform.InverseTransformPoint(hand.Input.Pos).z - this.m_curHandRodOffsetZ;
            this.MoveRamRod(delta, hand);
            this.lastHandZ = z;
        }

        public void MountToUnder(FVRViveHand h)
        {
            base.transform.SetParent(this.Root);
            this.RState = FlintlockPseudoRamRod.RamRodState.Lower;
            this.m_ramZ = this.m_maxZ_lower - 0.002f;
            SM.PlayGenericSound(this.AudEvent_InsertHolder, base.transform.position);
            base.transform.localPosition = new Vector3(this.Point_Lower_Rear.localPosition.x, this.Point_Lower_Rear.localPosition.y, this.m_ramZ);
            this.m_curHandRodOffsetZ = base.transform.InverseTransformPoint(h.Input.Pos).z;
        }

        public void MountToBarrel(AttachableFlintlockBarrel b, FVRViveHand h)
        {
            if (b == null)
            {
                this.m_curBarrel = null;
                base.gameObject.SetActive(false);
                return;
            }
            base.transform.SetParent(b.Muzzle);
            this.m_maxZ_barrel = 0.01f;
            this.m_minZ_barrel = -b.BarrelLength;
            this.m_curBarrel = b;
            this.m_ramZ = -0.02f;
            SM.PlayGenericSound(this.AudEvent_InsertBarrel, base.transform.position);
            base.transform.localPosition = new Vector3(this.Point_Lower_Rear.localPosition.x, this.Point_Lower_Rear.localPosition.y, this.m_ramZ);
            if (h != null)
            {
                this.m_curHandRodOffsetZ = base.transform.InverseTransformPoint(h.Input.Pos).z;
            }
        }

        private void MoveRamRod(float delta, FVRViveHand hand)
        {
            if (this.RState == FlintlockPseudoRamRod.RamRodState.Lower)
            {
                float ramZ = this.m_ramZ;
                this.m_ramZ += delta;
                this.m_ramZ = Mathf.Clamp(this.m_ramZ, this.m_minZ_lower, this.m_ramZ);
                float num = this.m_ramZ - ramZ;
                base.transform.localPosition = new Vector3(this.Point_Lower_Rear.localPosition.x, this.Point_Lower_Rear.localPosition.y, this.m_ramZ);
                if (this.m_ramZ >= this.m_maxZ_lower)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.RamRodPrefab, base.transform.position, base.transform.rotation);
                    this.ForceBreakInteraction();
                    FlintlockRamRod component = gameObject.GetComponent<FlintlockRamRod>();
                    hand.ForceSetInteractable(component);
                    component.BeginInteraction(hand);
                    SM.PlayGenericSound(this.AudEvent_ExtractHolder, base.transform.position);
                    this.Hide();
                }
            }
            else
            {
                this.m_ramZ += delta;
                float num2 = Mathf.Clamp(this.m_ramZ, -this.m_curBarrel.GetMaxDepth(), this.m_ramZ);
                base.transform.localPosition = new Vector3(0f, 0f, num2);
                if (this.m_ramZ < num2)
                {
                    this.m_curBarrel.Tamp((this.m_ramZ - num2) * 1f, this.m_ramZ);
                }
                if (this.m_ramZ >= 0f)
                {
                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.RamRodPrefab, base.transform.position, base.transform.rotation);
                    this.ForceBreakInteraction();
                    FlintlockRamRod component2 = gameObject2.GetComponent<FlintlockRamRod>();
                    hand.ForceSetInteractable(component2);
                    component2.BeginInteraction(hand);
                    SM.PlayGenericSound(this.AudEvent_ExtractBarrel, base.transform.position);
                    this.m_curBarrel = null;
                    this.Hide();
                }
                this.m_ramZ = num2;
            }
        }

        private void Hide()
        {
            base.gameObject.SetActive(false);
        }
    }
}
