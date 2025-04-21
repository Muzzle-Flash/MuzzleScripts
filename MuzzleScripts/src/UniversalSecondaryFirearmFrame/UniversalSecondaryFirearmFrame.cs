using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System;

namespace MuzzleScripts
{
    public class UniversalSecondaryFirearmFrame : FVRFireArm
    {
        [Header("Universal Secondary Firearm Frame")]
        public Transform Trigger;
        public FVRPhysicalObject.InterpStyle Trigger_InterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        public FVRPhysicalObject.Axis Trigger_Axis;
        public float Trigger_ForwardValue;
        public float Trigger_RearwardValue;
        public float TriggerResetThreshold = 0.4f;
        public float TriggerFiringThreshold = 0.95f;
        public AudioEvent TriggerResetAudio;
        private float m_triggerFloat = 0f;
        private bool m_hasTriggered = false;

        [Header("Mode Selector Config")]
        public bool UsesModeSelector;
        public Transform ModeSelector;
        public FVRPhysicalObject.InterpStyle Selector_InterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        public FVRPhysicalObject.Axis Selector_Axis = FVRPhysicalObject.Axis.X;
        public UniversalSecondaryFirearmFrame.SelectorMode[] Selector_Modes;
        public AudioEvent SelectorAudio;
        private int m_selectorMode;

        [Header("Full Auto Switch Config")]
        public bool UsesFullAutoSwitch;
        public Transform Switch;
        public FVRPhysicalObject.InterpStyle Switch_InterpStyle = FVRPhysicalObject.InterpStyle.Translate;
        public FVRPhysicalObject.Axis Switch_Axis = FVRPhysicalObject.Axis.Y;
        public float Switch_OnPosition;
        public float Switch_OffPosition;
        public float cycleDelay = 0.2f;
        public bool doesCycle = false;


        private int m_currentWeaponIndex = 0;
        private int m_lastNumberOfAttachableWeapons = 0;
        private float m_cycleCooldown = 0.2f;

        private List<AttachableFirearm> _attachableFirearms = new List<AttachableFirearm>();

        public enum SelectorModeType
        {
            Safe,
            RoundRobin,
            AllAtOnce,
            //RapidCycle
        }

        [Serializable]
        public class SelectorMode
        {
            public float SelectorPosition;
            public UniversalSecondaryFirearmFrame.SelectorModeType ModeType;
        }

        public override void Awake()
        {
            base.Awake();
            Hook();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Unhook();
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            GetAllAttachableWeaponsOnFirearm();
            base.BeginInteraction(hand);
        }

        public override void EndInteraction(FVRViveHand hand)
        {
            base.EndInteraction(hand);
            _attachableFirearms.Clear();
            m_triggerFloat = 0f;
        }

        private void GetAllAttachableWeaponsOnFirearm()
        {
            _attachableFirearms.Clear();
            foreach (var attachment in this.AttachmentsList)
            {
                AttachableFirearmPhysicalObject attachableFirearmPhysicalObject = attachment as AttachableFirearmPhysicalObject;
                if (attachableFirearmPhysicalObject != null)
                {
                    _attachableFirearms.Add(attachableFirearmPhysicalObject.FA);
                }
                if (m_lastNumberOfAttachableWeapons != _attachableFirearms.Count) m_currentWeaponIndex = 0;
                m_lastNumberOfAttachableWeapons = _attachableFirearms.Count;
            }
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            if (m_currentWeaponIndex >= _attachableFirearms.Count) m_currentWeaponIndex = 0;
            if (this.m_hasTriggeredUpSinceBegin)
            {
                m_triggerFloat = hand.Input.TriggerFloat;
            }
            else
            {
                m_triggerFloat = 0f;
            }

            if (hand.IsInStreamlinedMode)
            {
                if (hand.Input.BYButtonDown)
                {
                    if (this.UsesModeSelector) this.ToggleSelectorMode();
                }
                if (hand.Input.AXButtonDown)
                {
                    if (this.UsesFullAutoSwitch) this.ToggleFullAutoSwitch();
                }
            }
            else
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f)
                {
                    if (this.UsesModeSelector) this.ToggleSelectorMode();
                }
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
                {
                    if (this.UsesFullAutoSwitch) this.ToggleFullAutoSwitch();
                }
            }


