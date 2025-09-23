using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;


namespace MuzzleScripts
{
    public class RevolverSecondaryBarrelSwitch : FVRInteractiveObject
    {
        [Header("Revolver Secondary Barrel Switch")]
        public RevolverSecondaryBarrel RevolverSecondaryBarrel;
        public Transform Switch;
        public FVRPhysicalObject.Axis SwitchAxis;
        public FVRPhysicalObject.InterpStyle SwitchInterpStyle;
        public float ToggledOnValue;
        public float ToggledOffValue;
        private bool m_IsToggledOn;
        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.RevolverSecondaryBarrel.IsSecondaryHammerEngaged = !this.RevolverSecondaryBarrel.IsSecondaryHammerEngaged;
            this.m_IsToggledOn = !this.m_IsToggledOn;
            this.RevolverSecondaryBarrel.revolver.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1.5f);
            if (this.m_IsToggledOn)
            {
                this.RevolverSecondaryBarrel.revolver.SetAnimatedComponent(this.Switch, this.ToggledOnValue, this.SwitchInterpStyle, this.SwitchAxis);
            }
            else
            {
                this.RevolverSecondaryBarrel.revolver.SetAnimatedComponent(this.Switch, this.ToggledOffValue, this.SwitchInterpStyle, this.SwitchAxis);
            }
        }
    }
}
