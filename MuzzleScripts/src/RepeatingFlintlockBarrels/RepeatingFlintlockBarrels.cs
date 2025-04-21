using FistVR;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static RootMotion.FinalIK.VRIKCalibrator.CalibrationData;

namespace MuzzleScripts
{
    public class RepeatingFlintlockBarrels : FVRInteractiveObject
    {
        [Header("Repeating Flintlock Barrels Config")]
        public FlintlockWeapon Weapon;
        public Transform BarrelCluster;
        public bool RotatesClockwise = false;
        public FVRPhysicalObject.Axis Axis;
        public ActuationType Type;
        public float RotationRate = 4f;
        private float RotationCooldown = 0.5f;
        private FVRAlternateGrip _altGrip;
        private float m_curFlashPanLerp = 0f;
        private float m_tarFlashPanLerp = 0f;
        private bool isRotating = false;
        public enum ActuationType
        {
            Manual,
            Hammer
        }
        public override void Start()
        {
            base.Start();
            this._altGrip = Weapon.Foregrip.GetComponent<FVRAlternateGrip>();
        }
        //public void UpdateCluster()
        //{

        //    StartCoroutine(RotateCluster());
        //    if (this.RotatesClockwise)
        //    {
        //        this.Weapon.m_curFlashpan--;
        //        this.Weapon.m_curFlashpan = (int)Mathf.Repeat(Weapon.m_curFlashpan, Weapon.FlashPans.Count);
        //    }
        //    else
        //    {
        //        this.Weapon.m_curFlashpan = (int)Mathf.Repeat(Weapon.m_curFlashpan, Weapon.FlashPans.Count);
        //    }

        //    StopCoroutine(RotateCluster());

        //}
        //IEnumerator RotateCluster()
        //{
        //    int next_pan;
        //    if (this.RotatesClockwise)
        //    {
        //        next_pan = (int)Mathf.Repeat((Weapon.m_curFlashpan - 1), Weapon.FlashPans.Count);
        //    }
        //    else
        //    {
        //        next_pan = (int)Mathf.Repeat((Weapon.m_curFlashpan + 1), Weapon.FlashPans.Count);
        //    }
        //    var t = 0f;
        //    var start = GetLocalRotation(Weapon.m_curFlashpan);
        //    var target = GetLocalRotation(next_pan);
        //    while (t < 1)
        //    {
        //        t += Time.deltaTime * RotationRate;
        //        if (t > 1) t = 1;
        //        this.BarrelCluster.localRotation = Quaternion.Slerp(start, target, t);
        //        yield return null;
        //    }
        //}

        public Quaternion GetLocalRotation(int flashpan)
        {
            float num = (float)flashpan * (360f / (float)this.Weapon.FlashPans.Count);
            num = Mathf.Repeat(num, 360f);
            switch (Axis)
            {
                case FVRPhysicalObject.Axis.X:
                    return Quaternion.Euler(new Vector3(num, 0f, 0f));
                case FVRPhysicalObject.Axis.Y:
                    return Quaternion.Euler(new Vector3(0f, num, 0f));
                case FVRPhysicalObject.Axis.Z:
                    return Quaternion.Euler(new Vector3(0f, 0f, num));
                default:
                    return Quaternion.Euler(new Vector3(0f, 0f, num));
            }
        }


        public override void FVRUpdate()
        {
           base.FVRUpdate();
            if (this.RotationCooldown < 1f)
            {
                this.RotationCooldown += Time.deltaTime;
            }
            FVRViveHand hand = this._altGrip.m_hand;
            if (hand != null)
            {
                if (this.Weapon.HammerState != FlintlockWeapon.HState.Uncocked || this.isRotating == false)
                {
                    if (this.Type == ActuationType.Manual)
                    {
                        if (hand.IsInStreamlinedMode)
                        {
                            if (hand.Input.AXButtonDown)
                            {
                                this.m_tarFlashPanLerp = 1f;
                                this.Weapon.PlayAudioAsHandling(Weapon.AudEvent_HammerHalfCock, Weapon.Hammer.position);
                                this.isRotating = true;
                            }
                        }
                        else if (hand.Input.TouchpadDown)
                        {
                            this.m_tarFlashPanLerp = 1f;
                            this.Weapon.PlayAudioAsHandling(Weapon.AudEvent_HammerHalfCock, Weapon.Hammer.position);
                            this.isRotating = true;
                        }
                    }

                }
            }
            if (this.isRotating)
            {
                int next_pan;
                if (this.RotatesClockwise)
                {
                    next_pan = this.Weapon.m_curFlashpan - 1;
                    if (next_pan < 0)
                    {
                        next_pan = this.Weapon.FlashPans.Count - 1;
                    }
                }
                else
                {
                    next_pan = this.Weapon.m_curFlashpan + 1;
                    next_pan %= this.Weapon.FlashPans.Count;
                }
                this.m_curFlashPanLerp = Mathf.MoveTowards(this.m_curFlashPanLerp, this.m_tarFlashPanLerp, Time.deltaTime * this.RotationRate);
                var start = GetLocalRotation(this.Weapon.m_curFlashpan);
                var target = GetLocalRotation(next_pan);
                this.BarrelCluster.localRotation = Quaternion.Slerp(start, target, m_curFlashPanLerp);
                if (this.m_curFlashPanLerp > 0.99f)
                {
                    this.Weapon.m_curFlashpan = next_pan;
                    this.RotationCooldown = 0f;
                    this.m_curFlashPanLerp = 0f;
                    this.m_tarFlashPanLerp = 0f;
                    this.isRotating = false;
                }
                
            }

        }
    }
}