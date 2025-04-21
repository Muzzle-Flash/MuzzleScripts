using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using ModularWorkshop;
using OpenScripts2;
using UnityEngine;

namespace MuzzleScripts.src.ShotgunSlamfireAddon
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class ShotgunSlamfireAddon : MonoBehaviour, IPartFireArmRequirement
    {
        private FVRFireArm _firearm;
        private bool _origUsesSlamFire;
        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    this._firearm = value;
                    TubeFedShotgun tubeFedShotgun = this._firearm as TubeFedShotgun;
                    if (tubeFedShotgun != null)
                    {
                        this._origUsesSlamFire = tubeFedShotgun.UsesSlamFireTrigger;
                        tubeFedShotgun.UsesSlamFireTrigger = true;
                        return;
                    }
                }
                else if (value == null && this._firearm != null)
                {
                    TubeFedShotgun tubeFedShotgun = this._firearm as TubeFedShotgun;
                    if (tubeFedShotgun != null)
                    {
                        tubeFedShotgun.UsesSlamFireTrigger = this._origUsesSlamFire;
                    }
                }
            }
        }
    }
}
