using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;
using System.Reflection;

namespace MuzzleScripts
{
    public class FlareGunPercussionCap : MonoBehaviour
    {
        public Flaregun flaregun;
        public FVRFireArmChamber CapNipple;
        private static IntPtr _methodPointer;

        static FlareGunPercussionCap()
        {
            MethodInfo _methodInfo = typeof(FVRFireArm).GetMethod(nameof(FVRFireArm.UpdateInteraction), BindingFlags.Public | BindingFlags.Instance);
            _methodPointer = _methodInfo.MethodHandle.GetFunctionPointer();
        }

        public void Awake()
        {
            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        public void Hook()
        {
#if !DEBUG
            On.FistVR.Flaregun.UpdateInteraction += Flaregun_UpdateInteraction;
#endif
        }

        private void Unhook()
        {
#if !DEBUG
            On.FistVR.Flaregun.UpdateInteraction -= Flaregun_UpdateInteraction;
#endif
        }
#if !DEBUG
        private void Flaregun_UpdateInteraction(On.FistVR.Flaregun.orig_UpdateInteraction orig, Flaregun self, FVRViveHand hand)
        {
            if (self == flaregun)
            {
                var action = (Action<FVRViveHand>)Activator.CreateInstance(typeof(Action<FVRViveHand>), self, _methodPointer);
                action(hand);
                float num = 0f;
                FVRPhysicalObject.Axis hingeAxis = self.HingeAxis;
                if (hingeAxis != FVRPhysicalObject.Axis.X)
                {
                    if (hingeAxis != FVRPhysicalObject.Axis.Y)
                    {
                        if (hingeAxis == FVRPhysicalObject.Axis.Z)
                        {
                            num = self.transform.InverseTransformDirection(hand.Input.VelAngularWorld).z;
                        }
                    }
                    else
                    {
                        num = self.transform.InverseTransformDirection(hand.Input.VelAngularWorld).y;
                    }
                }
                else
                {
                    num = self.transform.InverseTransformDirection(hand.Input.VelAngularWorld).x;
                }
                if (num > 15f && self.CanUnlatch && !self.m_isHammerCocked && self.CanFlick)
                {
                    self.Unlatch();
                }
                else if (num < -15f && self.CanUnlatch && self.CanFlick)
                {
                    self.Latch();
                }
                if (!self.IsAltHeld)
                {
                    if (hand.IsInStreamlinedMode)
                    {
                        if (hand.Input.BYButtonDown && self.CanUnlatch)
                        {
                            self.ToggleLatchState();
                        }
                        if (hand.Input.AXButtonDown)
                        {
                            self.CockHammer();
                        }
                    }
                    else if (hand.Input.TouchpadDown)
                    {
                        Vector2 touchpadAxes = hand.Input.TouchpadAxes;
                        if (touchpadAxes.magnitude > 0.2f && Vector2.Angle(touchpadAxes, Vector2.down) < 45f && self.CanCockHammer)
                        {
                            self.CockHammer();
                        }
                        else if (touchpadAxes.magnitude > 0.2f && (Vector2.Angle(touchpadAxes, Vector2.left) < 45f || Vector2.Angle(touchpadAxes, Vector2.right) < 45f) && self.CanUnlatch)
                        {
                            self.ToggleLatchState();
                        }
                    }
                }
                if (self.m_isDestroyed)
                {
                    return;
                }
                if (self.m_hasTriggeredUpSinceBegin && !self.IsAltHeld)
                {
                    self.TriggerFloat = hand.Input.TriggerFloat;
                }
                else
                {
                    self.TriggerFloat = 0f;
                }
                float x = Mathf.Lerp(self.TriggerForwardBackRots.x, self.TriggerForwardBackRots.y, self.TriggerFloat);
                self.Trigger.localEulerAngles = new Vector3(x, 0f, 0f);
                if (self.TriggerFloat > 0.7f)
                {
                    if (self.m_isTriggerReset && self.m_isHammerCocked)
                    {
                        self.m_isTriggerReset = false;
                        self.SetHammerCocked(false);
                        if (self.Hammer != null)
                        {
                            self.SetAnimatedComponent(self.Hammer, self.HammerMinRot, self.HammerInterp, self.HammerAxis);
                        }
                        self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                        if (this.CapNipple.Fire())
                        {
                            self.Fire();
                        }
                    }
                }
                else if (self.TriggerFloat < 0.2f && !self.m_isTriggerReset)
                {
                    self.m_isTriggerReset = true;
                }
            }
            else
            {
                orig(self, hand);
            }
        }
    #endif
    }
}