using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using OpenScripts2;
using UnityEngine;

namespace MuzzleScripts
{
    public class LaserWeaponHeatsink : FVRPhysicalObject
    {
        public LaserWeapon LaserWeapon;
        [Tooltip("The HeatingEffect used by this Heatsink.")]
        public FirearmHeatingEffect HeatingEffect;
        [Tooltip("The Amount of heat generated per unity of energy consumed by the weapon. Maximum heat is a value of 1.")]
        public float HeatPerEnergyUnit;
        [Tooltip("The rate at which the heatsink cools down.")]
        public float CooldownRate;
        [Tooltip("Whether or not the heatsink's heat level increases the weapon's damage output.")]
        public bool DoesHeatIncreaseDamage;
        [Tooltip("Whether or not this heatsink is integral to the weapon.")]
        public bool IsIntegral;
        [Header("Overheat Config")]
        [Tooltip("Whether or not the heatsink forces the weapon to stop firing once it reaches maximum heat.")]
        public bool CanOverheat;
        [Tooltip("Whether or not the weapon can fire again after the heatsink becomes overheated.")]
        public bool CanRecoverFromOverheat;
        [Tooltip("Heat level the heatsink must cool down to before it can recover from overheating.")]
        public float OverheatRecoveryThreshold;
        [HideInInspector]
        public float Heat;
        public override void FVRUpdate()
        {
            base.FVRUpdate();
            this.HeatingEffect.Heat = this.Heat;
            if (this.LaserWeapon == null)
            {
                this.Heat -= Time.deltaTime * this.CooldownRate * 5f;
            }
        }
        public void InsertHeatsink(LaserWeapon laserWeapon)
        {
            this.LaserWeapon = laserWeapon;
            this.LaserWeapon.InsertHeatsink(this);
            FVRViveHand hand = this.m_hand;
            base.IsHeld = false;
            this.ForceBreakInteraction();
            base.SetParentage(this.LaserWeapon.HeatsinkMountPos);
            base.transform.rotation = this.LaserWeapon.HeatsinkMountPos.rotation;
            base.transform.position = this.LaserWeapon.HeatsinkMountPos.position;
            base.StoreAndDestroyRigidbody();
            if (this.LaserWeapon.QuickbeltSlot != null)
            {
                base.SetAllCollidersToLayer(false, "NoCol");
            }
            else
            {
                base.SetAllCollidersToLayer(false, "Default");
            }
        }
        public void RemoveHeatsink()
        {
            base.SetParentage(null);
            base.transform.position = this.LaserWeapon.HeatsinkMountPos.position;
            base.RecoverRigidbody();
            base.RootRigidbody.isKinematic = false;
            base.RootRigidbody.velocity = this.LaserWeapon.RootRigidbody.velocity + base.transform.up;
            base.RootRigidbody.angularVelocity = this.LaserWeapon.RootRigidbody.angularVelocity;
            this.LaserWeapon = null;
            base.SetAllCollidersToLayer(false, "Default");
        }
        public override bool IsInteractable()
        {
            return this.LaserWeapon == null;
        }
    }
}