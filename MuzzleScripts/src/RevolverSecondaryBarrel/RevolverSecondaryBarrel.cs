using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace MuzzleScripts
{
    public class RevolverSecondaryBarrel : MonoBehaviour
    {
        [Header("Revolver Secondary Barrel")]
        public Revolver revolver;
        public FVRFireArmChamber Chamber;
        public Transform Muzzle;
        public FVRFireArmRecoilProfile OverrideRecoilProfile;

        [HideInInspector]
        public bool IsSecondaryHammerEngaged;
        [HideInInspector]
        public bool IsSecondaryHammerCocked;
        [HideInInspector]
        public bool IsSecondaryBarrelClosed;

        public void Awake()
        {
            this.RegisterChambers();
            Hook();
        }
        public void RegisterChambers()
        {
            this.revolver.FChambers.Clear();
            foreach (FVRFireArmChamber chamber in this.revolver.Chambers)
            {
                this.revolver.FChambers.Add(chamber);
            }
            this.revolver.FChambers.Add(this.Chamber);
        }

        public void OnDestroy()
        {
            Unhook();
        }

        public void Hook()
        {
#if !DEBUG
            On.FistVR.Revolver.UpdateTriggerHammer += Revolver_UpdateTriggerHammer;
            On.FistVR.Revolver.SetLoadedChambers += Revolver_SetLoadedChambers;
            On.FistVR.Revolver.GetChamberRoundList += Revolver_GetChamberRoundList;
#endif
        }

        public void Unhook()
        {
#if !DEBUG
            On.FistVR.Revolver.UpdateTriggerHammer -= Revolver_UpdateTriggerHammer;
            On.FistVR.Revolver.SetLoadedChambers -= Revolver_SetLoadedChambers;
            On.FistVR.Revolver.GetChamberRoundList -= Revolver_GetChamberRoundList;
#endif
        }
#if !DEBUG
        private List<FireArmRoundClass> Revolver_GetChamberRoundList(On.FistVR.Revolver.orig_GetChamberRoundList orig, Revolver self)
        {
            if (self == revolver)
            {
                bool flag = false;
                List<FireArmRoundClass> list = new List<FireArmRoundClass>();
                for (int i = 0; i < self.FChambers.Count; i++)
                {
                    if (self.FChambers[i].IsFull)
                    {
                        list.Add(self.FChambers[i].GetRound().RoundClass);
                        flag = true;
                    }
                }
                List<FireArmRoundClass> result;
                if (flag)
                {
                    result = list;
                }
                else
                {
                    result = null;
                }
                return result;
            }
            else
            {
                return orig.Invoke(self);
            }
        }
        private void Revolver_SetLoadedChambers(On.FistVR.Revolver.orig_SetLoadedChambers orig, Revolver self, List<FireArmRoundClass> rounds)
        {
            if (self == revolver)
            {
                if (rounds.Count > 0)
                {
                    for (int i = 0; i < self.FChambers.Count; i++)
                    {
                        if (i < rounds.Count)
                        {
                            self.FChambers[i].Autochamber(rounds[i]);
                        }
                    }
                }
            }
            else
            {
                orig.Invoke(self, rounds);
            }
            
        }
        private void Revolver_UpdateTriggerHammer(On.FistVR.Revolver.orig_UpdateTriggerHammer orig, Revolver self)
        {
            if (self == revolver)
            {
                if (self.m_hasTriggeredUpSinceBegin && !self.m_isSpinning && !self.IsAltHeld && self.isCylinderArmLocked)
                {
                    self.m_tarTriggerFloat = self.m_hand.Input.TriggerFloat;
                    self.m_tarRealTriggerFloat = self.m_hand.Input.TriggerFloat;
                }
                else
                {
                    self.m_tarTriggerFloat = 0f;
                    self.m_tarRealTriggerFloat = 0f;
                }
                if (self.m_isHammerLocked)
                {
                    self.m_tarTriggerFloat += 0.8f;
                    self.m_triggerCurrentRot = Mathf.Lerp(self.m_triggerForwardRot, self.m_triggerBackwardRot, self.m_curTriggerFloat);
                }
                else
                {
                    self.m_triggerCurrentRot = Mathf.Lerp(self.m_triggerForwardRot, self.m_triggerBackwardRot, self.m_curTriggerFloat);
                }
                self.m_curTriggerFloat = Mathf.MoveTowards(self.m_curTriggerFloat, self.m_tarTriggerFloat, Time.deltaTime * 14f);
                self.m_curRealTriggerFloat = Mathf.MoveTowards(self.m_curRealTriggerFloat, self.m_tarRealTriggerFloat, Time.deltaTime * 14f);
                if (Mathf.Abs(self.m_triggerCurrentRot - self.lastTriggerRot) > 0.01f)
                {
                    if (self.Trigger != null)
                    {
                        self.Trigger.localEulerAngles = new Vector3(self.m_triggerCurrentRot, 0f, 0f);
                    }
                    for (int i = 0; i < self.TPieces.Count; i++)
                    {
                        self.SetAnimatedComponent(self.TPieces[i].TPiece, Mathf.Lerp(self.TPieces[i].TRange.x, self.TPieces[i].TRange.y, self.m_curTriggerFloat), self.TPieces[i].TInterp, self.TPieces[i].TAxis);
                    }
                }
                self.lastTriggerRot = self.m_triggerCurrentRot;
                if (self.m_shouldRecock)
                {
                    self.m_shouldRecock = false;
                    self.InitiateRecock();
                }
                if (self.CanFan && self.IsHeld && !self.m_isHammerLocked && self.m_recockingState == Revolver.RecockingState.Forward && self.m_hand.OtherHand != null)
                {
                    Vector3 velLinearWorld = self.m_hand.OtherHand.Input.VelLinearWorld;
                    float num = Vector3.Distance(self.m_hand.OtherHand.PalmTransform.position, self.HammerFanDir.position);
                    if (num < 0.15f && Vector3.Angle(velLinearWorld, self.HammerFanDir.forward) < 60f && velLinearWorld.magnitude > 1f)
                    {
                        self.InitiateRecock();
                        self.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
                    }
                }
                if (!this.IsSecondaryHammerEngaged)
                {
                    if (!self.m_hasTriggerCycled || (!self.IsDoubleActionTrigger && !self.DoesFiringRecock))
                    {
                        bool flag = false;
                        if (self.m_recockingState != Revolver.RecockingState.Forward)
                        {
                            flag = true;
                        }
                        if (!flag && self.m_curTriggerFloat >= 0.95f && (self.m_isHammerLocked || self.IsDoubleActionTrigger) && !self.m_hand.Input.TouchpadPressed)
                        {
                            if (self.m_isCylinderArmLocked)
                            {
                                self.m_hasTriggerCycled = true;
                                self.m_isHammerLocked = false;
                                if (self.IsCylinderRotClockwise)
                                {
                                    self.CurChamber++;
                                }
                                else
                                {
                                    int curChamber = self.CurChamber - 1;
                                    self.CurChamber = curChamber;
                                }
                                self.m_curChamberLerp = 0f;
                                self.m_tarChamberLerp = 0f;
                                self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                                int num2 = self.CurChamber + self.ChamberOffset;
                                if (num2 >= self.Cylinder.numChambers)
                                {
                                    num2 -= self.Cylinder.numChambers;
                                }
                                if (self.Chambers[num2].IsFull && !self.Chambers[num2].IsSpent)
                                {
                                    self.Chambers[num2].Fire();
                                    self.Fire();
                                    if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
                                    {
                                        self.Chambers[num2].IsSpent = false;
                                        self.Chambers[num2].UpdateProxyDisplay();
                                    }
                                    if (self.DoesFiringRecock)
                                    {
                                        self.m_shouldRecock = true;
                                    }
                                }
                            }
                        }
                        else if ((self.m_curTriggerFloat <= 0.14f || !self.IsDoubleActionTrigger) && !self.m_isHammerLocked && self.CanManuallyCockHammer)
                        {
                            bool flag2 = false;
                            if (self.DoesFiringRecock && self.m_recockingState != Revolver.RecockingState.Forward)
                            {
                                flag2 = true;
                            }
                            if (self.DoesTriggerBlockHammer && self.m_curTriggerFloat > 0.14f)
                            {
                                flag2 = true;
                            }
                            if (!self.IsAltHeld && !flag2)
                            {
                                if (self.m_hand.IsInStreamlinedMode)
                                {
                                    if (self.m_hand.Input.AXButtonDown)
                                    {
                                        self.m_isHammerLocked = true;
                                        self.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
                                    }
                                }
                                else if (self.m_hand.Input.TouchpadDown && Vector2.Angle(self.TouchPadAxes, Vector2.down) < 45f)
                                {
                                    self.m_isHammerLocked = true;
                                    self.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
                                }
                            }
                        }
                    }
                    else if (self.m_hasTriggerCycled && self.m_curRealTriggerFloat <= 0.08f)
                    {
                        self.m_hasTriggerCycled = false;
                        self.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
                    }
                }
                else if (this.IsSecondaryHammerEngaged)
                {
                    if (self.m_curTriggerFloat >= 0.95f && this.IsSecondaryHammerCocked && !self.m_hand.Input.TouchpadPressed)
                    {
                        self.m_hasTriggerCycled = true;
                        this.IsSecondaryHammerCocked = false;
                        self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                        if (this.IsSecondaryBarrelClosed)
                        {
                            if (this.Chamber.Fire())
                            {
                                FVRFireArm firearm = self as FVRFireArm;
                                firearm.Fire(this.Chamber, this.Muzzle, true, 1f, -1f);
                                if (GM.CurrentSceneSettings.IsSceneLowLight)
                                {
                                    if (!self.IsSuppressed())
                                    {
                                        FXM.InitiateMuzzleFlash(this.Muzzle.position, this.Muzzle.forward, 0.65f, new Color(1f, 0.9f, 0.77f), 1f);
                                    }
                                }
                                for (int i = 0; i < self.m_muzzleSystems.Count; i++)
                                {
                                    self.m_muzzleSystems[i].PSystem.transform.position = this.Muzzle.position;
                                    self.m_muzzleSystems[i].PSystem.Emit(self.m_muzzleSystems[i].NumParticlesPerShot);
                                }
                                for (int j = 0; j < self.GasOutEffects.Length; j++)
                                {
                                    self.GasOutEffects[j].AddGas(self.IsSuppressed());
                                }
                                self.FireMuzzleSmoke();
                                if (this.Chamber.GetRound().IsHighPressure)
                                {
                                    self.Recoil(self.IsTwoHandStabilized(), self.AltGrip != null, self.IsShoulderStabilized(), this.OverrideRecoilProfile, 1f);
                                }
                                self.PlayAudioGunShot(this.Chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), self.ShotLoudnessMult);
                                if (this.Chamber.GetRound().IsCaseless)
                                {
                                    this.Chamber.SetRound(null, false);
                                }
                                if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
                                {
                                    this.Chamber.IsSpent = false;
                                    this.Chamber.UpdateProxyDisplay();
                                }
                            }
                        }
                    }
                }
                if (!self.isChiappaHammer)
                {
                    if (self.m_hasTriggerCycled || !self.IsDoubleActionTrigger)
                    {
                        if (self.m_isHammerLocked)
                        {
                            self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 10f);
                        }
                        else
                        {
                            self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerForwardRot, Time.deltaTime * 30f);
                        }
                    }
                    else if (self.m_isHammerLocked)
                    {
                        self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 10f);
                    }
                    else if (!this.IsSecondaryHammerEngaged)
                    {
                        self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerForwardRot, self.m_hammerBackwardRot, self.m_curTriggerFloat);
                    }
                }
                if (self.isChiappaHammer)
                {
                    bool flag3 = false;
                    if (self.m_hand.IsInStreamlinedMode && self.m_hand.Input.AXButtonPressed)
                    {
                        flag3 = true;
                    }
                    else if (Vector2.Angle(self.m_hand.Input.TouchpadAxes, Vector2.down) < 45f && self.m_hand.Input.TouchpadPressed)
                    {
                        flag3 = true;
                    }
                    if (self.m_curTriggerFloat <= 0.02f && !self.IsAltHeld && flag3)
                    {
                        self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 15f);
                    }
                    else
                    {
                        self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerForwardRot, Time.deltaTime * 6f);
                    }
                }
                if (self.Hammer != null)
                {
                    self.Hammer.localEulerAngles = new Vector3(self.m_hammerCurrentRot, 0f, 0f);
                }
            }
            else
            {
                orig.Invoke(self);
            }
        }
#endif
    }
}
