using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace MuzzleScripts
{
    public class AttachableFlintlockFrizenTrigger : FVRInteractiveObject
    {
        public AttachableFlintlockFlashPan FlashPan;

        public Transform DistPoint;

        public override bool IsInteractable()
        {
            return this.FlashPan.GetWeapon().HammerState != FlintlockWeapon.HState.Uncocked;
        }

        public override void SimpleInteraction(FVRViveHand hand)
        {
            this.FlashPan.ToggleFrizenState();
            base.SimpleInteraction(hand);
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.FlashPan.GetWeapon().Attachment.IsHeld)
            {
                FVRViveHand otherHand = this.FlashPan.GetWeapon().Attachment.m_hand.OtherHand;
                Vector3 closestValidPoint = base.GetClosestValidPoint(otherHand.Input.Pos, otherHand.PalmTransform.position, this.DistPoint.position);
                float num = Vector3.Distance(this.DistPoint.position, closestValidPoint);
                if (num < 0.045f)
                {
                    Vector3 vector = this.FlashPan.GetWeapon().transform.InverseTransformVector(otherHand.Input.VelLinearWorld);
                    if (this.FlashPan.FrizenState == FlintlockFlashPan.FState.Up)
                    {
                        if (vector.z < -0.5f)
                        {
                            this.FlashPan.ToggleFrizenState();
                        }
                    }
                    else if (vector.z > 0.5f)
                    {
                        this.FlashPan.ToggleFrizenState();
                    }
                }
            }
        }
    }
}
