using FistVR;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FMOD;
using OpenScripts2;

namespace MuzzleScripts
{
    public class LaserWeapon : FVRFireArm
    {
        [Header("Laser Weapon Config")]
        public bool HasTrigger;
        public bool HasFireSelectorButton;
        public bool HasMagazineReleaseButton;

        [Header("Audio Config")]
        public AudioSource AudSource_B;
        [Tooltip("Audio Event that plays when the beam engages.")]
        public List<AudioEvent> AudEvent_Start;
        [Tooltip("Audio Event that plays when the beam disengages.")]
        public List<AudioEvent> AudEvent_End;
        [Tooltip("Audio Event that plays when the weapon errors.")]
        public AudioEvent AudEvent_Err;
        [Tooltip("Audio Event that plays when the weapon overheats.")]
        public AudioEvent AudEvent_Overheat;

        [Header("Beam Config")]
        public float DamageMultiplier = 1f;
        [Tooltip("Maximum distance the laser will go without colliding with anything.")]
        public float MaximumRange = 100f;
        [Tooltip("Whether or not barrel length affects the beam's maximum range. The maximum range will be multiplied by 1 + the length of the barrel in meters.")]
        public bool DoesBarrelLengthAffectMaximumRange;
        [Tooltip("Where the weapon begins to measure its barrel length to the muzzle.")]
        public Transform BarrelOrigin;
        [HideInInspector]
        public LaserBeamPrefab LaserBeamPrefab;
        public LaserBeamPrefab orig_LaserBeamPrefab;
        private float m_barrelLength;

        

        [Tooltip("What layers of colliders the laser will or will not ignore.")]
        public LayerMask BeamLayerMask;
        [Tooltip("Particle system that fires when the beam engages.")]
        public ParticleSystem PSys_Engage;
        [Tooltip("Particle system that fires when the beam disengages.")]
        public ParticleSystem PSys_Disengage;
        [Tooltip("Particle system that fires when the weapon overheats.")]
        public ParticleSystem PSys_Overheat;
        public BulletHoleDecalType BulletHoleDecalOverride;
        public List<sblp.BParamType> BParams;
		public Dictionary<MatBallisticType, sblp.BParamType> BPDic = new Dictionary<MatBallisticType, sblp.BParamType>();

        [HideInInspector]
        public List<sblpPoint> ImpactPoints = new List<sblpPoint>();
        [HideInInspector]
        public GameObject ImpactPointPrefab;
        [HideInInspector]
        public List<LineRenderer> Beams = new List<LineRenderer>();
        [HideInInspector]
        private float m_timeToEnergyConsumption = 0.1f;
        private float m_energyConsumed;
        private bool m_isBeamEngaged;
        private float m_beamThickness;
        private RaycastHit m_hit;

        [Header("Trigger Config")]
        public Transform Trigger;
        public float TriggerFiringThreshold;
        public float TriggreResetThreshold;
        public float TriggerForwardValue;
        public float TriggerRearwardValue;
        public FVRPhysicalObject.Axis TriggerAxis;
        public FVRPhysicalObject.InterpStyle TriggerInterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        private float m_triggerFloat;
        private bool m_hasTriggerReset;
        private int m_camBurst;
        private float m_engagementDelay;

        [Header("Fire Selector Config")]
        public Transform FireSelectorSwitch;
        public FVRPhysicalObject.Axis FireSelectorAxis;
        public FVRPhysicalObject.InterpStyle FireSelectorInterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        public LaserWeapon.FireSelectorMode[] FireSelectorModes;
        private int m_fireSelectorMode;

        [Header("Magazine Release Button Config")]
        public Transform MagazineReleaseButton;
        public FVRPhysicalObject.InterpStyle MagReleaseInterpStyle;
        public Vector3 MagReleaseUnpressed;
        public Vector3 MagReleasePressed;
        private bool m_wasMagReleaseHeldDown;
        private bool m_isMagReleaseButtonHeld;

        [Header("Heat Config")]
        [Tooltip("Whether or not the weapon generates heat as it fires.")]
        public bool DoesGenerateHeat;
        [Tooltip("The default HeatingEffect used if the weapon does not have a Heatsink.")]
        public FirearmHeatingEffect HeatingEffect;
        [Tooltip("The Amount of heat generated per unity of energy consumed. Maximum heat is a value of 1.")]
        public float HeatPerEnergySpent;
        [Tooltip("The rate at which the weapon cools down.")]
        public float CooldownRate;
        [Tooltip("Whether or not the weapon's heat level increases its damage output.")]
        public bool DoesHeatIncreaseDamage;
        private float orig_HeatPerEnergySpent;
        private float orig_CooldownRate;

        [Header("Overheat Config")]
        [Tooltip("Whether or not the weapon stops firing once it reaches maximum heat.")]
        public bool CanOverheat;
        [Tooltip("Whether or not the weapon can fire again after becoming overheated.")]
        public bool CanRecoverFromOverheat;
        [Tooltip("Heat level the weapon must cool down to before it can recover from overheating.")]
        public float OverheatRecoveryThreshold;

