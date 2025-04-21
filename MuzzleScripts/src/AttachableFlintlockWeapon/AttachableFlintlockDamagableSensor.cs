using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockDamagableSensor : MonoBehaviour, IFVRDamageable
    {

        public AttachableFlintlockFlashPan Pan;
        public AttachableFlintlockBarrel Barrel;

        public void Damage(Damage d)
        {
            if (d.Dam_Thermal > 1f)
            {
                if (this.Pan != null && this.Pan.FrizenState == FlintlockFlashPan.FState.Up)
                {
                    this.Pan.Ignite();
                }
                if (this.Barrel != null)
                {
                    this.Barrel.BurnOffOuter();
                }
            }
        }
    }
}
