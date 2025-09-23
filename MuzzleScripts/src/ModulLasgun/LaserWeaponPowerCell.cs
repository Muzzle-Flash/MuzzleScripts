using FistVR;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpeg.AutoGen;

namespace MuzzleScripts
{
    public class LaserWeaponPowerCell : FVRFireArmMagazine
    {
        [Header("Power Cell Config")]
        public Transform PowerDial;
        public FVRPhysicalObject.Axis PowerDialAxis;
        public FVRPhysicalObject.InterpStyle PowerDialInterpStyle;
        public float MaximumEnergyCapacity;
        public FireArmRoundClass DefaultLaserType;
        [Tooltip("The different power levels this power cell can cycle through.")]
        public LaserWeaponPowerCell.PowerLevel[] PowerLevels;

        [Header("Passive Regeneration")]
        [Tooltip("Whether or not this power cell can passively regenerate energy.")]
        public bool CanPassivelyRegenerate;
        [Tooltip("How much energy this power cell passively regenerates per second.")]
        public float PassiveRegenAmount;
        [Tooltip("How much time must pass once the weapon stops firing for passive regen to start.")]
        public float TimeToRegenStart;
        [HideInInspector]
        public int m_powerLevel;
        [HideInInspector]
        public LaserBeamPrefab LaserBeamPrefab;
        private FireArmRoundClass curLaserType;

