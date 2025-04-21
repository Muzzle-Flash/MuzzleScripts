using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockFlintScrew :FVRInteractiveObject
    {
        public AttachableFlintlockWeapon Weapon;

        public Transform Screw;

        public FlintlockFlintScrew.ScrewState SState;

        private float lerp;

        public Vector2 Heights = new Vector2(0.05455612f, 0.06048f);

        public AudioEvent AudEvent_Screw;

        public AudioEvent AudEvent_Unscrew;

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.ToggleScrewState();
        }

        public override bool IsInteractable()
        {
            return this.SState != FlintlockFlintScrew.ScrewState.Screwing && this.SState != FlintlockFlintScrew.ScrewState.Unscrewing && base.IsInteractable();
        }

        private void ToggleScrewState()
        {
            if (this.SState == FlintlockFlintScrew.ScrewState.Screwed)
            {
                this.Weapon.PlayAudioAsHandling(this.AudEvent_Unscrew, base.transform.position);
                this.SState = FlintlockFlintScrew.ScrewState.Unscrewing;
                this.lerp = 0f;
            }
            else if (this.SState == FlintlockFlintScrew.ScrewState.Unscrewed)
            {
                this.Weapon.PlayAudioAsHandling(this.AudEvent_Screw, base.transform.position);
                this.SState = FlintlockFlintScrew.ScrewState.Screwing;
                this.lerp = 0f;
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (this.SState == FlintlockFlintScrew.ScrewState.Screwing)
            {
                this.lerp += Time.deltaTime * 1.4f;
                this.Screw.localPosition = new Vector3(this.Screw.localPosition.x, Mathf.Lerp(this.Heights.y, this.Heights.x, this.lerp), this.Screw.localPosition.z);
                this.Screw.localEulerAngles = new Vector3(90f, Mathf.Lerp(0f, 720f, this.lerp), 0f);
                if (this.lerp >= 1f)
                {
                    this.SState = FlintlockFlintScrew.ScrewState.Screwed;
                }
            }
            else if (this.SState == FlintlockFlintScrew.ScrewState.Unscrewing)
            {
                this.lerp += Time.deltaTime * 1.4f;
                this.Screw.localPosition = new Vector3(this.Screw.localPosition.x, Mathf.Lerp(this.Heights.x, this.Heights.y, this.lerp), this.Screw.localPosition.z);
                this.Screw.localEulerAngles = new Vector3(90f, Mathf.Lerp(720f, 0f, this.lerp), 0f);
                if (this.lerp >= 1f)
                {
                    this.SState = FlintlockFlintScrew.ScrewState.Unscrewed;
                }
            }
        }
    }
}
