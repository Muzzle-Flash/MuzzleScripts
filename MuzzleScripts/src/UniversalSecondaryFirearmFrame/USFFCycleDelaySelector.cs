using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class USFFCycleDelaySelector : FVRInteractiveObject
    {
        public UniversalSecondaryFirearmFrame UniversalFrame;
        public Transform delaySelector;
        public FVRPhysicalObject.InterpStyle selector_InterpStyle = FVRPhysicalObject.InterpStyle.Rotation;
        public FVRPhysicalObject.Axis selector_Axis = FVRPhysicalObject.Axis.X;
        public USFFCycleDelaySelector.SelectorMode[] SelectorModes;
        private int curRateIndex = 0;

        [Serializable]
        public class SelectorMode
        {
            public float SelectorPosition;
            public float SelectorDelay;
        }

        public override void Awake()
        {
            base.Awake();
            this.UniversalFrame.cycleDelay = this.SelectorModes[this.curRateIndex].SelectorDelay;
        }

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.CycleMode();
        }

        private void CycleMode()
        {
            this.curRateIndex++;
            if (this.curRateIndex >= SelectorModes.Length)
            {
                this.curRateIndex = 0;
            }
            this.UniversalFrame.cycleDelay = this.SelectorModes[this.curRateIndex].SelectorDelay;
            this.UniversalFrame.PlayAudioAsHandling(this.UniversalFrame.SelectorAudio, this.UniversalFrame.transform.position);
            this.UniversalFrame.SetAnimatedComponent(this.delaySelector, this.SelectorModes[this.curRateIndex].SelectorPosition, this.selector_InterpStyle, this.selector_Axis);
        }
    }
}
