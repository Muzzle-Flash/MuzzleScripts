using FistVR;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuzzleScripts
{
    public class RevolverSecondaryBarrelSlidingFore : FVRAlternateGrip
    {
        [Header("Revolver Secondary Barrel Sliding Fore")]
        public RevolverSecondaryBarrel SecondaryBarrel;
        public M203_Fore.ForePos CurPos = M203_Fore.ForePos.Rearward;
        public M203_Fore.ForePos LastPos = M203_Fore.ForePos.Rearward;
        public Transform Point_Forward;
        public Transform Point_Rearward;
        public AudioEvent AudEvent_BreachClose;
        public AudioEvent AudEvent_BreachOpen;
        public AudioEvent AudEvent_EjectRound;
        private float m_handZOffset;
        private float m_curSlideSpeed;
        private float m_slideZ_current;
        private float m_slideZ_heldTarget;
        private float m_slideZ_forward;
        private float m_slideZ_rear;
        public Transform EjectPos;
        private bool m_isLocked;
        public Transform Button;
        public Vector3 ButtonPressed;
        public Vector3 ButtonUnpressed;
        private bool m_IsButtonHeld;

        public override void Awake()
        {
            base.Awake();
            this.m_slideZ_current = base.transform.localPosition.z;
            this.m_slideZ_forward = this.Point_Forward.localPosition.z;
            this.m_slideZ_rear = this.Point_Rearward.localPosition.z;
        }
        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
            this.m_handZOffset = base.transform.InverseTransformPoint(hand.Input.Pos).z;
        }
        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.m_IsButtonHeld)
            {
                this.Button.localPosition = this.ButtonPressed;
                this.m_isLocked = false;
            }
            else
            {
                this.Button.localPosition = this.ButtonUnpressed;
            }
        }
        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            Vector2 vector = new Vector2(this.m_slideZ_rear, this.m_slideZ_forward);
            float curZ = this.m_slideZ_current;
            float tarZ = this.m_slideZ_current;
            this.m_IsButtonHeld = false;
            if (hand.IsInStreamlinedMode)
            {
                if (this.m_hand.Input.BYButtonDown)
                {
                    this.m_IsButtonHeld = true;
                }
            }
            else if (hand.Input.TouchpadPressed && Vector3.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
            {
                this.m_IsButtonHeld = true;
            }
            if (base.IsHeld)
            {
                Vector3 closestValidPoint = base.GetClosestValidPoint(this.Point_Forward.position, this.Point_Rearward.position,
                    hand.Input.Pos - base.transform.forward * this.m_handZOffset * this.SecondaryBarrel.revolver.transform.localScale.x);
                this.m_slideZ_heldTarget = this.SecondaryBarrel.revolver.transform.InverseTransformPoint(closestValidPoint).z;
                tarZ = this.m_slideZ_heldTarget;
                curZ = Mathf.MoveTowards(this.m_slideZ_current, tarZ, 5f * Time.deltaTime);
            }
            curZ = Mathf.Clamp(curZ, vector.x, vector.y);
            if (Mathf.Abs(curZ - this.m_slideZ_current) > Mathf.Epsilon)
            {
                if (!m_isLocked)
                {
                    this.m_slideZ_current = curZ;
                    base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, this.m_slideZ_current);
                }
            }
            else
            {
                this.m_curSlideSpeed = 0f;
            }
            M203_Fore.ForePos value = this.CurPos;
            if (Mathf.Abs(this.m_slideZ_current - this.m_slideZ_forward) < 0.003f)
            {
                value = M203_Fore.ForePos.Forward;
            }
            else if (Mathf.Abs(this.m_slideZ_current - this.m_slideZ_rear) < 0.001f)
            {
                value = M203_Fore.ForePos.Rearward;
            }
            else
            {
                value = M203_Fore.ForePos.Mid;
            }
            int curPos = (int)this.CurPos;
            this.CurPos = (M203_Fore.ForePos)Mathf.Clamp((int)value, curPos - 1, curPos + 1);
            if (this.CurPos == M203_Fore.ForePos.Rearward && this.LastPos != M203_Fore.ForePos.Rearward)
            {
                this.SecondaryBarrel.Chamber.IsAccessible = false;
                this.CloseEvent();
            }
            else if (this.CurPos != M203_Fore.ForePos.Rearward && this.LastPos == M203_Fore.ForePos.Rearward)
            {
                this.SecondaryBarrel.Chamber.IsAccessible = true;
                this.OpenEvent();
            }
            else if (this.CurPos == M203_Fore.ForePos.Forward && this.LastPos != M203_Fore.ForePos.Forward)
            {
                this.EjectEvent();
            }
            this.LastPos = this.CurPos;
        }
        public void CloseEvent()
        {
            this.SecondaryBarrel.revolver.PlayAudioAsHandling(AudEvent_BreachClose, base.transform.position);
            this.SecondaryBarrel.IsSecondaryBarrelClosed = true;
            this.m_isLocked = true;
        }
        public void OpenEvent()
        {
            this.SecondaryBarrel.revolver.PlayAudioAsHandling(AudEvent_BreachOpen, base.transform.position);
            this.SecondaryBarrel.IsSecondaryBarrelClosed = false;
            this.m_isLocked = false;
        }
        public void EjectEvent()
        {
            this.SecondaryBarrel.IsSecondaryHammerCocked = true;
            if (this.SecondaryBarrel.Chamber.IsFull)
            {
                this.SecondaryBarrel.Chamber.EjectRound(this.EjectPos.position, -base.transform.forward, Vector3.zero, false);
                this.SecondaryBarrel.revolver.PlayAudioAsHandling(AudEvent_EjectRound, base.transform.position);
            }
        }
    }
}
