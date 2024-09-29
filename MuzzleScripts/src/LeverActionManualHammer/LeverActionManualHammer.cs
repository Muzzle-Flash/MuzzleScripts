using FistVR;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MuzzleScripts
{
    public class LeverActionManualHammer : MonoBehaviour
    {
        public LeverActionFirearm LeverAction;
		private static IntPtr _methodPointer;
		static LeverActionManualHammer()
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

        public void Update()
        {
            FVRViveHand hand = LeverAction.m_hand;
            if (hand != null)
            {
                if (hand.IsInStreamlinedMode)
                {
                    if (hand.Input.AXButtonDown)
                    {
                        CockHammer();
                    }
                }
                else
                {
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
                    {
                        CockHammer();
                    }
                }

            }
        }

        private void CockHammer()
        {
            if (LeverAction.m_isHammerCocked && LeverAction.m_isHammerCocked2)
            {
                return;
            }
            else
            {
				LeverAction.m_isHammerCocked = true;
				if (LeverAction.UsesSecondChamber)
				{
					LeverAction.m_isHammerCocked2 = true;
				}
				LeverAction.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
			}
            this.LeverAction.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
        }
        public void Hook()
        {
#if!DEBUG
            On.FistVR.LeverActionFirearm.UpdateLever += LeverActionFirearm_UpdateLever;
            On.FistVR.LeverActionFirearm.UpdateInteraction += LeverActionFirearm_UpdateInteraction;
#endif
		}

#if !DEBUG
		private void LeverActionFirearm_UpdateInteraction(On.FistVR.LeverActionFirearm.orig_UpdateInteraction orig, LeverActionFirearm self, FVRViveHand hand)
        {
            if (self == LeverAction)
            {
				var action = (Action<FVRViveHand>)Activator.CreateInstance(typeof(Action<FVRViveHand>), self, _methodPointer);

				action(hand);
				self.UpdateLever();
				self.Trigger.localEulerAngles = new Vector3(Mathf.Lerp(self.TriggerRotRange.x, self.TriggerRotRange.y, hand.Input.TriggerFloat), 0f, 0f);
				if (hand.Input.TriggerDown && !self.IsAltHeld && self.m_curLeverPos == LeverActionFirearm.ZPos.Rear && self.m_hasTriggeredUpSinceBegin && (self.m_isHammerCocked || self.m_isHammerCocked2))
				{
					self.Fire();
				}
				if (self.Hammer != null)
				{
					if (self.m_isHammerCocked)
					{
						self.Hammer.localEulerAngles = new Vector3(self.HammerAngleRange.x, 0f, 0f);
					}
					else
					{
						self.Hammer.localEulerAngles = new Vector3(self.HammerAngleRange.y, 0f, 0f);
					}
				}
            }
			else
			{
				orig(self, hand);
			}
		}
#endif



#if !DEBUG
		private void LeverActionFirearm_UpdateLever(On.FistVR.LeverActionFirearm.orig_UpdateLever orig, LeverActionFirearm self)
        {
            if (self == LeverAction)
            {
				bool flag = false;
				bool flag2 = false;
				if (self.IsHeld)
				{
					if (self.m_hand.IsInStreamlinedMode)
					{
						flag = self.m_hand.Input.BYButtonPressed;
						flag2 = self.m_hand.Input.BYButtonUp;
					}
					else
					{
						flag = self.m_hand.Input.TouchpadPressed;
						flag2 = self.m_hand.Input.TouchpadUp;
					}
				}
				self.m_isLeverReleasePressed = false;
				bool flag3 = false;
				if (!self.IsAltHeld && self.ForeGrip.m_hand != null)
				{
					if (self.ForeGrip.m_hand.Input.TriggerPressed && self.ForeGrip.m_hasTriggeredUpSinceBegin && GM.Options.ControlOptions.LongGunSnipingAssist != ControlOptions.LongGunSnipingAssistMode.TriggerHeld)
					{
						flag3 = true;
					}
					self.m_isLeverReleasePressed = true;
					self.curDistanceBetweenGrips = Vector3.Distance(self.m_hand.PalmTransform.position, self.AltGrip.m_hand.PalmTransform.position);
					if (self.lastDistanceBetweenGrips < 0f)
					{
						self.lastDistanceBetweenGrips = self.curDistanceBetweenGrips;
					}
				}
				else
				{
					self.lastDistanceBetweenGrips = -1f;
				}
				self.m_isSpinning = false;
				if (!self.IsAltHeld && self.CanSpin && flag)
				{
					self.m_isSpinning = true;
				}
				bool flag4 = false;
				if ((self.m_isHammerCocked || self.m_isHammerCocked2) && !self.m_isSpinning && self.m_curLeverPos == LeverActionFirearm.ZPos.Rear)
				{
					flag4 = true;
				}
				if (flag3)
				{
					flag4 = false;
				}
				if (self.AltGrip == null && !self.IsAltHeld)
				{
					flag4 = true;
				}
				if (flag4 && self.useLinearRacking)
				{
					self.SetBaseHandAngle(self.m_hand);
				}
				self.m_wasLeverLocked = flag4;
				if (flag2)
				{
					self.m_tarLeverRot = 0f;
					self.PoseSpinHolder.localPosition = self.m_baseSpinPosition;
					self.lastDistanceBetweenGrips = self.curDistanceBetweenGrips;
					self.m_rackingDisplacement = 0f;
				}
				else if (self.m_isLeverReleasePressed && !flag4)
				{
					if (self.useLinearRacking)
					{
						self.curDistanceBetweenGrips = Vector3.Distance(self.m_hand.PalmTransform.position, self.AltGrip.m_hand.PalmTransform.position);
						if (self.curDistanceBetweenGrips < self.lastDistanceBetweenGrips)
						{
							float num = self.lastDistanceBetweenGrips - self.curDistanceBetweenGrips;
							self.m_rackingDisplacement += num;
						}
						else
						{
							float num = self.curDistanceBetweenGrips - self.lastDistanceBetweenGrips;
							self.m_rackingDisplacement -= num;
						}
						self.m_rackingDisplacement = Mathf.Clamp(self.m_rackingDisplacement, 0f, 0.04f);
						if (self.m_rackingDisplacement < 0.005f)
						{
							self.m_rackingDisplacement = 0f;
						}
						if (self.m_rackingDisplacement > 0.035f)
						{
							self.m_rackingDisplacement = 0.04f;
						}
						self.PoseSpinHolder.localPosition = self.m_baseSpinPosition + Vector3.forward * self.m_rackingDisplacement * 2f;
						self.m_tarLeverRot = Mathf.Lerp(self.LeverAngleRange.y, self.LeverAngleRange.x, self.m_rackingDisplacement * 25f);
						self.lastDistanceBetweenGrips = self.curDistanceBetweenGrips;
					}
					else
					{
						Vector3 normalized = Vector3.ProjectOnPlane(self.m_hand.PoseOverride.forward, self.LeverRoot.right).normalized;
						Vector3 forward = self.LeverRoot.forward;
						float num2 = Mathf.Atan2(Vector3.Dot(self.LeverRoot.right, Vector3.Cross(forward, normalized)), Vector3.Dot(forward, normalized)) * 57.29578f;
						num2 -= self.BaseAngleOffset;
						num2 *= 3f;
						num2 = Mathf.Clamp(num2, self.LeverAngleRange.x, self.LeverAngleRange.y);
						self.m_tarLeverRot = num2;
					}
				}
				else if (self.m_isSpinning)
				{
					float num3 = Mathf.Clamp(self.m_hand.Input.VelLinearWorld.magnitude - 1f, 0f, 3f);
					float num4 = num3 * 120f;
					float num5 = Mathf.Repeat(Mathf.Abs(self.xSpinRot), 360f);
					num4 = Mathf.Clamp(num4, 0f, num5 * 0.5f);
					self.m_tarLeverRot = Mathf.Clamp(-num4, self.LeverAngleRange.x, self.LeverAngleRange.y);
					self.PoseSpinHolder.localPosition = self.m_baseSpinPosition;
				}
				if (Mathf.Abs(self.m_curLeverRot - self.LeverAngleRange.y) < 1f)
				{
					if (self.m_lastLeverPos == LeverActionFirearm.ZPos.Forward)
					{
						self.m_curLeverPos = LeverActionFirearm.ZPos.Middle;
					}
					else
					{
						self.m_curLeverPos = LeverActionFirearm.ZPos.Rear;
						self.IsBreachOpenForGasOut = false;
					}
				}
				else if (Mathf.Abs(self.m_curLeverRot - self.LeverAngleRange.x) < 1f)
				{
					if (self.m_lastLeverPos == LeverActionFirearm.ZPos.Rear)
					{
						self.m_curLeverPos = LeverActionFirearm.ZPos.Middle;
					}
					else
					{
						self.m_curLeverPos = LeverActionFirearm.ZPos.Forward;
						self.IsBreachOpenForGasOut = true;
					}
				}
				else
				{
					self.m_curLeverPos = LeverActionFirearm.ZPos.Middle;
					self.IsBreachOpenForGasOut = true;
				}
				if (self.m_curLeverPos == LeverActionFirearm.ZPos.Rear && self.m_lastLeverPos != LeverActionFirearm.ZPos.Rear)
				{
					self.m_tarLeverRot = self.LeverAngleRange.y;
					self.m_curLeverRot = self.LeverAngleRange.y;
					if (self.m_isActionMovingForward && self.m_proxy.IsFull && !self.Chamber.IsFull)
					{
						self.m_hand.Buzz(self.m_hand.Buzzer.Buzz_OnHoverInteractive);
						self.Chamber.SetRound(self.m_proxy.Round, false);
						self.m_proxy.ClearProxy();
						self.PlayAudioEvent(FirearmAudioEventType.HandleBack, 1f);
					}
					else
					{
						self.PlayAudioEvent(FirearmAudioEventType.HandleBackEmpty, 1f);
					}
					if (self.UsesSecondChamber && self.m_isActionMovingForward && self.m_proxy2.IsFull && !self.Chamber2.IsFull)
					{
						self.Chamber2.SetRound(self.m_proxy2.Round, false);
						self.m_proxy2.ClearProxy();
					}
					self.m_isActionMovingForward = false;
				}
				else if (self.m_curLeverPos == LeverActionFirearm.ZPos.Forward && self.m_lastLeverPos != LeverActionFirearm.ZPos.Forward)
				{
					self.m_tarLeverRot = self.LeverAngleRange.x;
					self.m_curLeverRot = self.LeverAngleRange.x;
					if (!self.m_isActionMovingForward && self.Chamber.IsFull)
					{
						self.m_hand.Buzz(self.m_hand.Buzzer.Buzz_OnHoverInteractive);
						self.Chamber.EjectRound(self.ReceiverEjectionPoint.position, self.transform.right * self.EjectionDir.x + self.transform.up * self.EjectionDir.y + self.transform.forward * self.EjectionDir.z, self.transform.right * self.EjectionSpin.x + self.transform.up * self.EjectionSpin.y + self.transform.forward * self.EjectionSpin.z, false);
						self.PlayAudioEvent(FirearmAudioEventType.HandleForward, 1f);
					}
					else
					{
						self.PlayAudioEvent(FirearmAudioEventType.HandleForwardEmpty, 1f);
					}
					if (self.UsesSecondChamber && !self.m_isActionMovingForward && self.Chamber2.IsFull)
					{
						self.Chamber2.EjectRound(self.SecondEjectionSpot.position, self.transform.right * self.EjectionDir.x + self.transform.up * self.EjectionDir.y + self.transform.forward * self.EjectionDir.z, self.transform.right * self.EjectionSpin.x + self.transform.up * self.EjectionSpin.y + self.transform.forward * self.EjectionSpin.z, false);
					}
					self.m_isActionMovingForward = true;
				}
				else if (/*!self.GrabsRoundFromMagOnBoltForward &&*/ self.m_curLeverPos == LeverActionFirearm.ZPos.Middle && self.m_lastLeverPos == LeverActionFirearm.ZPos.Rear)
				{
					if (self.Magazine != null && !self.m_proxy.IsFull && self.Magazine.HasARound())
					{
						GameObject fromPrefabReference = self.Magazine.RemoveRound(false);
						self.m_proxy.SetFromPrefabReference(fromPrefabReference);
					}
					if (self.UsesSecondChamber && self.Magazine != null && !self.m_proxy2.IsFull && self.Magazine.HasARound())
					{
						GameObject fromPrefabReference2 = self.Magazine.RemoveRound(false);
						self.m_proxy2.SetFromPrefabReference(fromPrefabReference2);
					}
				}
				else if (/*!self.GrabsRoundFromMagOnBoltForward &&*/ self.m_curLeverPos == LeverActionFirearm.ZPos.Middle && self.m_lastLeverPos == LeverActionFirearm.ZPos.Forward)
				{
					if (self.Magazine != null && !self.m_proxy.IsFull && self.Magazine.HasARound())
					{
						GameObject fromPrefabReference3 = self.Magazine.RemoveRound(false);
						self.m_proxy.SetFromPrefabReference(fromPrefabReference3);
					}
					if (self.UsesSecondChamber && self.Magazine != null && !self.m_proxy2.IsFull && self.Magazine.HasARound())
					{
						GameObject fromPrefabReference4 = self.Magazine.RemoveRound(false);
						self.m_proxy2.SetFromPrefabReference(fromPrefabReference4);
					}
				}
				float t = Mathf.InverseLerp(self.LeverAngleRange.y, self.LeverAngleRange.x, self.m_curLeverRot);
				if (self.m_proxy.IsFull)
				{
					if (self.m_isActionMovingForward)
					{
						self.m_proxy.ProxyRound.position = Vector3.Lerp(self.ReceiverUpperPathForward.position, self.ReceiverUpperPathRearward.position, t);
						self.m_proxy.ProxyRound.rotation = Quaternion.Slerp(self.ReceiverUpperPathForward.rotation, self.ReceiverUpperPathRearward.rotation, t);
						if (self.LoadingGate != null)
						{
							self.LoadingGate.localEulerAngles = new Vector3(self.LoadingGateAngleRange.x, 0f, 0f);
						}
					}
					else
					{
						self.m_proxy.ProxyRound.position = Vector3.Lerp(self.ReceiverLowerPathForward.position, self.ReceiverLowerPathRearward.position, t);
						self.m_proxy.ProxyRound.rotation = Quaternion.Slerp(self.ReceiverLowerPathForward.rotation, self.ReceiverLowerPathRearward.rotation, t);
						if (self.LoadingGate != null)
						{
							self.LoadingGate.localEulerAngles = new Vector3(self.LoadingGateAngleRange.y, 0f, 0f);
						}
					}
				}
				else if (self.LoadingGate != null)
				{
					self.LoadingGate.localEulerAngles = new Vector3(self.LoadingGateAngleRange.y, 0f, 0f);
				}
				if (self.Chamber.IsFull)
				{
					self.Chamber.ProxyRound.position = Vector3.Lerp(self.ReceiverEjectionPathForward.position, self.ReceiverEjectionPathRearward.position, t);
					self.Chamber.ProxyRound.rotation = Quaternion.Slerp(self.ReceiverEjectionPathForward.rotation, self.ReceiverEjectionPathRearward.rotation, t);
				}
				if (self.UsesSecondChamber && self.Chamber2.IsFull)
				{
					self.Chamber2.ProxyRound.position = Vector3.Lerp(self.ReceiverEjectionPathForward.position, self.ReceiverEjectionPathRearward.position, t);
					self.Chamber2.ProxyRound.rotation = Quaternion.Slerp(self.ReceiverEjectionPathForward.rotation, self.ReceiverEjectionPathRearward.rotation, t);
				}
				if (self.m_curLeverPos != LeverActionFirearm.ZPos.Rear && !self.m_proxy.IsFull)
				{
					self.Chamber.IsAccessible = true;
				}
				else
				{
					self.Chamber.IsAccessible = false;
				}
				if (self.UsesSecondChamber)
				{
					if (self.m_curLeverPos != LeverActionFirearm.ZPos.Rear && !self.m_proxy2.IsFull)
					{
						self.Chamber2.IsAccessible = true;
					}
					else
					{
						self.Chamber2.IsAccessible = false;
					}
				}
				for (int i = 0; i < self.ActuatedPieces.Length; i++)
				{
					if (self.ActuatedPieces[i].InterpStyle == FVRPhysicalObject.InterpStyle.Translate)
					{
						self.ActuatedPieces[i].Piece.localPosition = Vector3.Lerp(self.ActuatedPieces[i].PosBack, self.ActuatedPieces[i].PosForward, t);
					}
					else
					{
						self.ActuatedPieces[i].Piece.localEulerAngles = Vector3.Lerp(self.ActuatedPieces[i].PosBack, self.ActuatedPieces[i].PosForward, t);
					}
				}
				self.m_lastLeverPos = self.m_curLeverPos;
			}
            else
            {
                orig(self);
            }
        }
#endif

        public void Unhook()
		{
#if !DEBUG
            On.FistVR.LeverActionFirearm.UpdateLever -= LeverActionFirearm_UpdateLever;
#endif
        }
	}
}