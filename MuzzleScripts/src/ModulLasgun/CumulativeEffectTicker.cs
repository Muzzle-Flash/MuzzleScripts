using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class CumulativeEffectTicker : MonoBehaviour
    {
        private float m_CumulativeEffectTick = 0;
        private float m_TimeToEffectDecay = 3f;
        private float m_EffectDecayAmount = 0.1f;
        private float m_EffectDecayTimer = 0f;
        public void Start()
        {
            this.m_EffectDecayTimer = 0f;
        }

        public float EffectProgress
        {
            get
            {
                return this.m_CumulativeEffectTick;
            }
            set
            {
                this.m_EffectDecayTimer = 0f;
                this.m_CumulativeEffectTick = value;
            }
        }

        public void Update()
        {
            if (this.m_EffectDecayTimer < this.m_TimeToEffectDecay)
            {
                this.m_EffectDecayTimer += Time.deltaTime;
            }
            if (this.m_EffectDecayTimer >= this.m_TimeToEffectDecay)
            {
                this.m_CumulativeEffectTick -= Time.deltaTime * m_EffectDecayAmount;
                if (this.m_CumulativeEffectTick <= 0)
                {
                    Component.Destroy(this);
                }
            }
        }
    }
}
