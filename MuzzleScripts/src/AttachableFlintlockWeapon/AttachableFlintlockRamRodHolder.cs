using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace MuzzleScripts
{
    public class AttachableFlintlockRamRodHolder : MonoBehaviour
    {
        public AttachableFlintlockWeapon Weapon;

        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null)
            {
                return;
            }
            GameObject gameObject = other.attachedRigidbody.gameObject;
            if (gameObject.CompareTag("flintlock_ramrod"))
            {
                FlintlockRamRod component = gameObject.GetComponent<FlintlockRamRod>();
                if (!component.IsHeld)
                {
                    return;
                }
                FVRViveHand hand = component.m_hand;
                component.ForceBreakInteraction();
                this.Weapon.RamRod.gameObject.SetActive(true);
                this.Weapon.RamRod.RState = FlintlockPseudoRamRod.RamRodState.Lower;
                this.Weapon.RamRod.MountToUnder(hand);
                hand.ForceSetInteractable(this.Weapon.RamRod);
                this.Weapon.RamRod.BeginInteraction(hand);
                UnityEngine.Object.Destroy(other.gameObject);
            }
        }
    }
}
