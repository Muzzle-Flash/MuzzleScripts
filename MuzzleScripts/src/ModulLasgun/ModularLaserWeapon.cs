using FistVR;
using ModularWorkshop;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenScripts2;

namespace MuzzleScripts
{
    public class ModularLaserWeapon : LaserWeapon, IModularWeapon
    {
        [Header("Modular Configuration")]
        public ModularFVRFireArm ModularFVRFireArm;
        public bool AllowExternalBoltReleaseButtonModification = true;

        [ContextMenu("Copy Existing ModularFirearm Component")]
        public void CopyModularWeapon()
        {
            IModularWeapon modularWeapon = base.GetComponents<IModularWeapon>().Single((IModularWeapon mw) => (object)mw != this);
            ModularFVRFireArm modularFVRFireArm = modularWeapon.GetModularFVRFireArm;
            ModularWeaponPartsAttachmentPoint barrelAttachmentPoint = modularFVRFireArm.ModularBarrelAttachmentPoint;
            if (barrelAttachmentPoint.ModularPartsGroupID != null)
            {
                this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartsGroupID = barrelAttachmentPoint.ModularPartsGroupID;
                this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartPoint = barrelAttachmentPoint.ModularPartPoint;
                this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartUIPoint = barrelAttachmentPoint.ModularPartUIPoint;
                this.ModularFVRFireArm.ModularBarrelAttachmentPoint.SelectedModularWeaponPart = barrelAttachmentPoint.SelectedModularWeaponPart;
                this.ModularFVRFireArm.ModularBarrelAttachmentPoint.DisallowTakeAndHoldRandomization = barrelAttachmentPoint.DisallowTakeAndHoldRandomization;
            }
            ModularWeaponPartsAttachmentPoint handguardAttachmentPoint = modularFVRFireArm.ModularHandguardAttachmentPoint;
            if (handguardAttachmentPoint.ModularPartsGroupID != null)
            {
                this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartsGroupID = handguardAttachmentPoint.ModularPartsGroupID;
                this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartPoint = handguardAttachmentPoint.ModularPartPoint;
                this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartUIPoint = handguardAttachmentPoint.ModularPartUIPoint;
                this.ModularFVRFireArm.ModularHandguardAttachmentPoint.SelectedModularWeaponPart = handguardAttachmentPoint.SelectedModularWeaponPart;
                this.ModularFVRFireArm.ModularHandguardAttachmentPoint.DisallowTakeAndHoldRandomization = handguardAttachmentPoint.DisallowTakeAndHoldRandomization;
            }
            ModularWeaponPartsAttachmentPoint stockAttachmentPoint = modularFVRFireArm.ModularStockAttachmentPoint;
            if (stockAttachmentPoint.ModularPartsGroupID != null)
            {
                this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartsGroupID = stockAttachmentPoint.ModularPartsGroupID;
                this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartPoint = stockAttachmentPoint.ModularPartPoint;
                this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartUIPoint = stockAttachmentPoint.ModularPartUIPoint;
                this.ModularFVRFireArm.ModularStockAttachmentPoint.SelectedModularWeaponPart = stockAttachmentPoint.SelectedModularWeaponPart;
                this.ModularFVRFireArm.ModularStockAttachmentPoint.DisallowTakeAndHoldRandomization = stockAttachmentPoint.DisallowTakeAndHoldRandomization;
            }
            this.ModularFVRFireArm.ModularWeaponPartsAttachmentPoints = (from pap in modularFVRFireArm.ModularWeaponPartsAttachmentPoints where pap != null select pap).ToArray();
            //foreach (ModularWeaponPartsAttachmentPoint partsAttachmentPoint in ModularFVRFireArm.ModularWeaponPartsAttachmentPoints)
            //{
            //    ModularWeaponPartsAttachmentPoint newPartAttachmentPoint = new();
            //    newPartAttachmentPoint.ModularPartsGroupID = partsAttachmentPoint.ModularPartsGroupID;
            //    newPartAttachmentPoint.ModularPartPoint = partsAttachmentPoint.ModularPartPoint;
            //    newPartAttachmentPoint.ModularPartUIPoint = partsAttachmentPoint.ModularPartUIPoint;
            //    newPartAttachmentPoint.SelectedModularWeaponPart = partsAttachmentPoint.SelectedModularWeaponPart;
            //    newPartAttachmentPoint.DisallowTakeAndHoldRandomization = partsAttachmentPoint.DisallowTakeAndHoldRandomization;
                
            //}
            if (modularFVRFireArm.ReceiverSkinUIPoint != null)
            {
                this.ModularFVRFireArm.ReceiverSkinUIPoint = modularFVRFireArm.ReceiverSkinUIPoint;
            }
        }

