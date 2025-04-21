using FistVR;
using ModularWorkshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class MuzzleLoadingWeapon : FVRFireArm
    {
        [Header("Muzzle Loading Weapon Config")]
        public List<MuzzleLoadingIgnitionSource> IgnitionSources;
        private int m_curFlashPan;
        public MuzzleLoadingProxyRamRod RamRod;
        public bool HasTrigger = true;
        public Transform Trigger;
        public FVRPhysicalObject.InterpStyle Trigger_InterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        public FVRPhysicalObject.Axis Trigger_Axis;
        public float Trigger_ForwardValue;
        public float Trigger_RearwardValue;
        private float FireReFire;
        private float m_triggerFloat = 0;
        public float TriggerFiringThreshold = 0.95f;
        public AudioEvent TriggerResetAudio;

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.FireReFire < 0.2f)
            {
                this.FireReFire += Time.deltaTime;
            }
            if (this.HasTrigger)
            {
                base.SetAnimatedComponent(this.Trigger, Mathf.Lerp(this.Trigger_ForwardValue, this.Trigger_RearwardValue, this.m_triggerFloat), this.Trigger_InterpStyle, this.Trigger_Axis);
            }
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            if (this.m_hasTriggeredUpSinceBegin)
            {
                this.m_triggerFloat = hand.Input.TriggerFloat;
            }
            else
            {
                this.m_triggerFloat = 0f;
            }
        }
    }
}
