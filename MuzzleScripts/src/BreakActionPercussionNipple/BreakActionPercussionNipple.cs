using FistVR;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MuzzleScripts
{
    public class BreakActionPercussionNipple : MonoBehaviour
    {
        public BreakActionWeapon BreakAction;
        public FVRFireArmChamber[] CapNipples;
        
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
#if!DEBUG
            On.FistVR.BreakActionWeapon.DropHammer += BreakActionWeapon_DropHammer;
#endif
        }
#if!DEBUG
        private void BreakActionWeapon_DropHammer(On.FistVR.BreakActionWeapon.orig_DropHammer orig, BreakActionWeapon self)
        {
            if (self == BreakAction)
            {
                if (!self.m_isLatched)
                {
                    return;
                }
                self.firedOneShot = false;
                for (int i = 0; i < self.Barrels.Length; i++)
                {
                    {
                        if (self.Barrels[i].m_isHammerCocked)
                        {
                            
                            self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                            self.Barrels[i].m_isHammerCocked = false;
                            self.UpdateVisualHammers();
                            if (this.CapNipples[i].Fire())
                            {
                                self.Fire(i, self.FireAllBarrels, i);
                            }

                            if (!self.FireAllBarrels)
                            {
                                break;
                            }

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