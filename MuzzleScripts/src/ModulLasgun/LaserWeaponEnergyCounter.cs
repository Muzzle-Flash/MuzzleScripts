using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MuzzleScripts
{
    public class LaserWeaponEnergyCounter : MonoBehaviour
    {
        [Header("Component Config")]
        public LaserWeapon LaserWeapon;
        public LaserWeaponPowerCell PowerCell;
        public FVRFireArmAttachment Attachment;

        [Header("Image Config")]
        public Image ProgressImage;
        public Image EmptyImage;

        [Header("Text Config")]
        public Text CurEnergyText;

        private float curEnergyLevel = 0f;
        private float maxEnergyLevel = 0f;
        private float energyLerp;
        [HideInInspector]
        public bool m_SetToZero;

        public void Awake()
        {
            if (this.ProgressImage != null && this.ProgressImage.type != Image.Type.Filled)
            {
                Debug.LogError($"{name}'s ProgressImage is not of type \"Filled\" so it cannot be used as a progress bar. Disabling this Energy Counter");
                this.enabled = false;
            }
        }
        public void Update()
        {
            this.UpdateEnergy();
            this.UpdateImage();
            this.UpdateText();
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
        public void UpdateEnergy()
        {
            
            LaserWeaponPowerCell powerCell = this.GetPowerCell();
            if (powerCell != null)
            {
                this.curEnergyLevel = powerCell.FuelAmountLeft;
                this.maxEnergyLevel = powerCell.MaximumEnergyCapacity;
                this.energyLerp = Mathf.InverseLerp(0f, this.maxEnergyLevel, this.curEnergyLevel);
                this.energyLerp = Mathf.Clamp01(this.energyLerp);
                if (this.energyLerp <= 0.01f)
                {
                    this.m_SetToZero = true;
                }
                else
                {
                    this.m_SetToZero = false;
                }
            }
            else
            {
                this.m_SetToZero = true;
                this.maxEnergyLevel = 0f;
                this.curEnergyLevel = 0f;
                this.energyLerp = 0f;
            }
        }
        public void UpdateImage()
        {
            if (this.ProgressImage == null) return;
            this.ProgressImage.fillAmount = this.energyLerp;
            if (this.m_SetToZero)
            {
                this.ProgressImage.enabled = false;
                if (this.EmptyImage != null)
                {
                    this.EmptyImage.enabled = true;
                }
            }
            else if (!this.m_SetToZero)
            {
                this.ProgressImage.enabled = true;
                if (this.EmptyImage != null)
                {
                    this.EmptyImage.enabled = false;
                }
            }
        }
        public void UpdateText()
        {
            if (this.CurEnergyText != null)
            {
                string energyPercent = ((int)Mathf.Clamp(Mathf.Lerp(0f, 100f, this.energyLerp), 0f, 100f)).ToString();
                int lengthToAdd = 2 - energyPercent.Length;
                if (lengthToAdd < 0) lengthToAdd = 0;
                for (int i = 0; i < lengthToAdd; i++)
                {
                    energyPercent = "0" + energyPercent;
                }
                energyPercent = energyPercent + "%";
                this.CurEnergyText.text = energyPercent;
            }
        }
    }
}
