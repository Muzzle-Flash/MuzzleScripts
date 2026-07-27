using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class LaserBeamPrefab : MonoBehaviour
    {
        [Header("VFX Config")]
        [Tooltip("The LineRenderer that makes up the laser beam itself.")]
        public LineRenderer LineRenderer;
        [Tooltip("The GameObject to be spawned whenever the laser collides with something.")]
        public GameObject ImpactPointPrefab;
        [Tooltip("The color used to indicate this beam type.")]
        public Color IndicatorColor = new Color(1f, 0.13f, 0f, 1f);
        [Tooltip("The name used to indicate this beam type.")]
        public String IndicatorText;

        [Header("Damage Config")]
        [Tooltip("Amount of Kinetic Damage dealt on impact, modified by power level.")]
        public float TotalKineticDamagePerHit = 50f;
        [Tooltip("Amount of Energy Damage dealt on impact, modified by power level.")]
        public float TotalEnergeticDamagePerHit = 100f;
        public bool IsStunningRay;
        public float StunDuration;
        public bool IsBlindingRay;
        public float BlindDuration;
        public bool IsEMPRay;
        public float EMPDisruptionDuration;
        public bool DoesIgniteOnHit;
        [Tooltip("Value from 0 to 1, the closer to 1, the less hits it takes to ignite the target. A value of 1 instantly ignites.")]
        [Range(0, 1)]
        public float IgnitionProgressPerHit;
        public bool DoesVaporizeOnHit;
        [Tooltip("Value from 0 to 1, the closer to 1, the less hits it takes to vaporize the target. A value of 1 instantly vaporizes.")]
        [Range(0, 1)]
        public float VaporizeProgressPerHit;
        [Tooltip("Disables Thermal Damage to not interfere with freezing debuff.")]
        public bool IsFreezingRay;
        public OnHitEffect[] OnHitEffects;

        [Serializable]
        public class OnHitEffect
        {
            [Tooltip("The Type of effect to be applied.")]
            public PowerupType PType;
            [Tooltip("The Intensity of the effect to be applied.")]
            public PowerUpIntensity PIntensity;
            [Tooltip("The Duration of the effect to be applied.")]
            public PowerUpDuration PDuration;
            [Tooltip("Whether or not the effect makes a sosig puke. Don't look at me, that's Anton's doing.")]
            public bool PIsPuke;
            [Tooltip("Inverts the effect. Turns a buff into a debuff and vice versa.")]
            public bool PIsInverted;
            [Tooltip("Value from 0 to 1, the closer to 1, the less hits it takes to apply the effect. A value of 1 instantly applies.")]
            [Range(0, 1)]
            public float EffectProgressPerHit;
        }
    }
}
