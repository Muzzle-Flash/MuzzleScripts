using FistVR;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MuzzleScripts
{
    public class BreakActionSafetySwitch : MonoBehaviour
    {
        public BreakActionWeapon BreakAction;
        public Transform Switch;
        public FVRPhysicalObject.Axis Axis;
        public FireSelectorMode[] FireSelectorModes;
        private int _fireSelectorMode = 0;
        
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
            FVRViveHand hand = BreakAction.m_hand;
            if (hand != null)
            {
                if (hand.IsInStreamlinedMode)
                {
                    if (hand.Input.AXButtonDown)
                    {
                        ChangeFireSelectorMode();
                    }
                }
                else
                {
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
                    {
                        ChangeFireSelectorMode();
                    }
                }

            }
        }

        private void ChangeFireSelectorMode()
        {
            _fireSelectorMode++;
            if (FireSelectorModes.Length <= _fireSelectorMode)
            {
                _fireSelectorMode = 0;
            }
            Vector3 pos = Switch.localPosition;
            switch (Axis)
            {
                case FVRPhysicalObject.Axis.X:
                    pos.x = FireSelectorModes[_fireSelectorMode].SelectorPosition;
                    break;
                case FVRPhysicalObject.Axis.Y:
                    pos.y = FireSelectorModes[_fireSelectorMode].SelectorPosition;
                    break;
                case FVRPhysicalObject.Axis.Z:
                    pos.z = FireSelectorModes[_fireSelectorMode].SelectorPosition;
                    break;
                default:
                    break;
            }
            Switch.localPosition = pos;
            this.BreakAction.PlayAudioEvent(FirearmAudioEventType.FireSelector, 1f);
        }

        public enum FireSelectorModeType
        {
            Safe,
            Single,
            All
        }
        [Serializable]
        public class FireSelectorMode
        {
            public float SelectorPosition;

            public BreakActionSafetySwitch.FireSelectorModeType ModeType;
        }
        public void Hook()
        {
#if!DEBUG
            On.FistVR.BreakActionWeapon.DropHammer += BreakActionWeapon_DropHammer;
#endif
        }
#if!DEBUG
        private void BreakActionWeapon_DropHammer(On.FistVR.BreakActionWeapon.orig_DropHammer orig, BreakActionWeapon self)
        {
            if (self == BreakAction)
            {
                if (!self.m_isLatched || FireSelectorModes[_fireSelectorMode].ModeType == FireSelectorModeType.Safe)
                {
                    return;
                }
                self.firedOneShot = false;
                for (int i = 0; i < self.Barrels.Length; i++)
                {
                    if (self.Barrels[i].m_isHammerCocked)
                    {
                        self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                        self.Barrels[i].m_isHammerCocked = false;
                        self.UpdateVisualHammers();
                        self.Fire(i, self.FireAllBarrels, i);
                        if (!self.FireAllBarrels && FireSelectorModes[_fireSelectorMode].ModeType == FireSelectorModeType.Single)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                orig(self);
            }
        }
#endif

        public void Unhook()
		{
#if!DEBUG
            On.FistVR.BreakActionWeapon.DropHammer -= BreakActionWeapon_DropHammer;
#endif
        }
	}
}