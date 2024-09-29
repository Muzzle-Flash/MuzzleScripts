using FistVR;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MuzzleScripts
{
    public class SingleActionRevolverRingTrigger : MonoBehaviour
    {
		public SingleActionRevolver Revolver;
		private bool IsRingPressed = false;
		private bool WasHammerCocked = false;
		private bool HasFired = false;
		public float TriggerResetThreshold = 0.01f;
		public Transform RingLever;
		public float Ring_Rot_Pressed;
		public float Ring_Rot_Released;
		private float Current_Ring_Rot;
		private static IntPtr _methodPointer;
		static SingleActionRevolverRingTrigger()
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
            On.FistVR.SingleActionRevolver.UpdateInteraction += SingleActionRevolver_UpdateInteraction;
            On.FistVR.SingleActionRevolver.UpdateTriggerHammer += SingleActionRevolver_UpdateTriggerHammer;
#endif
		}

        private void SingleActionRevolver_UpdateTriggerHammer(On.FistVR.SingleActionRevolver.orig_UpdateTriggerHammer orig, SingleActionRevolver self)
        {
            if (self == Revolver)
            {
				if (self.IsHeld && !self.m_isStateToggled && !self.m_isHammerCocked && !self.m_isHammerCocking && self.m_hand.OtherHand != null)
				{
					Vector3 velLinearWorld = self.m_hand.OtherHand.Input.VelLinearWorld;
					float num = Vector3.Distance(self.m_hand.OtherHand.PalmTransform.position, self.HammerFanDir.position);
					if (num < 0.15f && Vector3.Angle(velLinearWorld, self.HammerFanDir.forward) < 60f && velLinearWorld.magnitude > 1f)
					{
						self.CockHammer(10f);
						this.WasHammerCocked = true;

					}
				}
				if (self.m_isHammerCocking)
				{
					if (self.m_hammerCockLerp < 1f)
					{
						self.m_hammerCockLerp += Time.deltaTime * self.m_hammerCockSpeed;
					}
					else
                    {
						self.m_hammerCockLerp = 1f;
						self.m_isHammerCocking = false;
						self.m_isHammerCocked = true;
						if (this.IsRingPressed)
                        {
							self.CurChamber++;
							self.m_curChamberLerp = 0f;
							self.m_tarChamberLerp = 0f;
						}
					}
				}
				if (!self.m_isStateToggled)
				{
					self.Hammer.localEulerAngles = new Vector3(Mathf.Lerp(self.Hammer_Rot_Uncocked, self.Hammer_Rot_Cocked, self.m_hammerCockLerp), 0f, 0f);
				}
				else
				{
					self.Hammer.localEulerAngles = new Vector3(self.Hammer_Rot_Halfcocked, 0f, 0f);
				}
				if (self.LoadingGate != null)
				{
					if (!self.m_isStateToggled)
					{
						self.LoadingGate.localEulerAngles = new Vector3(0f, 0f, self.LoadingGate_Rot_Closed);
					}
					else
					{
						self.LoadingGate.localEulerAngles = new Vector3(0f, 0f, self.LoadingGate_Rot_Open);
					}
				}
				self.m_triggerFloat = 0f;
				if (self.m_hasTriggeredUpSinceBegin && !self.m_isSpinning && !self.m_isStateToggled)
				{
					self.m_triggerFloat = self.m_hand.Input.TriggerFloat;
				}
				if (!this.WasHammerCocked || this.IsRingPressed)
                {
					this.RingLever.localEulerAngles = new Vector3(Mathf.Lerp(this.Ring_Rot_Released, this.Ring_Rot_Pressed, self.m_triggerFloat), 0f, 0f);
                }
				else
                {
					self.Trigger.localEulerAngles = new Vector3(Mathf.Lerp(self.Trigger_Rot_Forward, self.Trigger_Rot_Rearward, self.m_triggerFloat), 0f, 0f);
				}

				if (self.m_triggerFloat > self.TriggerThreshold)
				{
					if (self.m_isHammerCocked)
                    {
						this.HasFired = true;
						self.DropHammer();
					}
					
				}
			}
			else
            {
				orig(self);
            }
        }

#if !DEBUG
        private void SingleActionRevolver_UpdateInteraction(On.FistVR.SingleActionRevolver.orig_UpdateInteraction orig, SingleActionRevolver self, FVRViveHand hand)
        {
			if (self == Revolver)
            {
				var action = (Action<FVRViveHand>)Activator.CreateInstance(typeof(Action<FVRViveHand>), self, _methodPointer);

				action(hand);
				self.m_isSpinning = false;
				if (!self.IsAltHeld)
				{
					if (!self.m_isStateToggled)
					{
						if (hand.Input.TouchpadPressed && !hand.IsInStreamlinedMode && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
						{
							self.m_isSpinning = true;
						}
						if (this.HasFired && this.WasHammerCocked && self.m_triggerFloat < this.TriggerResetThreshold)
						{
							this.IsRingPressed = false;
							this.WasHammerCocked = false;
							this.HasFired = false;
							self.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
						}
						if (!this.WasHammerCocked && !(self.m_isHammerCocked || self.m_isHammerCocking) && self.m_triggerFloat > this.TriggerResetThreshold)
                        {
							self.CockHammer(5f);
							self.TriggerThreshold = self.TriggerThreshold +1;
							this.IsRingPressed = true;
							this.WasHammerCocked = true;
						}
						if (this.IsRingPressed && self.m_triggerFloat < this.TriggerResetThreshold)
                        {
							this.IsRingPressed = false;
							self.TriggerThreshold = self.TriggerThreshold - 1;
							self.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
						}
						if (hand.IsInStreamlinedMode)
						{
							if (!this.IsRingPressed && hand.Input.AXButtonDown)
							{
								self.CockHammer(5f);
								this.WasHammerCocked = true;
							}
							if (hand.Input.BYButtonDown && self.StateToggles)
							{
								self.ToggleState();
								this.WasHammerCocked = false;
								self.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
							}
						}
						else if (hand.Input.TouchpadDown)
						{
							if (!this.IsRingPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
							{
								self.CockHammer(5f);
								this.WasHammerCocked = true;
							}
							else if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && self.StateToggles)
							{
								self.ToggleState();
								this.WasHammerCocked = false;
								self.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
							}
						}
					}
					else
					{
						if (hand.IsInStreamlinedMode)
						{
							if (hand.Input.AXButtonDown)
							{
								self.AdvanceCylinder();
							}
							if (hand.Input.BYButtonDown && self.StateToggles)
							{
								self.ToggleState();
								this.WasHammerCocked = false;
								self.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
							}
						}
						else if (hand.Input.TouchpadDown)
						{
							if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && self.StateToggles)
							{
								self.ToggleState();
								this.WasHammerCocked = false;
								self.PlayAudioEvent(FirearmAudioEventType.BreachClose, 1f);
							}
							else if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
							{
								self.AdvanceCylinder();
							}
						}
						if (hand.Input.TriggerDown)
						{
							self.EjectPrevCylinder();
						}
					}
				}
				self.UpdateTriggerHammer();
				self.UpdateCylinderRot();
				if (!self.IsHeld)
				{
					self.m_isSpinning = false;
				}
			}
			else
            {
				orig(self, hand);
			}
		}
#endif

		public void Unhook()
		{
#if !DEBUG
			On.FistVR.SingleActionRevolver.UpdateInteraction -= SingleActionRevolver_UpdateInteraction;
			On.FistVR.SingleActionRevolver.UpdateTriggerHammer -= SingleActionRevolver_UpdateTriggerHammer;
#endif
		}
	}
}