            UniversalSecondaryFirearmFrame.SelectorModeType modeType = this.Selector_Modes[m_selectorMode].ModeType;
            if (this._attachableFirearms.Count > 0 && !(hand.Input.TouchpadDown || hand.Input.AXButtonDown || hand.Input.BYButtonDown))
            {
                if (modeType != SelectorModeType.Safe && this.m_hasTriggeredUpSinceBegin)
                {
                    if (modeType == SelectorModeType.AllAtOnce)
                    {
                        if (this.doesCycle)
                        {
                            if (hand.Input.TriggerFloat > 0f)
                            {
                                if (!this.m_hasTriggered) this.m_cycleCooldown = 0f;
                                this.m_hasTriggered = true;
                                foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                                {
                                    attachableFirearm.ProcessInput(hand, true, this);
                                }
                            }
                            if (hand.Input.TriggerFloat == 1f)
                            {
                                foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                                {
                                    attachableFirearm.ProcessInput(hand, true, this);
                                }
                                if (this.m_cycleCooldown >= this.cycleDelay)
                                {
                                    hand.Input.TriggerFloat = 0f;
                                    hand.Input.TriggerDown = false;
                                    foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                                    {
                                        attachableFirearm.ProcessInput(hand, true, this);
                                    }
                                    this.m_hasTriggered = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                            {
                                attachableFirearm.ProcessInput(hand, true, this);
                            }
                        }
                    }
                    else if (modeType == SelectorModeType.RoundRobin)
                    {
                        if (this.doesCycle)
                        {
                            if (hand.Input.TriggerFloat > 0f)
                            {
                                if (!this.m_hasTriggered) this.m_cycleCooldown = 0f;
                                this.m_hasTriggered = true;
                                this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                            }
                            if (hand.Input.TriggerFloat == 1f)
                            {
                                this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                                if (this.m_cycleCooldown >= this.cycleDelay)
                                {
                                    hand.Input.TriggerFloat = 0f;
                                    hand.Input.TriggerDown = false;
                                    this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                                    this.m_currentWeaponIndex++;
                                    if (this.m_currentWeaponIndex > this._attachableFirearms.Count) this.m_currentWeaponIndex = 0;
                                    this.m_hasTriggered = false;
                                }
                            }
                            else if (hand.Input.TriggerFloat == 0f && this.m_hasTriggered)
                            {
                                foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                                {
                                    attachableFirearm.ProcessInput(hand, true, this);
                                }
                                this.m_hasTriggered = false;
                                base.PlayAudioAsHandling(this.TriggerResetAudio, this.transform.position);
                                this.m_currentWeaponIndex++;
                                if (this.m_currentWeaponIndex > this._attachableFirearms.Count) this.m_currentWeaponIndex = 0;
                            }
                        }
                        else
                        {
                            if (hand.Input.TriggerFloat > 0f)
                            {
                                this.m_hasTriggered = true;
                                this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                            }
                            else if (hand.Input.TriggerFloat == 0f && this.m_hasTriggered)
                            {
                                this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                                this.m_hasTriggered = false;
                                base.PlayAudioAsHandling(this.TriggerResetAudio, this.transform.position);
                                this.m_currentWeaponIndex++;
                                if (this.m_currentWeaponIndex > this._attachableFirearms.Count) this.m_currentWeaponIndex = 0;
                            }
                        }
                    }

                    //else if (modeType == SelectorModeType.RapidCycle)
                    //{
                    //    if (hand.Input.TriggerFloat > 0f)
                    //    {
                    //        this.m_hasTriggered = true;
                    //        this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                    //    }
                    //    if (hand.Input.TriggerFloat == 1f)
                    //    {
                    //        this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                    //        if (this.m_cycleCooldown >= this.cycleDelay)
                    //        {
                    //            hand.Input.TriggerFloat = 0.1f;
                    //            this._attachableFirearms[m_currentWeaponIndex].ProcessInput(hand, true, this);
                    //            this.m_currentWeaponIndex++;
                    //            if (this.m_currentWeaponIndex > this._attachableFirearms.Count) this.m_currentWeaponIndex = 0;
                    //            this.m_cycleCooldown = 0f;
                    //        }
                    //    }
                    //    else if (hand.Input.TriggerFloat == 0f && this.m_hasTriggered)
                    //    {
                    //        foreach (AttachableFirearm attachableFirearm in this._attachableFirearms)
                    //        {
                    //            attachableFirearm.ProcessInput(hand, true, this);
                    //        }
                    //        this.m_hasTriggered = false;
                    //        base.PlayAudioAsHandling(this.TriggerResetAudio, this.transform.position);
                    //        this.m_currentWeaponIndex++;
                    //        if (this.m_currentWeaponIndex > this._attachableFirearms.Count) this.m_currentWeaponIndex = 0;
                    //    }
                    //}
                }
            }
        }

        public void ToggleSelectorMode()
        {
            if (this.Selector_Modes.Length > 1)
            {
                this.m_selectorMode++;
                if (this.m_selectorMode >= this.Selector_Modes.Length) this.m_selectorMode = 0;
                UniversalSecondaryFirearmFrame.SelectorMode selectorMode = this.Selector_Modes[m_selectorMode];
                base.PlayAudioAsHandling(SelectorAudio, this.transform.position);
                if (this.ModeSelector != null)
                {
                    base.SetAnimatedComponent(this.ModeSelector, selectorMode.SelectorPosition, this.Selector_InterpStyle, this.Selector_Axis);
                }
            }
        }

        public void ToggleFullAutoSwitch()
        {
            this.doesCycle = !this.doesCycle;
            base.PlayAudioAsHandling(SelectorAudio, this.transform.position);
            if (this.doesCycle == true)
            {
                base.SetAnimatedComponent(this.Switch, this.Switch_OnPosition, this.Switch_InterpStyle, this.Switch_Axis);
            }
            else
            {
                base.SetAnimatedComponent(this.Switch, this.Switch_OffPosition, this.Switch_InterpStyle, this.Switch_Axis);
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.m_cycleCooldown < 1f) this.m_cycleCooldown += Time.deltaTime;
            base.SetAnimatedComponent(this.Trigger, Mathf.Lerp(this.Trigger_ForwardValue, this.Trigger_RearwardValue, this.m_triggerFloat), this.Trigger_InterpStyle, this.Trigger_Axis);
        }

        public void Hook()
        {
#if!DEBUG
            On.FistVR.FVRPhysicalObject.RegisterAttachment += FVRPhysicalObject_RegisterAttachment;
            On.FistVR.FVRPhysicalObject.DeRegisterAttachment += FVRPhysicalObject_DeRegisterAttachment;
#endif
        }

        public void Unhook()
        {
#if!DEBUG
            On.FistVR.FVRPhysicalObject.RegisterAttachment -= FVRPhysicalObject_RegisterAttachment;
            On.FistVR.FVRPhysicalObject.DeRegisterAttachment -= FVRPhysicalObject_DeRegisterAttachment;
#endif
        }
#if !DEBUG
        private void FVRPhysicalObject_DeRegisterAttachment(On.FistVR.FVRPhysicalObject.orig_DeRegisterAttachment orig, FVRPhysicalObject self, FVRFireArmAttachment attachment)
        {
            if (self == this)
            {
                if (this.AttachmentsHash.Remove(attachment))
                {
                    this.AttachmentsList.Remove(attachment);
                    this.ResetClampCOM();
                }
                GetAllAttachableWeaponsOnFirearm();
            }
        }

        private void FVRPhysicalObject_RegisterAttachment(On.FistVR.FVRPhysicalObject.orig_RegisterAttachment orig, FVRPhysicalObject self, FVRFireArmAttachment attachment)
        {
            if (self == this)
            {
                if (this.AttachmentsHash.Add(attachment))
                {
                    this.AttachmentsList.Add(attachment);
                    this.ResetClampCOM();
                }
                GetAllAttachableWeaponsOnFirearm();
            }
        }
#endif
    }
}