        public PowerLevel powerLevel
        {
            get
            {
                return this.PowerLevels[m_powerLevel];
            }
        }
        [Serializable]
        public class PowerLevel
        {
            public float DialPosition;
            [Tooltip("Modifies energy consumption, damage output, and noise level.")]
            public sblpCell.PLevel PL;
            [Tooltip("Multiplier that affects energy consumption rate and damage output.")]
            public float PowerCoefficient;
            [Tooltip("How thick the beam should be in meters.")]
            public float BeamThickness;
        }
        public override void Awake()
        {
            base.Awake();
            this.Hook();
            base.ReloadMagWithType(this.DefaultLaserType);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            this.Unhook();
        }
        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            if (hand.IsInStreamlinedMode && hand.Input.BYButtonDown)
            {
                this.TogglePowerLevel();
            }
            else if (!hand.IsInStreamlinedMode && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
            {
                this.TogglePowerLevel();
            }
        }
        public void Hook()
        {
#if !DEBUG
            On.FistVR.FVRFireArmMagazine.ReloadMagWithType += FVRFireArmMagazine_ReloadMagWithType;
            On.FistVR.FVRFireArmMagazine.ReloadMagWithList += FVRFireArmMagazine_ReloadMagWithList;
            On.FistVR.FVRFireArmMagazine.ReloadMagWithTypeUpToPercentage += FVRFireArmMagazine_ReloadMagWithTypeUpToPercentage;
#endif
        }
        public void Unhook()
        {
#if !DEBUG
            On.FistVR.FVRFireArmMagazine.ReloadMagWithType -= FVRFireArmMagazine_ReloadMagWithType;
            On.FistVR.FVRFireArmMagazine.ReloadMagWithList -= FVRFireArmMagazine_ReloadMagWithList;
            On.FistVR.FVRFireArmMagazine.ReloadMagWithTypeUpToPercentage -= FVRFireArmMagazine_ReloadMagWithTypeUpToPercentage;
#endif
        }
#if !DEBUG
        private void FVRFireArmMagazine_ReloadMagWithTypeUpToPercentage(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithTypeUpToPercentage orig, FVRFireArmMagazine self, FireArmRoundClass rClass, float percentage)
        {
            if (self == this)
            {
                this.ReloadMagWithTypeUpToPercentage(rClass, percentage);
            }
            else
            {
                orig.Invoke(self, rClass, percentage);
            }
        }
        private void FVRFireArmMagazine_ReloadMagWithList(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithList orig, FVRFireArmMagazine self, List<FireArmRoundClass> list)
        {
            if (self == this)
            {
                this.ReloadMagWithList(list);
            }
            else
            {
                orig.Invoke(self, list);
            }
        }
        private void FVRFireArmMagazine_ReloadMagWithType(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithType orig, FVRFireArmMagazine self, FireArmRoundClass rClass)
        {
            if (self == this)
            {
                this.ReloadMagWithType(rClass);
            }
            else
            {
                orig.Invoke(self, rClass);
            }
        }
#endif
        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            string key = string.Empty;
            string text = string.Empty;
            if (this.PowerLevels.Length > 1)
            {
                key = "PowerLevel";
                if (f.ContainsKey(key))
                {
                    text = f[key];
                    int.TryParse(text, out this.m_powerLevel);
                }
                if (this.PowerDial != null)
                {
                    LaserWeaponPowerCell.PowerLevel powerLevel = this.PowerLevels[this.m_powerLevel];
                    base.SetAnimatedComponent(this.PowerDial, powerLevel.DialPosition, this.PowerDialInterpStyle, PowerDialAxis);
                }
            }
            base.ConfigureFromFlagDic(f);
        }
        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();
            if (this.PowerLevels.Length > 1)
            {
                string key = "PowerLevel";
                string value = this.m_powerLevel.ToString();
                flagDic.Add(key, value);
            }
            return flagDic;
        }
        public override GameObject DuplicateFromSpawnLock(FVRViveHand hand)
        {
            GameObject gameObject = base.DuplicateFromSpawnLock(hand);
            FVRFireArmMagazine component = gameObject.GetComponent<FVRFireArmMagazine>();
            component.ReloadMagWithType(this.curLaserType);
            component.FuelAmountLeft = this.FuelAmountLeft;
            LaserWeaponPowerCell component2 = gameObject.GetComponent<LaserWeaponPowerCell>();
            component2.m_powerLevel = this.m_powerLevel;
            if (component2.PowerDial != null)
            {
                component2.SetAnimatedComponent(component2.PowerDial, component2.powerLevel.DialPosition, component2.PowerDialInterpStyle, component2.PowerDialAxis);
            }
            return gameObject;
        }
        public void PassiveRegen()
        {
            if (this.CanPassivelyRegenerate)
            {
                if (this.FuelAmountLeft == this.MaximumEnergyCapacity) return;
                this.m_timeSinceRoundInserted = 0f;
                this.FuelAmountLeft += this.PassiveRegenAmount * Time.deltaTime ;
                if (this.FuelAmountLeft >= this.MaximumEnergyCapacity)
                {
                    this.FuelAmountLeft = this.MaximumEnergyCapacity;
                }
            }
        }
        private void GenerateFauxLoadedRound(FireArmRoundClass rClass)
        {
            this.LoadedRounds = new FVRLoadedRound[1];
            FVRLoadedRound FauxLoadedRound = new FVRLoadedRound();
            FauxLoadedRound.LR_Class = rClass;
            this.LoadedRounds[0] = FauxLoadedRound;
            this.m_numRounds = 1;
        }
        public new void ReloadMagWithType(FireArmRoundClass rClass)
        {
            GameObject gameObject = AM.GetRoundSelfPrefab(this.RoundType, rClass).GetGameObject();
            LaserBeamPrefab laserBeamPrefab = gameObject.GetComponent<LaserBeamPrefab>();
            if (laserBeamPrefab != null)
            {
                this.LaserBeamPrefab = laserBeamPrefab;
                this.curLaserType = rClass;
                this.GenerateFauxLoadedRound(rClass);
                this.ForceFull();
                if (this.FireArm != null)
                {
                    LaserWeapon laserWeapon = this.FireArm.GetComponent<LaserWeapon>();
                    if (laserWeapon != null)
                    {
                        laserWeapon.SetBeam(this.LaserBeamPrefab);
                    }
                }
            }
        }
        public new void ReloadMagWithList(List<FireArmRoundClass> list)
        {
            this.ReloadMagWithType(list[0]);
        }
        public new void ReloadMagWithTypeUpToPercentage(FireArmRoundClass rClass, float percentage)
        {
            this.ForceEmpty();
            float num = this.MaximumEnergyCapacity * percentage;
            if (num >= this.MaximumEnergyCapacity)
            {
                num = this.MaximumEnergyCapacity;
            }
            if (num <= 0)
            {
                num = 0;
            }
            this.FuelAmountLeft = num;
            GameObject gameObject = AM.GetRoundSelfPrefab(this.RoundType, rClass).GetGameObject();
            LaserBeamPrefab laserBeamPrefab = gameObject.GetComponent<LaserBeamPrefab>();
            if (laserBeamPrefab != null)
            {
                this.LaserBeamPrefab = laserBeamPrefab;
                this.curLaserType = rClass;
                this.GenerateFauxLoadedRound(rClass);
                if (this.FireArm != null)
                {
                    LaserWeapon laserWeapon = this.FireArm.GetComponent<LaserWeapon>();
                    if (laserWeapon != null)
                    {
                        laserWeapon.SetBeam(this.LaserBeamPrefab);
                    }
                }
            }
        }
        public new void ForceFull()
        {
            this.FuelAmountLeft = this.MaximumEnergyCapacity;
        }
        public new void ForceEmpty()
        {
            this.FuelAmountLeft = 0;
        }
        public new bool IsFull()
        {
            return this.FuelAmountLeft == this.MaximumEnergyCapacity;
        }
        public bool IsEmpty()
        {
            return this.FuelAmountLeft == 0;
        }
        public void AddEnergy(float f)
        {
            if (this.IsFull()) return;
            this.m_timeSinceRoundInserted = 0f;
            this.FuelAmountLeft += f;
            if (this.FuelAmountLeft > this.MaximumEnergyCapacity)
            {
                this.FuelAmountLeft = this.MaximumEnergyCapacity;
            }
        }
        public void TogglePowerLevel()
        {
            if (this.PowerLevels.Length > 1)
            {
                this.m_powerLevel++;
                if (this.m_powerLevel >= this.PowerLevels.Length)
                {
                    this.m_powerLevel -= this.PowerLevels.Length;
                }
                LaserWeaponPowerCell.PowerLevel powerLevel = this.PowerLevels[this.m_powerLevel];
                if (this.PowerDial != null)
                {
                    this.SetAnimatedComponent(this.PowerDial, powerLevel.DialPosition, this.PowerDialInterpStyle, this.PowerDialAxis);
                }
                SM.PlayGenericSound(this.Profile.FireSelector, base.transform.position);
            }
        }
    }
}