        [Header("Heatsink Config")]
        [Tooltip("Whether or not the weapon uses a discrete Heatsink.")]
        public bool HasHeatsink;
        public LaserWeaponHeatsinkSlot HeatsinkSlot;
        public Transform HeatsinkMountPos;
        public LaserWeaponHeatsink Heatsink;
        [Tooltip("Whether or not the Heatsink slot requires moving a part to access.")]
        public bool DoesSlotRequireManualPartToAccess;
        [Tooltip("Whether or not the Heatsink's cooling behavior is affected by a manually moved part.")]
        public bool DoesManualPartAffectCooling;
        [Tooltip("While the manual part is closed, the Heatsink's cooling rate will be multiplied by this value.")]
        public float ClosedCoolingRateMultiplier;
        [Tooltip("While the manual part is open, the Heatsink's cooling rate will be multiplied by this value.")]
        public float OpenCoolingRateMultiplier;
        public MovableObjectPart MovablePart;
        private LaserWeaponHeatsink m_lastEjectedHeatsink;


        [HideInInspector]
        private float m_heat;
        private bool m_isOverheated;

        [Header("Special Features")]
        public bool EjectsMagazineOnEmpty;
        public Transform MagMountTransformOverride;
        
        private float m_timeSinceFired;
        private float m_regenTimer;
        private bool m_hasErrored = false;

        public LaserWeaponHeatsink LastEjectedHeatsink
        {
            get
            {
                return this.m_lastEjectedHeatsink;
            }
        }

        public enum FireSelectorModeType
        {
            Safe,
            Single,
            Burst,
            Beam,
        }

        [Serializable]
        public class FireSelectorMode
        {
            public float SelectorPosition;
            public LaserWeapon.FireSelectorModeType ModeType;
            public int BurstAmount = 3;
            [Tooltip("Determines if the weapon 'remembers' the remaining number of pulses from the last burst")]
            public bool ARStyleBurst;
            [Tooltip("The amount of time (in seconds) between pulses in a burst")]
            public float EngagementDelay;
            [Tooltip("The amount of energy a single pulse should consume. At lower power levels the pulse may be longer.")]
            public float EnergyToUse;
        }

