using FistVR;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MuzzleScripts
{
    public class RepeatingFlintlockBarrels : FVRInteractiveObject
    {
        public FlintlockWeapon Weapon;
        public Transform BarrelCluster;
        public bool RotatesClockwise = false;
        public FVRPhysicalObject.Axis Axis;
        public ActuationType Type;
        public float RotationRate = 4f;
        public float RotationCooldown = 0.5f;

        public enum ActuationType
        {
            Manual,
            Hammer
        }
        public void AdvancePan()
        {
            if (this.Weapon.HammerState != FlintlockWeapon.HState.Uncocked)
            {
                StartCoroutine(RotateCluster());
                if (this.RotatesClockwise)
                {
                    this.Weapon.m_curFlashpan--;
                    this.Weapon.m_curFlashpan = (int)Mathf.Repeat(Weapon.m_curFlashpan, Weapon.FlashPans.Count);
                }
                else
                {
                    this.Weapon.m_curFlashpan++;
                    this.Weapon.m_curFlashpan = (int)Mathf.Repeat(Weapon.m_curFlashpan, Weapon.FlashPans.Count);
                }
                StopCoroutine(RotateCluster());
            }
        }
        IEnumerator RotateCluster()
        {
            int next_pan;
            if (this.RotatesClockwise)
            {
                next_pan = (int)Mathf.Repeat((Weapon.m_curFlashpan - 1), Weapon.FlashPans.Count);
            }
            else
            {
                next_pan = (int)Mathf.Repeat((Weapon.m_curFlashpan + 1), Weapon.FlashPans.Count);
            }
            var t = 0f;
            var start = GetLocalRotation(Weapon.m_curFlashpan);
            var target = GetLocalRotation(next_pan);
            while (t < 1)
            {
                t += Time.deltaTime * RotationRate;
                if (t > 1) t = 1;
                this.BarrelCluster.localRotation = Quaternion.Slerp(start, target, t);
                yield return null;
            }

            
        }
        public override void SimpleInteraction(FVRViveHand hand)
        {
            if (RotationCooldown <= 0.2)
            {
                base.SimpleInteraction(hand);
                if (this.Type == ActuationType.Manual)
                {
                    AdvancePan();
                    RotationCooldown = 0f;
                }
            }
        }
        public Quaternion GetLocalRotation(int flashpan)
        {
            float num = (float)flashpan*(360f/(float)this.Weapon.FlashPans.Count);
            num = Mathf.Repeat(num, 360f);
            return Quaternion.Euler(0f, 0, num);
        }
        public override void FVRUpdate()
        {
           base.FVRUpdate();
            if (RotationCooldown < 1f)
            {
                RotationCooldown += Time.deltaTime;
            }

        }
    }
}