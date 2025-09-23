using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class LaserWeaponHeatsinkReleaseTrigger : FVRInteractiveObject
    {
        public LaserWeapon LaserWeapon;

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
            if (this.LaserWeapon.Heatsink != null)
            {
                this.EndInteraction(hand);
                LaserWeaponHeatsink heatsink = this.LaserWeapon.Heatsink;
                this.LaserWeapon.RemoveHeatsink();
                UnityEngine.Debug.Log("Please kill me");
                hand.ForceSetInteractable(heatsink);
                heatsink.BeginInteraction(hand);
            }
        }
        
        public override bool IsInteractable()
        {
            return (this.LaserWeapon.Heatsink != null) && (!this.LaserWeapon.DoesSlotRequireManualPartToAccess || this.LaserWeapon.MovablePart.IsOpen);
        }
    }
}
