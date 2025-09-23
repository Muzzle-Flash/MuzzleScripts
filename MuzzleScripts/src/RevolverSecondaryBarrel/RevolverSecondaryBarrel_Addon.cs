using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenScripts2;
using ModularWorkshop;
using UnityEngine;
using FistVR;

namespace MuzzleScripts
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class RevolverSecondaryBarrel_Addon : MonoBehaviour, IPartFireArmRequirement
    {
        private FVRFireArm _firearm;
        private Transform _origMuzzle;
        private FVRFireArmChamber _origChamber;
        private FVRFireArmRecoilProfile _origOverrideRecoilProfile;

        public Transform Muzzle;
        public FVRFireArmChamber Chamber;
        public FVRFireArmRecoilProfile OverrideRecoilProfile;

        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    this._firearm = value;
                    RevolverSecondaryBarrel secondaryBarrel = this._firearm.GetComponentInChildren<RevolverSecondaryBarrel>();
                    if (secondaryBarrel != null)
                    {
                        this._origMuzzle = secondaryBarrel.Muzzle;
                        this._origChamber = secondaryBarrel.Chamber;
                        this._origOverrideRecoilProfile = secondaryBarrel.OverrideRecoilProfile;
                        secondaryBarrel.Muzzle = this.Muzzle;
                        secondaryBarrel.Chamber = this.Chamber;
                        secondaryBarrel.Chamber.Firearm = this._firearm;
                        secondaryBarrel.OverrideRecoilProfile = this.OverrideRecoilProfile;
                        secondaryBarrel.RegisterChambers();
                        return;
                    }
                }
                else if (value == null && this._firearm != null)
                {
                    RevolverSecondaryBarrel secondaryBarrel = this._firearm.GetComponentInChildren<RevolverSecondaryBarrel>();
                    if (secondaryBarrel != null)
                    {
                        secondaryBarrel.Muzzle = this._origMuzzle;
                        secondaryBarrel.Chamber = this._origChamber;
                        secondaryBarrel.OverrideRecoilProfile = this._origOverrideRecoilProfile;
                        secondaryBarrel.RegisterChambers();
                    }
                }
            }
        }
    }
}