        public override void Awake()
        {
            base.Awake();
            if (this.FChambers.Count == 0 )
            {
                FVRFireArmChamber chamber = new FVRFireArmChamber();
                chamber.Firearm = this;
                chamber.IsAccessible = false;
                this.FChambers.Add(chamber);
            }
            this.Dics();
            if (this.LaserBeamPrefab != null) this.SetBeam(this.LaserBeamPrefab);
            this.orig_CooldownRate = this.CooldownRate;
            this.orig_HeatPerEnergySpent = this.HeatPerEnergySpent;
            this.orig_LaserBeamPrefab = this.LaserBeamPrefab;
            if (this.Heatsink != null)
            {
                if (this.Heatsink.IsIntegral)
                {
                    this.InsertHeatsink(this.Heatsink);
                }
            }
        }
        public void SetBeam(LaserBeamPrefab beam)
        {
            for (int i = 0; i < this.Beams.Count; i++)
            {
                UnityEngine.Object.Destroy(this.Beams[i].gameObject);
            }
            for (int i = 0; i < this.ImpactPoints.Count; i++)
            {
                UnityEngine.Object.Destroy(this.ImpactPoints[i].gameObject);
            }
            this.Beams.Clear();
            this.ImpactPoints.Clear();
            this.LaserBeamPrefab = beam;
            for (int i = 0; i < 10; i++)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.LaserBeamPrefab.ImpactPointPrefab);
                sblpPoint component = gameObject.GetComponent<sblpPoint>();
                this.ImpactPoints.Add(component);
                this.ImpactPoints[i].Geo.SetActive(false);
                ParticleSystem.EmissionModule emission = this.ImpactPoints[i].PSys.emission;
                emission.enabled = false;
                LineRenderer lineRenderer = UnityEngine.Object.Instantiate<LineRenderer>(this.LaserBeamPrefab.LineRenderer);
                this.Beams.Add(lineRenderer);
                lineRenderer.enabled = false;
            }
        }
        private void Dics()
        {
            for (int i = 0; i < this.BParams.Count; i++)
            {
                for (int j = 0; j < this.BParams[i].Mats.Count; j++)
                {
                    if (!this.BPDic.ContainsKey(this.BParams[i].Mats[j]))
                    {
                        this.BPDic.Add(this.BParams[i].Mats[j], this.BParams[i]);
                    }
                }
            }
        }
        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            string key = string.Empty;
            string text = string.Empty;
            if (this.FireSelectorModes.Length > 1)
            {
                key = "FireSelectorState";
                if (f.ContainsKey(key))
                {
                    text = f[key];
                    int.TryParse(text, out this.m_fireSelectorMode);
                }
                if (this.FireSelectorSwitch != null)
                {
                    LaserWeapon.FireSelectorMode fireSelectorMode = this.FireSelectorModes[this.m_fireSelectorMode];
                    base.SetAnimatedComponent(this.FireSelectorSwitch, fireSelectorMode.SelectorPosition, this.FireSelectorInterpStyle, this.FireSelectorAxis);
                }
            }
            base.ConfigureFromFlagDic(f);
        }
        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();
            if (this.FireSelectorModes.Length > 1)
            {
                string key = "FireSelectorState";
                string value = this.m_fireSelectorMode.ToString();
                flagDic.Add(key, value);
            }
            return flagDic;
        }
        public void InsertHeatsink(LaserWeaponHeatsink heatsink)
        {
            if (this.Heatsink == null && heatsink != null)
            {
                this.m_lastEjectedHeatsink = null;
                this.Heatsink = heatsink;
                if (this.m_hand != null)
                {
                    this.m_hand.Buzz(this.m_hand.Buzzer.Buzz_BeginInteraction);
                    if (this.Heatsink.m_hand != null)
                    {
                        this.Heatsink.m_hand.Buzz(this.m_hand.Buzzer.Buzz_BeginInteraction);
                    }
                }
                this.CooldownRate = this.Heatsink.CooldownRate;
                this.HeatPerEnergySpent = this.Heatsink.HeatPerEnergyUnit;
                this.m_heat = this.Heatsink.Heat;
                this.CanOverheat = this.Heatsink.CanOverheat;
                this.CanRecoverFromOverheat = this.Heatsink.CanOverheat;
                this.OverheatRecoveryThreshold = this.Heatsink.OverheatRecoveryThreshold;
                if (this.m_heat < 1) this.m_isOverheated = false;
                else if (this.m_heat >= 1) this.m_isOverheated = true;
                this.PlayAudioEvent(FirearmAudioEventType.MagazineIn, 0.85f);
            }
        }
        public void RemoveHeatsink()
        {
            if (this.Heatsink != null)
            {
                this.CooldownRate = this.orig_CooldownRate;
                this.HeatPerEnergySpent = this.orig_HeatPerEnergySpent;
                this.m_lastEjectedHeatsink = this.Heatsink;
                this.m_ejectDelay = 0.4f;
                this.PlayAudioEvent(FirearmAudioEventType.MagazineOut, 1f);
                if (this.m_hand != null)
                {
                    this.m_hand.Buzz(this.m_hand.Buzzer.Buzz_BeginInteraction);
                }
                this.Heatsink.RemoveHeatsink();
                if (this.Heatsink.m_hand != null)
                {
                    this.Magazine.m_hand.Buzz(this.m_hand.Buzzer.Buzz_BeginInteraction);
                }
                this.Heatsink = null;
            }
        }
        public void ReleaseMag()
        {
            if (this.Magazine != null)
            {
                base.EjectMag(false);
            }
        }
        public void ToggleMagPower()
        {
            if (this.Magazine == null) return;
            LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
            if (powerCell != null)
            {
                powerCell.TogglePowerLevel();
            }
        }
        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            this.m_isMagReleaseButtonHeld = false;
            if (this.IsAltHeld) return;
            if (this.m_hasTriggeredUpSinceBegin)
            {
                this.m_triggerFloat = hand.Input.TriggerFloat;
            }
            else
            {
                this.m_triggerFloat = 0f;
            }
            if (!this.m_hasTriggerReset && this.m_triggerFloat <= this.TriggreResetThreshold)
            {
                this.m_hasTriggerReset = true;
                this.TryToDisengageBeam();
                this.m_hasErrored = false;
                if (this.FireSelectorModes.Length > 0)
                {
                    if (this.FireSelectorModes[this.m_fireSelectorMode].ARStyleBurst && this.m_camBurst <= 0)
                    {
                        this.m_camBurst = this.FireSelectorModes[this.m_fireSelectorMode].BurstAmount;
                    }
                    else if (!this.FireSelectorModes[this.m_fireSelectorMode].ARStyleBurst)
                    {
                        this.m_camBurst = this.FireSelectorModes[this.m_fireSelectorMode].BurstAmount;
                    }
                    this.m_engagementDelay = this.FireSelectorModes[this.m_fireSelectorMode].EngagementDelay;
                }
                base.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
            }
            if (hand.IsInStreamlinedMode)
            {
                if (hand.Input.BYButtonDown)
                {
                    this.ToggleFireSelector();
                }
                if (hand.Input.AXButtonDown && this.HasMagazineReleaseButton)
                {
                    this.m_isMagReleaseButtonHeld = true;
                    this.ReleaseMag();
                }
            }
            else
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f)
                {
                    this.ToggleFireSelector();
                }
                if (hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
                {
                    this.m_isMagReleaseButtonHeld = true;
                    this.ReleaseMag();
                }
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    this.ToggleMagPower();
                }
            }
            LaserWeapon.FireSelectorModeType modeType = this.FireSelectorModes[this.m_fireSelectorMode].ModeType;
            if (modeType != LaserWeapon.FireSelectorModeType.Safe && this.m_hasTriggeredUpSinceBegin)
            {
                if (this.m_triggerFloat >= this.TriggerFiringThreshold && (this.m_hasTriggerReset || modeType == FireSelectorModeType.Beam || (modeType == FireSelectorModeType.Burst && this.m_camBurst > 0 && this.m_engagementDelay <= 0)))
                {
                    this.TryToEngageBeam();
                    this.m_hasTriggerReset = false;
                }
            }
        }
        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.m_engagementDelay > 0f)
            {
                this.m_engagementDelay -= Time.deltaTime;
            }
            if (this.m_timeSinceFired < this.m_regenTimer)
            {
                this.m_timeSinceFired += Time.deltaTime;
            }
            LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
            if (powerCell != null)
            {
                if (powerCell.CanPassivelyRegenerate && this.m_timeSinceFired >= this.m_regenTimer && powerCell.TimeSinceRoundInserted > 0.5f)
                {
                    powerCell.PassiveRegen();
                }
                if (powerCell.IsEmpty() && this.EjectsMagazineOnEmpty)
                {
                    this.EjectMag();
                }
            }
            this.UpdateComponents();
            if (!this.IsHeld)
            {
                this.TryToDisengageBeam();
            }
            this.UpdateBeam();
            this.UpdateHeat();
        }
        public void UpdateHeat()
        {
            if (!this.m_isBeamEngaged)
            {
                float CooldownMultiplier = 1;
                if (this.DoesManualPartAffectCooling)
                {
                    if (this.MovablePart.IsOpen)
                    {
                        CooldownMultiplier = this.OpenCoolingRateMultiplier;
                    }
                    else if (this.MovablePart.IsClosed)
                    {
                        CooldownMultiplier = this.ClosedCoolingRateMultiplier;
                    }
                }
                this.m_heat -= Time.deltaTime * CooldownRate * CooldownMultiplier;
            }
            this.m_heat = Mathf.Clamp01(m_heat);
            if (this.HasHeatsink && this.Heatsink != null)
            {
                this.Heatsink.Heat = this.m_heat;
            }
            else if (!this.HasHeatsink && this.HeatingEffect != null)
            {
                this.HeatingEffect.Heat = this.m_heat;
            }
            if (this.CanOverheat)
            {
                if (this.m_heat >= 1)
                {
                    this.m_isOverheated = true;
                    if (this.AudEvent_Overheat != null) base.PlayAudioAsHandling(this.AudEvent_Overheat, base.transform.position);
                    if (this.PSys_Overheat != null) this.PSys_Overheat.Emit(2000);
                }
                if (this.CanRecoverFromOverheat)
                {
                    if (this.m_heat <= this.OverheatRecoveryThreshold) this.m_isOverheated = false;
                }
            }
        }
        public void UpdateComponents()
        {
            if (this.HasTrigger)
            {
                base.SetAnimatedComponent(this.Trigger, Mathf.Lerp(this.TriggerForwardValue, this.TriggerRearwardValue, this.m_triggerFloat), this.TriggerInterpStyle, this.TriggerAxis);
            }
            if (this.HasMagazineReleaseButton && this.MagazineReleaseButton != null)
            {
                float t = 0f;
                if (this.m_isMagReleaseButtonHeld)
                {
                    t = 1f;
                }
                Vector3 val = Vector3.Lerp(this.MagReleaseUnpressed, this.MagReleasePressed, t);
                if (this.m_wasMagReleaseHeldDown != this.m_isMagReleaseButtonHeld)
                {
                    this.m_wasMagReleaseHeldDown = this.m_isMagReleaseButtonHeld;
                }
                base.SetAnimatedComponent(this.MagazineReleaseButton, val, this.MagReleaseInterpStyle);
            }
        }
        public void UpdateBeam()
        {
            if (this.m_isBeamEngaged)
            {
                if (this.Magazine == null)
                {
                    this.TryToDisengageBeam();
                    return;
                }
                if (this.Heatsink == null)
                {
                    this.TryToDisengageBeam();
                    return;
                }
                if (!this.IsHeld)
                {
                    this.TryToDisengageBeam();
                    return;
                }
                if ((this.FireSelectorModes[m_fireSelectorMode].ModeType == FireSelectorModeType.Single || this.FireSelectorModes[m_fireSelectorMode].ModeType == FireSelectorModeType.Burst) && this.m_energyConsumed >= this.FireSelectorModes[m_fireSelectorMode].EnergyToUse)
                {
                    if (this.m_camBurst > 0) this.m_camBurst--;
                    this.m_engagementDelay = this.FireSelectorModes[m_fireSelectorMode].EngagementDelay;
                    this.TryToDisengageBeam();
                    return;
                }
                if (this.m_isOverheated)
                {
                    this.TryToDisengageBeam();
                    return;
                }
                LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
                this.m_timeToEnergyConsumption -= Time.deltaTime;
                if (this.m_timeToEnergyConsumption < 0f)
                {
                    float f = 0.25f;
                    if (powerCell.powerLevel.PL == sblpCell.PLevel.Medium)
                    {
                        f = 1f;
                    }
                    else if (powerCell.powerLevel.PL == sblpCell.PLevel.High)
                    {
                        f = 3f;
                    }
                    f *= powerCell.powerLevel.PowerCoefficient;
                    if (!this.Magazine.HasFuel(f))
                    {
                        this.TryToDisengageBeam();
                        this.Magazine.ForceEmpty();
                        return;
                    }
                    if (!this.Magazine.IsInfinite) this.Magazine.DrainFuel(f);
                    this.m_timeSinceFired = 0f;
                    this.m_energyConsumed += f;
                    if (this.DoesGenerateHeat)
                    {
                        this.m_heat += f * this.HeatPerEnergySpent;
                    }
                    this.m_timeToEnergyConsumption = 0.1f;
                }
                Vector3 laserPos = this.GetMuzzle().position;
                Vector3 laserDir = this.GetMuzzle().forward;
                float num;
                int num2;
                float num3 = powerCell.powerLevel.BeamThickness;
                float num4 = this.MaximumRange;
                this.m_barrelLength = Vector3.Distance(this.BarrelOrigin.position, this.GetMuzzle().position);
                if (this.DoesBarrelLengthAffectMaximumRange)
                {
                    num4 += (this.m_barrelLength * 1000f);
                }
                switch (powerCell.powerLevel.PL)
                {
                    case sblpCell.PLevel.Low:
                        num = 0.14f;
                        num2 = 2;
                        break;
                    case sblpCell.PLevel.Medium:
                        num = 0.25f;
                        num2 = 4;
                        break;
                    case sblpCell.PLevel.High:
                        num = 1f;
                        num2 = 10;
                        break;
                    default:
                        num = 1f;
                        num2 = 10;
                        break;
                }
                for (int k = 0; k < this.Beams.Count; k++)
                {
                    if (num2 >= 1)
                    {
                        this.Beams[k].transform.position = laserPos;
                        this.Beams[k].transform.rotation = Quaternion.LookRotation(laserDir);

                        Ray ray = new Ray(laserPos, laserDir);
                        if (Physics.Raycast(ray, out this.m_hit, num4, this.BeamLayerMask))
                        {
                            this.ImpactPoints[k].transform.position = this.m_hit.point;
                            this.ImpactPoints[k].transform.rotation = Quaternion.LookRotation(laserDir);
                            this.ImpactPoints[k].Geo.SetActive(true);
                            ParticleSystem.EmissionModule emission = this.ImpactPoints[k].PSys.emission;
                            emission.enabled = true;
                            emission.rateOverTime = (float)(5 * (10 - k));
                            num4 -= this.m_hit.distance;
                            this.Beams[k].startWidth = num3;
                            this.Beams[k].endWidth = num3;
                            this.Beams[k].positionCount = 2;
                            this.Beams[k].SetPositions([laserPos, this.m_hit.point]);
                            this.Beams[k].enabled = true;
                            this.Beams[k].gameObject.SetActive(true);
                            bool hasRigidBody = false;
                            if (this.m_hit.collider.attachedRigidbody != null) hasRigidBody = true;

                            bool isVaporizable = false;
                            IVaporizable vaporizable = this.m_hit.collider.transform.gameObject.GetComponent<IVaporizable>();
                            if (vaporizable == null && hasRigidBody)
                            {
                                vaporizable = this.m_hit.collider.attachedRigidbody.gameObject.GetComponent<IVaporizable>();
                            }
                            if (vaporizable != null) isVaporizable = true;

                            bool isDamagable = false;
                            IFVRDamageable damageable = this.m_hit.collider.transform.gameObject.GetComponent<IFVRDamageable>();
                            if (damageable == null && hasRigidBody)
                            {
                                damageable = this.m_hit.collider.attachedRigidbody.gameObject.GetComponent<IFVRDamageable>();
                            }
                            if (damageable != null) isDamagable = true;

                            bool isIgnitable = false;
                            FVRIgnitable ignitable = this.m_hit.collider.transform.gameObject.GetComponent<FVRIgnitable>();
                            if (ignitable == null && hasRigidBody)
                            {
                                ignitable = this.m_hit.collider.attachedRigidbody.gameObject.GetComponent<FVRIgnitable>();
                            }
                            if (ignitable != null) isIgnitable = true;

                            bool hasMatdef = false;
                            PMat pmat = this.m_hit.collider.transform.gameObject.GetComponent<PMat>();
                            if (pmat != null && pmat.MatDef != null) hasMatdef = true;
                            MatDef matDef;
                            if (!hasMatdef)
                            {
                                matDef = PM.DefaultMatDef;
                            }
                            else
                            {
                                matDef = pmat.MatDef;
                            }
                            int num6 = 10;
                            bool reflectsBeam = false;
                            if (pmat != null && this.BPDic.ContainsKey(pmat.MatDef.BallisticType))
                            {
                                sblp.BParamType bParamType = this.BPDic[pmat.MatDef.BallisticType];
                                num6 = bParamType.Pen;
                                reflectsBeam = bParamType.Reflects;
                            }
                            if (isIgnitable)
                            {
                                CumulativeEffectTicker effectTicker = ignitable.gameObject.GetComponent<CumulativeEffectTicker>();
                                if (effectTicker == null)
                                {
                                    effectTicker = ignitable.gameObject.AddComponent<CumulativeEffectTicker>();
                                }
                                else
                                {
                                    effectTicker.EffectProgress += this.LaserBeamPrefab.IgnitionProgressPerHit;
                                }
                                if (this.LaserBeamPrefab.DoesIgniteOnHit && effectTicker.EffectProgress >= ignitable.IgnitionThreshold)
                                {
                                    FXM.Ignite(ignitable, effectTicker.EffectProgress);
                                    Component.Destroy(effectTicker);
                                }
                            }
                            if (isVaporizable)
                            {
                                CumulativeEffectTicker effectTicker = this.m_hit.collider.gameObject.GetComponent<CumulativeEffectTicker>();
                                if (effectTicker == null)
                                {
                                    effectTicker = this.m_hit.collider.gameObject.AddComponent<CumulativeEffectTicker>();
                                }
                                else
                                {
                                    effectTicker.EffectProgress += this.LaserBeamPrefab.VaporizeProgressPerHit;
                                }
                                if (this.LaserBeamPrefab.DoesVaporizeOnHit && effectTicker.EffectProgress >= 1f)
                                {
                                    vaporizable.Vaporize(GM.CurrentPlayerBody.GetPlayerIFF());
                                    Component.Destroy(effectTicker);
                                }
                            }
                            if (isDamagable)
                            {
                                Damage damage = new Damage();
                                damage.Class = Damage.DamageClass.Projectile;
                                float totalKinetic = (float)num2 * this.LaserBeamPrefab.TotalKineticDamagePerHit * num;
                                damage.Dam_TotalKinetic = totalKinetic;
                                damage.Dam_Blunt = totalKinetic * 0.1f;
                                damage.Dam_Piercing = totalKinetic * 0.9f;
                                float totalEnergetic = this.LaserBeamPrefab.TotalEnergeticDamagePerHit * num;
                                if (this.LaserBeamPrefab.IsFreezingRay)
                                {
                                    damage.Dam_Chilling = totalEnergetic * num;
                                    damage.Dam_Thermal = 0f;
                                }
                                else
                                {
                                    damage.Dam_Thermal = totalEnergetic * num;
                                    damage.Dam_Chilling = 0f;
                                }
                                if (this.LaserBeamPrefab.IsStunningRay)
                                {
                                    damage.Dam_Stunning = this.LaserBeamPrefab.StunDuration * num;
                                }
                                if (this.LaserBeamPrefab.IsBlindingRay)
                                {
                                    damage.Dam_Blinding = this.LaserBeamPrefab.BlindDuration * num;
                                }
                                if (this.LaserBeamPrefab.IsEMPRay)
                                {
                                    damage.Dam_EMP = this.LaserBeamPrefab.EMPDisruptionDuration * num;
                                }
                                damage.Dam_TotalEnergetic = 100 * num;
                                float damageMult = this.DamageMultiplier;
                                if (this.DoesHeatIncreaseDamage)
                                {
                                    damageMult  = this.DamageMultiplier + this.m_heat;
                                }
                                damage.Dam_TotalEnergetic *= damageMult * powerCell.powerLevel.PowerCoefficient;
                                damage.Dam_TotalKinetic *= damageMult * powerCell.powerLevel.PowerCoefficient;
                                damage.point = this.m_hit.point;
                                damage.hitNormal = this.m_hit.normal;
                                damage.strikeDir = laserDir;
                                damage.damageSize = num3;
                                damage.Source_IFF = GM.CurrentPlayerBody.GetPlayerIFF();
                                this.DealDamage(damage, damageable, reflectsBeam);
                            }
                            num2 -= num6;
                            Vector3 reflectDir = laserDir;
                            if (reflectsBeam)
                            {
                                reflectDir = Vector3.Reflect(laserDir, this.m_hit.normal);
                            }
                            if (!isDamagable && !reflectsBeam && GM.Options.SimulationOptions.HitDecalMode2 == SimulationOptions.HitDecals.Enabled)
                            {
                                BulletHoleDecalType bulletHoleDecalType = matDef.BulletHoleType;
                                if (bulletHoleDecalType != BulletHoleDecalType.None && this.BulletHoleDecalOverride != BulletHoleDecalType.None)
                                {
                                    bulletHoleDecalType = this.BulletHoleDecalOverride;
                                }
                                float damageSize = Mathf.Clamp(num3 * 3f, 0.0001f, 0.01f);
                                if (bulletHoleDecalType != BulletHoleDecalType.None)
                                {
                                    FXM.SpawnBulletDecal(bulletHoleDecalType, this.m_hit.point, this.m_hit.normal, damageSize);
                                }
                            }
                            if (Vector3.Angle(reflectDir, laserDir) >= 90f)
                            {
                                laserPos = this.m_hit.point + this.m_hit.normal * 0.001f;
                            }
                            else
                            {
                                laserPos = this.m_hit.point - this.m_hit.normal * 0.001f;
                            }
                            laserDir = reflectDir;
                        }
                        else
                        {
                            this.Beams[k].startWidth = num3;
                            this.Beams[k].endWidth = num3;
                            this.Beams[k].positionCount = 2;
                            this.Beams[k].SetPositions([laserPos, ray.GetPoint(num4)]);
                            this.Beams[k].enabled = true;
                            this.Beams[k].gameObject.SetActive(true);
                            this.ImpactPoints[k].Geo.SetActive(false);
                            ParticleSystem.EmissionModule emission = this.ImpactPoints[k].PSys.emission;
                            emission.enabled = false;
                        }
                    }
                    else
                    {
                        this.ImpactPoints[k].Geo.SetActive(false);
                        ParticleSystem.EmissionModule emission = this.ImpactPoints[k].PSys.emission;
                        emission.enabled = false;
                        this.Beams[k].enabled = false;
                    }
                }
            }
        }
        public void DealDamage(Damage damage, IFVRDamageable damageable, bool reflectsBeam)
        {
            SosigLink sosigLink = damageable as SosigLink;
            SosigWearable sosigWearable = damageable as SosigWearable;
            FVRPlayerHitbox playerHitbox = damageable as FVRPlayerHitbox;
            if (sosigLink != null)
            {
                foreach (LaserBeamPrefab.OnHitEffect onHitEffect in this.LaserBeamPrefab.OnHitEffects)
                {
                    CumulativeEffectTicker effectTicker = sosigLink.gameObject.GetComponent<CumulativeEffectTicker>();
                    if (effectTicker == null)
                    {
                        effectTicker = sosigLink.gameObject.AddComponent<CumulativeEffectTicker>();
                    }
                    else
                    {
                        effectTicker.EffectProgress += onHitEffect.EffectProgressPerHit;
                    }
                    if (effectTicker.EffectProgress >= 1)
                    {
                        sosigLink.S.ActivatePower(onHitEffect.PType, onHitEffect.PIntensity, onHitEffect.PDuration, onHitEffect.PIsPuke, onHitEffect.PIsInverted);
                        Component.Destroy(effectTicker);
                    }
                }
            }
            if (sosigWearable != null && !reflectsBeam)
            {
                foreach (LaserBeamPrefab.OnHitEffect onHitEffect in this.LaserBeamPrefab.OnHitEffects)
                {
                    CumulativeEffectTicker effectTicker = sosigWearable.GetComponent<CumulativeEffectTicker>();
                    if (effectTicker == null)
                    {
                        effectTicker = sosigWearable.gameObject.AddComponent<CumulativeEffectTicker>();
                    }
                    else
                    {
                        effectTicker.EffectProgress += onHitEffect.EffectProgressPerHit;
                    }
                    if (effectTicker.EffectProgress >= 1)
                    {
                        sosigWearable.S.ActivatePower(onHitEffect.PType, onHitEffect.PIntensity, onHitEffect.PDuration, onHitEffect.PIsPuke, onHitEffect.PIsInverted);
                        Component.Destroy(effectTicker);
                    }
                }
            }
            if (playerHitbox != null)
            {
                foreach (LaserBeamPrefab.OnHitEffect onHitEffect in this.LaserBeamPrefab.OnHitEffects)
                {
                    CumulativeEffectTicker effectTicker = playerHitbox.gameObject.GetComponent<CumulativeEffectTicker>();
                    if (effectTicker == null)
                    {
                        effectTicker = playerHitbox.gameObject.AddComponent<CumulativeEffectTicker>();
                    }
                    else
                    {
                        effectTicker.EffectProgress += onHitEffect.EffectProgressPerHit;
                    }
                    if (effectTicker.EffectProgress >= 1)
                    {
                        playerHitbox.Body.ActivatePower(onHitEffect.PType, onHitEffect.PIntensity, onHitEffect.PDuration, onHitEffect.PIsPuke, onHitEffect.PIsInverted);
                        Component.Destroy(effectTicker);
                    }
                }
            }

            damageable.Damage(damage);
        }
        public override void LoadMag(FVRFireArmMagazine mag)
        {
            base.LoadMag(mag);
            LaserWeaponPowerCell powerCell = mag as LaserWeaponPowerCell;
            this.m_regenTimer = powerCell.TimeToRegenStart;
            this.SetIntensity();
            this.SetBeam(powerCell.LaserBeamPrefab);
        }
        private void SetIntensity()
        {
            if (this.Magazine != null)
            {
                LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
                if (powerCell == null)
                {
                    this.AudSource_B.volume = 0.2f;
                    this.AudSource_B.pitch = 0.85f;
                    return;
                }
                if (powerCell.powerLevel.PL == sblpCell.PLevel.Low)
                {
                    this.AudSource_B.volume = 0.2f;
                    this.AudSource_B.pitch = 0.85f;
                }
                else if (powerCell.powerLevel.PL == sblpCell.PLevel.Medium)
                {
                    this.AudSource_B.volume = 0.3f;
                    this.AudSource_B.pitch = 1f;
                }
                else if (powerCell.powerLevel.PL == sblpCell.PLevel.High)
                {
                    this.AudSource_B.volume = 0.4f;
                    this.AudSource_B.pitch = 1.3f;
                }
            }
            else
            {
                this.AudSource_B.volume = 0.2f;
                this.AudSource_B.pitch = 0.85f;
            }
        }
        public void ResetCamBurst()
        {
            LaserWeapon.FireSelectorMode fireSelectorMode = this.FireSelectorModes[this.m_fireSelectorMode];
            this.m_camBurst = fireSelectorMode.BurstAmount;
            this.m_engagementDelay = 0f;
        }
        private void ToggleFireSelector()
        {
            if (this.FireSelectorModes.Length > 1)
            {
                this.m_fireSelectorMode++;
                if (this.m_fireSelectorMode >= this.FireSelectorModes.Length)
                {
                    this.m_fireSelectorMode -= this.FireSelectorModes.Length;
                }
                LaserWeapon.FireSelectorMode fireSelectorMode = this.FireSelectorModes[this.m_fireSelectorMode];
                if (this.m_triggerFloat < 0.1f)
                {
                    this.ResetCamBurst();
                }
                base.PlayAudioEvent(FirearmAudioEventType.FireSelector, 1f);
                if (this.FireSelectorSwitch != null)
                {
                    this.SetAnimatedComponent(this.FireSelectorSwitch, fireSelectorMode.SelectorPosition, this.FireSelectorInterpStyle, this.FireSelectorAxis);
                }
            }
            this.ResetCamBurst();
        }
        private void TryToEngageBeam()
        {
            if (this.m_isBeamEngaged) return;
            this.PSys_Engage.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
            this.PSys_Disengage.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
            this.AudSource_B.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
            if (this.CanOverheat && this.m_isOverheated)
            {
                if (!this.m_hasErrored)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Err, base.transform.position);
                    this.m_hasErrored = true;
                }
                return;
            }
            if (this.HasHeatsink && this.Heatsink == null)
            {
                if (!this.m_hasErrored)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Err, base.transform.position);
                    this.m_hasErrored = true;
                }
                return;
            }
            if (this.Magazine == null)
            {
                if (!this.m_hasErrored)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Err, base.transform.position);
                    this.m_hasErrored = true;
                }
                return;
            }
            LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
            if (powerCell == null)
            {
                if (!this.m_hasErrored)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Err, base.transform.position);
                    this.m_hasErrored = true;
                }
                return;
            }
            float f = 0.25f;
            if (powerCell.powerLevel.PL == sblpCell.PLevel.Medium)
            {
                f = 1f;
            }
            else if (powerCell.powerLevel.PL == sblpCell.PLevel.High)
            {
                f = 3f;
            }
            f *= powerCell.powerLevel.PowerCoefficient;
            if (!this.Magazine.HasFuel(f))
            {
                if (!this.m_hasErrored)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Err, base.transform.position);
                    this.m_hasErrored = true;
                }
                return;
            }
            this.m_timeToEnergyConsumption = 0.1f;
            this.m_isBeamEngaged = true;
            base.PlayAudioAsHandling(this.AudEvent_Start[(int)powerCell.powerLevel.PL], this.GetMuzzle().position);
            this.PSys_Engage.Emit(20);
            this.AudSource_B.Play();
        }
        private void TryToDisengageBeam()
        {
            if (this.m_isBeamEngaged)
            {
                this.PSys_Engage.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
                this.PSys_Disengage.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
                this.AudSource_B.gameObject.transform.SetParent(this.GetMuzzle().transform, false);
                this.m_isBeamEngaged = false;
                this.m_energyConsumed = 0;
                this.AudSource_B.Stop();
                this.PSys_Disengage.Emit(20);
                for (int i = 0; i < this.ImpactPoints.Count; i++)
                {
                    this.ImpactPoints[i].Geo.SetActive(false);
                    var emission = this.ImpactPoints[i].PSys.emission;
                    emission.enabled = false;
                    this.Beams[i].gameObject.SetActive(false);
                }
                if (this.Magazine == null) return;
                LaserWeaponPowerCell powerCell = this.Magazine as LaserWeaponPowerCell;
                float f = 0.25f;
                if (powerCell.powerLevel.PL == sblpCell.PLevel.Medium)
                {
                    f = 1f;
                }
                else if (powerCell.powerLevel.PL == sblpCell.PLevel.High)
                {
                    f = 3f;
                }
                f *= powerCell.powerLevel.PowerCoefficient;
                if (this.Magazine.HasFuel(f))
                {
                    if (!this.Magazine.IsInfinite) this.Magazine.DrainFuel(f);
                }
                base.PlayAudioAsHandling(this.AudEvent_End[(int)powerCell.powerLevel.PL], this.GetMuzzle().position);
            }
        }
        [ContextMenu("Copy Existing Physical Object Component")]
        public void CopyPhysicalObject()
        {
            FVRFireArm fireArm = base.GetComponents<FVRFireArm>().Single((FVRFireArm fa) => fa != this);
            FVRPhysicalObject physicalObject = fireArm as FVRPhysicalObject;
            FVRPhysicalObject physicalObject1 = this as FVRPhysicalObject;
            physicalObject1.CopyComponent(physicalObject);
        }
    }
}
