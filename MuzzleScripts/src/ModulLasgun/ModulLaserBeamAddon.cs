using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using ModularWorkshop;
using OpenScripts2;
using UnityEngine;

namespace MuzzleScripts
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class ModulLaserBeamAddon : MonoBehaviour, IPartFireArmRequirement
    {
        private FVRFireArm _firearm;
        private LaserBeamPrefab _origLaserBeamPrefab;
        public LaserBeamPrefab LaserBeamPrefab;

        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    this._firearm = value;
                    LaserWeapon laserWeapon = this._firearm as LaserWeapon;
                    if (laserWeapon != null)
                    {
                        this._origLaserBeamPrefab = laserWeapon.orig_LaserBeamPrefab;
                        laserWeapon.orig_LaserBeamPrefab = this.LaserBeamPrefab;
                        laserWeapon.SetBeam(this.LaserBeamPrefab);
                    }
                }
                else if (value == null && this._firearm != null)
                {
                    LaserWeapon laserWeapon = this._firearm as LaserWeapon;
                    if (laserWeapon != null)
                    {
                        laserWeapon.orig_LaserBeamPrefab = this._origLaserBeamPrefab;
                        laserWeapon.SetBeam(this._origLaserBeamPrefab);
                    }
                }
            }
        }
    }
}
