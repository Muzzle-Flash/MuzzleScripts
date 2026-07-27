using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MuzzleScripts
{
    class LaserBeamTypeIndicator : MonoBehaviour
    {
        [Header("Component Config")]
        public LaserWeapon LaserWeapon;
        public LaserWeaponPowerCell PowerCell;
        public FVRFireArmAttachment Attachment;

        [Header("Image Config")]
        public Image IndicatorImage;

        [Header("Text Config")]
        public Text IndicatorText;

        public void Update()
        {
            this.UpdateIndicator();
        }
        public LaserWeaponPowerCell GetPowerCell()
        {
            LaserWeaponPowerCell powerCell = null;
            if (this.LaserWeapon != null)
            {
                powerCell = this.LaserWeapon.Magazine as LaserWeaponPowerCell;
            }
            else if (this.Attachment != null)
            {
                LaserWeapon laserWeapon = this.Attachment.curMount.Parent as LaserWeapon;
                powerCell = laserWeapon.Magazine as LaserWeaponPowerCell;
            }
            else if (this.PowerCell != null)
            {
                powerCell = this.PowerCell;
            }
            return powerCell;

        }
        public void UpdateIndicator()
        {
            LaserWeaponPowerCell powerCell = this.GetPowerCell();
            Color IndicatorColor = new Color(1f, 1f, 1f, 1f);
            String IndicatorString = null;
            if (powerCell != null)
            {
                if (powerCell.LaserBeamPrefab != null)
                {
                    IndicatorColor = powerCell.LaserBeamPrefab.IndicatorColor;
                    IndicatorString = powerCell.LaserBeamPrefab.IndicatorText;
                }
            }
            if (this.IndicatorImage != null)
            {
                this.IndicatorImage.color = IndicatorColor;
            }
            if (this.IndicatorText != null)
            {
                this.IndicatorText.text = IndicatorString;
            }
        }
    }
}
