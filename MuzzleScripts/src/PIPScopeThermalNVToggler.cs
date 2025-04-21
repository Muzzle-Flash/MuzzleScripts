using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;

namespace MuzzleScripts.src
{
    public class PIPScopeThermalNVToggler : FVRInteractiveObject
    {
        public PIPScope pipScope;
        public bool TogglesIR;
        public bool TogglesNV;
        public bool TogglesThermal;

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            if (this.TogglesIR)
            {
                this.pipScope.enableIRFilter = !this.pipScope.enableIRFilter;
            }
            if (this.TogglesNV)
            {
                this.pipScope.enableNightvision = !this.pipScope.enableNightvision;
            }
            if (this.TogglesThermal)
            {
                this.pipScope.enableThermal = !this.pipScope.enableThermal;
            }
        }
    }
}
