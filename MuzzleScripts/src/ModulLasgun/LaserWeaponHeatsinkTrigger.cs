using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace MuzzleScripts
{
    public class LaserWeaponHeatsinkTrigger : MonoBehaviour
    {
        public LaserWeaponHeatsink Heatsink;
        private void OnTriggerEnter(Collider other)
        {
            if (this.Heatsink != null && this.Heatsink.LaserWeapon == null && this.Heatsink.QuickbeltSlot == null)
            {
                LaserWeaponHeatsinkSlot slot = other.gameObject.GetComponent<LaserWeaponHeatsinkSlot>();
                if (slot != null)
                {
                    if (slot.LaserWeapon != null && slot.LaserWeapon.Heatsink == null)
                    {
                        if (slot.LaserWeapon.EjectDelay <= 0f || this.Heatsink != slot.LaserWeapon.LastEjectedHeatsink && slot.LaserWeapon.HasHeatsink && slot.LaserWeapon.Heatsink == null && (!slot.LaserWeapon.DoesSlotRequireManualPartToAccess || (slot.LaserWeapon.MovablePart.IsOpen)))
                        {
                            this.Heatsink.InsertHeatsink(slot.LaserWeapon);
                        }
                    }
                }
            }
        }
    }
}