        [ContextMenu("Copy Existing Firearm Component")]
        public void CopyFirearm()
        {
            LaserWeapon laserWeapon = base.GetComponents<LaserWeapon>().Single((LaserWeapon lw) => lw != this);
            if (laserWeapon.Foregrip != null)
            {
                laserWeapon.Foregrip.GetComponent<FVRAlternateGrip>().PrimaryObject = this;
            }
            
            UniversalMagGrabTrigger componentInChildren = laserWeapon.GetComponentInChildren<UniversalMagGrabTrigger>();
            if (componentInChildren != null)
            {
                componentInChildren.FireArm = this;
            }
            FVRFireArmReloadTriggerWell componentInChildren2 = laserWeapon.GetComponentInChildren<FVRFireArmReloadTriggerWell>();
            if (componentInChildren2 != null)
            {
                componentInChildren2.FireArm = this;
            }
            laserWeapon.AttachmentMounts = (from mount in laserWeapon.AttachmentMounts where mount != null select mount).ToList<FVRFireArmAttachmentMount>();
            foreach (FVRFireArmAttachmentMount fvrfireArmAttachmentMount in laserWeapon.AttachmentMounts)
            {
                fvrfireArmAttachmentMount.MyObject = this;
                fvrfireArmAttachmentMount.Parent = this;
            }
            this.CopyComponent(laserWeapon);
        }
        [ContextMenu("Populate Receiver Mesh Renderer List")]
        public void PopulateReceiverMeshList()
        {
            FVRFireArm firearm = this as FVRFireArm;
            this.ModularFVRFireArm.GetReceiverMeshRenderers(firearm);
        }
        [ContextMenu("Update Selected Parts")]
        public void UpdateSelectedParts()
        {
            this.ModularFVRFireArm.UpdateSelectedParts();
        }
        public GameObject UIPrefab
        {
            get
            {
                return this.ModularFVRFireArm.UIPrefab;
            }
        }
        public string ModularBarrelPartsID
        {
            get
            {
                return this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartsGroupID;
            }
        }
        public Transform ModularBarrelPoint
        {
            get
            {
                return this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartPoint;
            }
        }
        public TransformProxy ModularBarrelUIPointProxy
        {
            get
            {
                return this.ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartUIPointProxy;
            }
        }
        public Dictionary<string, GameObject> ModularBarrelPrefabsDictionary
        {
            get
            {
                return this.ModularFVRFireArm.ModularBarrelPrefabsDictionary;
            }
        }
        public string ModularHandguardPartsID
        {
            get
            {
                return this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartsGroupID;
            }
        }
        public Transform ModularHandguardPoint
        {
            get
            {
                return this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartPoint;
            }
        }
        public TransformProxy ModularHandguardUIPointProxy
        {
            get
            {
                return this.ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartUIPointProxy;
            }
        }
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary
        {
            get
            {
                return this.ModularFVRFireArm.ModularHandguardPrefabsDictionary;
            }
        }
        public string ModularStockPartsID
        {
            get
            {
                return this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartsGroupID;
            }
        }
        public Transform ModularStockPoint
        {
            get
            {
                return this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartPoint;
            }
        }
        public TransformProxy ModularStockUIPointProxy
        {
            get
            {
                return this.ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartUIPointProxy;
            }
        }
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary
        {
            get
            {
                return this.ModularFVRFireArm.ModularStockPrefabsDictionary;
            }
        }
        public string SelectedModularBarrel
        {
            get
            {
                return this.ModularFVRFireArm.ModularBarrelAttachmentPoint.SelectedModularWeaponPart;
            }
        }
        public string SelectedModularHandguard
        {
            get
            {
                return this.ModularFVRFireArm.ModularHandguardAttachmentPoint.SelectedModularWeaponPart;
            }
        }
        public string SelectedModularStock
        {
            get
            {
                return this.ModularFVRFireArm.ModularStockAttachmentPoint.SelectedModularWeaponPart;
            }
        }
        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints
        {
            get
            {
                return this.ModularFVRFireArm.ModularWeaponPartsAttachmentPoints;
            }
        }
        public ModularWorkshopPlatform WorkshopPlatform
        {
            get
            {
                return this.ModularFVRFireArm.WorkshopPlatform;
            }
            set
            {
                this.ModularFVRFireArm.WorkshopPlatform = value;
            }
        }
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints
        {
            get
            {
                return this.ModularFVRFireArm.SubAttachmentPoints;
            }
        }
        public ModularFVRFireArm GetModularFVRFireArm
        {
            get
            {
                return this.ModularFVRFireArm;
            }
        }
        public Dictionary<string, ModularWeaponPartsAttachmentPoint> AllAttachmentPoints
        {
            get
            {
                return this.ModularFVRFireArm.AllAttachmentPoints;
            }
        }
        public override void Awake()
        {
            base.Awake();
            this.ConvertTransformsToProxies();
            this.ModularFVRFireArm.Awake(this);
            this.ConfigureAll();
        }
        public void ConvertTransformsToProxies()
        {
            this.ModularFVRFireArm.ConvertTransformsToProxies(this);
        }
        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);
            this.ModularFVRFireArm.ConfigureFromFlagDic(f, this);
        }
        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();
            return this.ModularFVRFireArm.GetFlagDic(flagDic);
        }
        public void ConfigureAll()
        {
            if (this.ModularBarrelPartsID != string.Empty)
            {
                string selectedPart = (this.ModularFVRFireArm.IsInTakeAndHold && !this.ModularFVRFireArm.WasUnvaulted && !this.ModularFVRFireArm.ModularBarrelAttachmentPoint.DisallowTakeAndHoldRandomization) ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[this.ModularBarrelPartsID].GetRandomPart() : this.SelectedModularBarrel;
                this.ConfigureModularBarrel(selectedPart, false);
            }
            if (this.ModularHandguardPartsID != string.Empty)
            {
                string selectedPart = (this.ModularFVRFireArm.IsInTakeAndHold && !this.ModularFVRFireArm.WasUnvaulted && !this.ModularFVRFireArm.ModularHandguardAttachmentPoint.DisallowTakeAndHoldRandomization) ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[this.ModularHandguardPartsID].GetRandomPart() : this.SelectedModularHandguard;
                this.ConfigureModularHandguard(selectedPart, false);
            }
            if (this.ModularStockPartsID != string.Empty)
            {
                string selectedPart = (this.ModularFVRFireArm.IsInTakeAndHold && !this.ModularFVRFireArm.WasUnvaulted && !this.ModularFVRFireArm.ModularStockAttachmentPoint.DisallowTakeAndHoldRandomization) ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[this.ModularStockPartsID].GetRandomPart() : this.SelectedModularStock;
                this.ConfigureModularStock(selectedPart, false);
            }
            foreach (ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint in this.ModularFVRFireArm.ModularWeaponPartsAttachmentPoints)
            {
                if (!modularWeaponPartsAttachmentPoint.IsPointDisabled)
                {
                    ModularWorkshopPartsDefinition modularWorkshopPartsDefinition;
                    if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out modularWorkshopPartsDefinition) && modularWorkshopPartsDefinition.PartsDictionary.Count > 0)
                    {
                        string selectedPart = (this.ModularFVRFireArm.IsInTakeAndHold && !this.ModularFVRFireArm.WasUnvaulted && !modularWeaponPartsAttachmentPoint.DisallowTakeAndHoldRandomization) ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[modularWeaponPartsAttachmentPoint.ModularPartsGroupID].GetRandomPart() : modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart;
                        this.ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, this.ModularFVRFireArm.IsInTakeAndHold);
                    }
                    else if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID) && modularWorkshopPartsDefinition.PartsDictionary.Count == 0)
                    {
                        ModularWorkshopManager.LogError(this, "PartsAttachmentPoint Error: Parts group \"" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID + "\" found in ModularWorkshopManager dictionary, but it is empty!");
                        modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                    }
                    else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID) && modularWeaponPartsAttachmentPoint.UsesExternalParts)
                    {
                        ModularWorkshopManager.Log(this, "PartsAttachmentPoint Info: Parts group \"" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID + "\" disabled due to using external parts and no external parts found.");
                        modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                    }
                    else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID))
                    {
                        ModularWorkshopManager.LogWarning(this, "PartsAttachmentPoint Warning: Parts group \"" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID + "\" not found in ModularWorkshopManager dictionary! Disabling part point!");
                        modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                    }
                }
            }
        }
        public void ApplySkin(string ModularPartsGroupID, string SkinName)
        {
            this.AllAttachmentPoints[ModularPartsGroupID].ApplySkin(SkinName);
        }
        public ModularBarrel ConfigureModularBarrel(string partName, bool isRandomized = false)
        {
            return this.ModularFVRFireArm.ConfigureModularBarrel(partName, isRandomized);
        }
        public ModularHandguard ConfigureModularHandguard(string partName, bool isRandomized = false)
        {
            return this.ModularFVRFireArm.ConfigureModularHandguard(partName, isRandomized);
        }
        public ModularStock ConfigureModularStock(string partName, bool isRandomized = false)
        {
            return this.ModularFVRFireArm.ConfigureModularStock(partName, isRandomized);
        }
        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string partName, bool isRandomized = false)
        {
            return this.ModularFVRFireArm.ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, partName, isRandomized, null, null);
        }
    }
}
