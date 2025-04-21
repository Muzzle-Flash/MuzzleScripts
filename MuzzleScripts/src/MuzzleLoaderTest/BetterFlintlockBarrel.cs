using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class BetterFlintlockBarrel : FlintlockBarrel
    {
        private List<Renderer> _origProxyRends0;
        private List<Renderer> _origProxyRends1;

        [Serializable]
        public class NewLoadedElement : FlintlockBarrel.LoadedElement
        {
            public GameObject ProjectilePrefab;
            public Mesh LE_Mesh;
            public Material LE_Material;
            public GameObject LE_SelfPrefab;
        }
        public new void Awake()
        {
            base.Awake();
            this._origProxyRends0 = (from r in this.ProxyRends0 where r != null select r).ToList<Renderer>();
            this._origProxyRends1 = (from r in this.ProxyRends1 where r != null select r).ToList<Renderer>();
        }
        private new void Update()
        {
            if (this.TampRefire < 0.2f)
            {
                this.TampRefire += Time.deltaTime;
            }
            if (this.m_insertSoundRefire < 0.2f)
            {
                this.m_insertSoundRefire += Time.deltaTime;
            }
            if (this.m_isIgnited)
            {
                this.m_igniteTick -= Time.deltaTime;
                if (this.m_igniteTick <= 0f)
                {
                    this.Fire();
                }
            }
            else
            {
                this.BarrelContentsSim();
                this.BarrelContentsDraw();
            }
        }
        private void InsertElement(MuzzleLoadedObject loadedObject)
        {
            MuzzleLoadedElement element = loadedObject.Element;
            if (element == null) return;
            BetterFlintlockBarrel.NewLoadedElement loadedElement = new BetterFlintlockBarrel.NewLoadedElement();
            loadedElement.Type = FlintlockBarrel.LoadedElementType.Shot;
            loadedElement.Position = 0f;
            loadedElement.PowderAmount = 1;
            loadedElement.LE_Mesh = element.Meshes[0];
            loadedElement.LE_Material = element.Material;
            loadedElement.ProjectilePrefab = element.ProjectilePrefab;
            loadedElement.LE_SelfPrefab = loadedObject.ObjectWrapper.GetGameObject();
            this.LoadedElements.Add(loadedElement);
        }

        public new void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null) return;
            float muzzleAngle = Vector3.Angle(this.Muzzle.forward, Vector3.up);
            if (muzzleAngle > 90f) return;
            GameObject gameObject = other.attachedRigidbody.gameObject;
            MuzzleLoadedObject muzzleLoadedObject = gameObject.GetComponent<MuzzleLoadedObject>();
            if (gameObject.CompareTag("flintlock_shot"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.Shot)) return;
                if (this.m_weapon.RamRod.gameObject.activeSelf && this.m_weapon.RamRod.RState == FlintlockPseudoRamRod.RamRodState.Barrel && this.m_weapon.RamRod.GetCurBarrel() == this) return;
                if (this.IsBarrelPlugged()) return;

                this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[1], this.Muzzle.position);
                if (muzzleLoadedObject != null && muzzleLoadedObject.Element != null)
                {
                    if (muzzleLoadedObject.Element.Type != MuzzleLoadedElement.MuzzleLoadedElementType.Ball) return;
                    this.InsertElement(muzzleLoadedObject);
                }
                else
                {
                    this.InsertElement(FlintlockBarrel.LoadedElementType.Shot);
                }
                UnityEngine.Object.Destroy(other.gameObject);
            }
            else if (gameObject.CompareTag("flintlock_paper"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.ShotInPaper)) return;
                if (this.m_weapon.RamRod.gameObject.activeSelf && this.m_weapon.RamRod.RState == FlintlockPseudoRamRod.RamRodState.Barrel && this.m_weapon.RamRod.GetCurBarrel() == this) return;
                if (this.IsBarrelPlugged()) return;
                if (Vector3.Angle(this.Muzzle.forward, gameObject.transform.forward) > 80f) return;

                FlintlockPaperCartridge cartridge = gameObject.GetComponent<FlintlockPaperCartridge>();
                if (cartridge.CState == FlintlockPaperCartridge.CartridgeState.Whole) return;
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[2], this.Muzzle.position);
                for (int i = 0; i < cartridge.numPowderChunksLeft; i++)
                {
                    this.InsertElement(FlintlockBarrel.LoadedElementType.Powder);
                }
                this.InsertElement(FlintlockBarrel.LoadedElementType.ShotInPaper);
                UnityEngine.Object.Destroy(other.gameObject);
            }
            else if (gameObject.CompareTag("flintlock_powdergrain"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.Powder))
                {
                    return;
                }
                if (this.m_insertSoundRefire > 0.15f)
                {
                    this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[0], this.Muzzle.position);
                }
                this.InsertElement(FlintlockBarrel.LoadedElementType.Powder);
                UnityEngine.Object.Destroy(other.gameObject);
            }
            else if (gameObject.CompareTag("flintlock_ramrod"))
            {
                FlintlockRamRod component2 = gameObject.GetComponent<FlintlockRamRod>();
                if (!component2.IsHeld)
                {
                    return;
                }
                FVRViveHand hand = component2.m_hand;
                component2.ForceBreakInteraction();
                this.m_weapon.RamRod.gameObject.SetActive(true);
                this.m_weapon.RamRod.RState = FlintlockPseudoRamRod.RamRodState.Barrel;
                this.m_weapon.RamRod.MountToBarrel(this, hand);
                this.Tamp(0.05f, 0.001f);
                hand.ForceSetInteractable(this.m_weapon.RamRod);
                this.m_weapon.RamRod.BeginInteraction(hand);
                UnityEngine.Object.Destroy(other.gameObject);
            }
        }
        private List<FlintlockBarrel.LoadedElement> GetProjectilesInBarrel()
        {
            List<FlintlockBarrel.LoadedElement> loadedProjectiles = (from e in this.LoadedElements where e.Type == FlintlockBarrel.LoadedElementType.Shot || e.Type == FlintlockBarrel.LoadedElementType.ShotInPaper select e).ToList<FlintlockBarrel.LoadedElement>();
            return loadedProjectiles;
        }
        private new void Fire()
        {
            if (this.LoadedElements.Count > 0 && this.LoadedElements[0].Type != FlintlockBarrel.LoadedElementType.Powder)
            {
                this.m_isIgnited = false;
                return;
            }
            bool flag = false;
            bool flag2 = false;
            int numProjectilesInBarrel = this.GetNumProjectilesInBarrel();
            List<FlintlockBarrel.LoadedElement> loadedProjectiles = this.GetProjectilesInBarrel();
            int numPowder = this.GetNumPowder();
            float num = Mathf.Lerp(0.2f, 3.1f, (float)numPowder / 60f);
            float t = Mathf.Lerp(0f, 1f, (float)numPowder / 60f);
            float num2 = this.Spread;
            float num3 = this.PowderToVelMultCurve.Evaluate((float)numPowder);
            if (numProjectilesInBarrel > 0)
            {
                num3 *= 1f / (float)numProjectilesInBarrel;
            }
            else
            {
                num = 0.05f;
            }
            if (num > 3f)
            {
                flag2 = true;
            }
            if (numProjectilesInBarrel > 3 && numPowder > 15)
            {
                flag = true;
            }
            if (numProjectilesInBarrel > 0 && numPowder > 100)
            {
                flag = true;
            }
            num2 += this.Spread * 0.2f * (float)(numProjectilesInBarrel - 1);
            if (flag)
            {
                num2 *= 5f;
            }
            float num4 = Mathf.Lerp(this.MuzzleOverFireSystemScaleRange.x, this.MuzzleOverFireSystemScaleRange.y, t);
            float num5 = Mathf.Lerp(this.MuzzleOverFireSystemEmitRange.x, this.MuzzleOverFireSystemEmitRange.y, t);
            this.MuzzleOverFireSystem.transform.localEulerAngles = new Vector3(num4, num4, num4);
            this.MuzzleOverFireSystem.Emit(Mathf.RoundToInt(num5));
            this.m_pan.FlashBlast(Mathf.RoundToInt(num5) * 2, Mathf.RoundToInt(num5) * 2);
            int num6 = 3 * numProjectilesInBarrel;
            if (numProjectilesInBarrel > 0 && numPowder < num6)
            {
                this.LoadedElements.RemoveAt(0);
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_Squib, this.m_pan.transform.position);
                this.m_isIgnited = false;
                return;
            }
            this.FireMuzzleSmoke();
            this.m_weapon.Fire(num);
            if (this.m_weapon.RamRod.GetCurBarrel() == this)
            {
                this.m_weapon.RamRod.gameObject.SetActive(false);
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_weapon.RamRodProj, this.Muzzle.position, this.Muzzle.rotation);
                gameObject.GetComponent<BallisticProjectile>().Fire(this.Muzzle.forward, this.m_weapon);
                this.m_weapon.RamRod.GameObject.SetActive(false);
                this.m_weapon.RamRod.MountToBarrel(null, null);
            }
            int num7 = 0;
            while ((float)num7 < num5)
            {
                Vector3 b = this.Muzzle.forward * 0.005f;
                GameObject original = this.IgniteProjectile_Visible;
                if (numProjectilesInBarrel > 0)
                {
                    original = this.IgniteProjectile_NotVisible;
                }
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original, this.Muzzle.position - b, this.Muzzle.rotation);
                gameObject2.transform.Rotate(new Vector3(UnityEngine.Random.Range(-num2 * 2f, num2 * 2f), UnityEngine.Random.Range(-num2 * 2f, num2 * 2f), 0f));
                BallisticProjectile component = gameObject2.GetComponent<BallisticProjectile>();
                component.Fire(component.MuzzleVelocityBase, gameObject2.transform.forward, this.m_weapon, true);
                num7++;
            }
            if (flag)
            {
                this.m_weapon.PlayAudioGunShot(true, FVRTailSoundClass.Explosion, FVRTailSoundClass.SuppressedLarge, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
            }
            else if (numProjectilesInBarrel > 0)
            {
                this.m_weapon.PlayAudioGunShot(true, FVRTailSoundClass.Shotgun, FVRTailSoundClass.SuppressedLarge, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
            }
            else
            {
                this.m_weapon.PlayAudioGunShot(true, FVRTailSoundClass.Launcher, FVRTailSoundClass.SuppressedLarge, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
            }
            float min = 1f - this.GetMinProjPos() / this.BarrelLength;
            float max = 1f - this.GetMaxProjPos() / this.BarrelLength;
            float num8 = 1f * UnityEngine.Random.Range(min, max);
            num2 += num8;
            for (int i = 0; i < loadedProjectiles.Count; i++)
            {
                BetterFlintlockBarrel.NewLoadedElement newElement = loadedProjectiles[i] as NewLoadedElement;
                if (newElement != null)
                {
                    Vector3 b2 = this.Muzzle.forward * 0.005f;
                    GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(newElement.ProjectilePrefab, this.Muzzle.position - b2, this.Muzzle.rotation);
                    gameObject3.transform.Rotate(new Vector3(UnityEngine.Random.Range(-num2, num2), UnityEngine.Random.Range(-num2, num2), 0f));
                    BallisticProjectile component2 = gameObject3.GetComponent<BallisticProjectile>();
                    component2.Fire(component2.MuzzleVelocityBase * num3 * this.VelocityMult, gameObject3.transform.forward, this.m_weapon, true);
                }
                else
                {
                    Vector3 b2 = this.Muzzle.forward * 0.005f;
                    GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(this.ProjectilePrefab, this.Muzzle.position - b2, this.Muzzle.rotation);
                    gameObject3.transform.Rotate(new Vector3(UnityEngine.Random.Range(-num2, num2), UnityEngine.Random.Range(-num2, num2), 0f));
                    BallisticProjectile component2 = gameObject3.GetComponent<BallisticProjectile>();
                    component2.Fire(component2.MuzzleVelocityBase * num3 * this.VelocityMult, gameObject3.transform.forward, this.m_weapon, true);
                }
                
            }
            this.ClearBarrel();
            if (flag2)
            {
                this.m_weapon.ForceBreakInteraction();
                this.m_weapon.RootRigidbody.velocity = this.m_weapon.transform.forward * -8f + this.m_weapon.transform.up * 1f;
                this.m_weapon.RootRigidbody.angularVelocity = this.m_weapon.transform.right * -5f;
            }
            if (flag)
            {
                this.m_weapon.Blowup();
            }
            this.m_isIgnited = false;
        }
        private new void BarrelContentsDraw()
        {
            if (this.LoadedElements.Count > 1)
            {
                int index = this.LoadedElements.Count - 1;
                int index2 = this.LoadedElements.Count - 2;
                for (int i = 0; i < this.ProxyRends0.Count; i++)
                {
                    if (this.LoadedElements[index].Type == (FlintlockBarrel.LoadedElementType)i)
                    {
                        this.ProxyRends0[i].enabled = true;
                        this.ProxyRends0[i].transform.position = this.Muzzle.position - this.Muzzle.forward * this.LoadedElements[index].Position;
                        if (this.LoadedElements[index].Type != FlintlockBarrel.LoadedElementType.Powder && this.LoadedElements[index].Position < 0.01f)
                        {
                            if (this.LoadedElements[index].Type == FlintlockBarrel.LoadedElementType.Shot)
                            {
                                BetterFlintlockBarrel.NewLoadedElement newLoadedElement = this.LoadedElements[index] as BetterFlintlockBarrel.NewLoadedElement;
                                if (newLoadedElement != null)
                                {
                                    this.ProxyRends0[i].material = newLoadedElement.LE_Material;
                                    this.ProxyRends0[i].GetComponent<MeshFilter>().mesh = newLoadedElement.LE_Mesh;
                                }
                                else
                                {
                                    this.ProxyRends0[i].material = this._origProxyRends0[i].material;
                                    this.ProxyRends0[i].GetComponent<MeshFilter>().mesh = this._origProxyRends0[i].GetComponent<MeshFilter>().mesh;
                                }
                                this.ProxyRends0[i].transform.position = this.LodgePoint_Shot.position;
                            }
                            else if (this.LoadedElements[index].Type == FlintlockBarrel.LoadedElementType.ShotInPaper)
                            {
                                this.ProxyRends0[i].transform.position = this.LodgePoint_Paper.position;
                            }
                        }
                        if (this.LoadedElements[index].Type == FlintlockBarrel.LoadedElementType.Powder)
                        {
                            int powderAmount = this.LoadedElements[index].PowderAmount;
                            if (powderAmount > 15)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[3];
                            }
                            else if (powderAmount > 9)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[2];
                            }
                            else if (powderAmount > 4)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[1];
                            }
                            else
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[0];
                            }
                        }
                    }
                    else
                    {
                        this.ProxyRends0[i].enabled = false;
                    }
                }
                for (int j = 0; j < this.ProxyRends1.Count; j++)
                {
                    if (this.LoadedElements[index2].Type == (FlintlockBarrel.LoadedElementType)j)
                    {
                        this.ProxyRends1[j].enabled = true;
                        this.ProxyRends1[j].transform.position = this.Muzzle.position - this.Muzzle.forward * this.LoadedElements[index2].Position;
                        if (this.LoadedElements[index2].Type == FlintlockBarrel.LoadedElementType.Powder)
                        {
                            int powderAmount2 = this.LoadedElements[index2].PowderAmount;
                            if (powderAmount2 > 15)
                            {
                                this.ProxyPowder1.mesh = this.ProxyPowderPiles[3];
                            }
                            else if (powderAmount2 > 9)
                            {
                                this.ProxyPowder1.mesh = this.ProxyPowderPiles[2];
                            }
                            else if (powderAmount2 > 4)
                            {
                                this.ProxyPowder1.mesh = this.ProxyPowderPiles[1];
                            }
                            else
                            {
                                this.ProxyPowder1.mesh = this.ProxyPowderPiles[0];
                            }
                        }
                    }
                    else
                    {
                        this.ProxyRends1[j].enabled = false;
                    }
                }
            }
            else if (this.LoadedElements.Count > 0)
            {
                for (int k = 0; k < this.ProxyRends0.Count; k++)
                {
                    if (this.LoadedElements[0].Type == (FlintlockBarrel.LoadedElementType)k)
                    {
                        this.ProxyRends0[k].enabled = true;
                        this.ProxyRends0[k].transform.position = this.Muzzle.position - this.Muzzle.forward * this.LoadedElements[0].Position;
                        if (this.LoadedElements[0].Type != FlintlockBarrel.LoadedElementType.Powder && this.LoadedElements[0].Position < 0.01f)
                        {
                            if (this.LoadedElements[0].Type == FlintlockBarrel.LoadedElementType.Shot)
                            {
                                BetterFlintlockBarrel.NewLoadedElement newLoadedElement = this.LoadedElements[0] as BetterFlintlockBarrel.NewLoadedElement;
                                if (newLoadedElement != null)
                                {
                                    this.ProxyRends0[k].material = newLoadedElement.LE_Material;
                                    this.ProxyRends0[k].GetComponent<MeshFilter>().mesh = newLoadedElement.LE_Mesh;
                                }
                                else
                                {
                                    this.ProxyRends0[k].material = this._origProxyRends0[k].material;
                                    this.ProxyRends0[k].GetComponent<MeshFilter>().mesh = this._origProxyRends0[k].GetComponent<MeshFilter>().mesh;
                                }
                                this.ProxyRends0[k].transform.position = this.LodgePoint_Shot.position;
                            }
                            else if (this.LoadedElements[0].Type == FlintlockBarrel.LoadedElementType.ShotInPaper)
                            {
                                this.ProxyRends0[k].transform.position = this.LodgePoint_Paper.position;
                            }
                        }
                        if (this.LoadedElements[0].Type == FlintlockBarrel.LoadedElementType.Powder)
                        {
                            int powderAmount3 = this.LoadedElements[0].PowderAmount;
                            if (powderAmount3 > 15)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[3];
                            }
                            else if (powderAmount3 > 9)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[2];
                            }
                            else if (powderAmount3 > 4)
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[1];
                            }
                            else
                            {
                                this.ProxyPowder0.mesh = this.ProxyPowderPiles[0];
                            }
                        }
                    }
                    else
                    {
                        this.ProxyRends0[k].enabled = false;
                    }
                }
                for (int l = 0; l < this.ProxyRends1.Count; l++)
                {
                    this.ProxyRends1[l].enabled = false;
                }
            }
            else
            {
                for (int m = 0; m < this.ProxyRends0.Count; m++)
                {
                    this.ProxyRends0[m].enabled = false;
                }
                for (int n = 0; n < this.ProxyRends1.Count; n++)
                {
                    this.ProxyRends1[n].enabled = false;
                }
            }
        }
        private int ExpellElement(BetterFlintlockBarrel.NewLoadedElement loadedElement)
        {
            FlintlockBarrel.LoadedElementType type = loadedElement.Type;
            int PowderAmount = loadedElement.PowderAmount;
            if (type != FlintlockBarrel.LoadedElementType.Powder)
            {
                Vector3 position = this.Muzzle.position + this.Muzzle.forward * this.GetLengthOfElement(type, PowderAmount) * 0.75f;
                UnityEngine.Object.Instantiate<GameObject>(loadedElement.LE_SelfPrefab, position, this.Muzzle.rotation);
                return 0;
            }
            int result = PowderAmount - 1;
            Vector3 position2 = this.Muzzle.position + UnityEngine.Random.onUnitSphere * 0.005f + this.Muzzle.forward * this.GetLengthOfElement(type, 1) * 0.75f;
            UnityEngine.Object.Instantiate<GameObject>(this.EjectedObjectPrefabs[(int)type], position2, this.Muzzle.rotation);
            return result;
        }
        private new void BarrelContentsSim()
        {
            float num = Vector3.Angle(this.Muzzle.forward, Vector3.up);
            if (num > 90f)
            {
                if (this.LoadedElements.Count > 0)
                {
                    for (int i = this.LoadedElements.Count - 1; i >= 0; i--)
                    {
                        if (this.LoadedElements[i].Type == FlintlockBarrel.LoadedElementType.Powder)
                        {
                            float min = 0f;
                            float max = this.BarrelLength;
                            bool flag = true;
                            if (i + 1 < this.LoadedElements.Count)
                            {
                                min = this.LoadedElements[i + 1].Position + this.GetLengthOfElement(this.LoadedElements[i + 1].Type, this.LoadedElements[i + 1].PowderAmount);
                                flag = false;
                            }
                            if (i - 1 >= 0)
                            {
                                max = this.LoadedElements[i - 1].Position;
                            }
                            float position = this.LoadedElements[i].Position;
                            float num2 = (num - 90f) / 90f * 2f;
                            float num3 = position - num2 * Time.deltaTime;
                            if (flag && num3 <= 0f)
                            {
                                int num4;
                                BetterFlintlockBarrel.NewLoadedElement newLoadedElement = this.LoadedElements[i] as BetterFlintlockBarrel.NewLoadedElement;
                                if (newLoadedElement != null)
                                {
                                    num4 = this.ExpellElement(newLoadedElement);
                                }
                                num4 = this.ExpellElement(this.LoadedElements[i].Type, this.LoadedElements[i].PowderAmount);
                                if (num4 <= 0)
                                {
                                    this.LoadedElements.RemoveAt(i);
                                }
                                else
                                {
                                    this.LoadedElements[i].PowderAmount = num4;
                                }
                            }
                            else
                            {
                                num3 = Mathf.Clamp(num3, min, max);
                                this.LoadedElements[i].Position = num3;
                            }
                        }
                    }
                }
            }
            else if (this.LoadedElements.Count > 0)
            {
                for (int j = this.LoadedElements.Count - 1; j >= 0; j--)
                {
                    if (this.LoadedElements[j].Type == FlintlockBarrel.LoadedElementType.Powder)
                    {
                        float min2 = 0f;
                        float max2 = this.BarrelLength - this.GetLengthOfElement(this.LoadedElements[j].Type, this.LoadedElements[j].PowderAmount);
                        if (j + 1 < this.LoadedElements.Count)
                        {
                            min2 = this.LoadedElements[j + 1].Position + this.GetLengthOfElement(this.LoadedElements[j + 1].Type, this.LoadedElements[j + 1].PowderAmount);
                        }
                        if (j - 1 >= 0)
                        {
                            max2 = this.LoadedElements[j - 1].Position;
                        }
                        float position2 = this.LoadedElements[j].Position;
                        float num5 = (1f - num / 90f) * 2f;
                        float num6 = position2 + num5 * Time.deltaTime;
                        num6 = Mathf.Clamp(num6, min2, max2);
                        this.LoadedElements[j].Position = num6;
                    }
                }
            }
        }
        [ContextMenu("Copy From Barrel")]
        public void CopyFromBarrel()
        {
            FlintlockBarrel orig = this.GetComponent<FlintlockBarrel>();
            this.Muzzle = orig.Muzzle;
            this.LodgePoint_Paper = orig.LodgePoint_Paper;
            this.LodgePoint_Shot = orig.LodgePoint_Shot;
            this.BarrelLength = orig.BarrelLength;
            this.ProxyRends0 = orig.ProxyRends0;
            this.ProxyRends1 = orig.ProxyRends1;
            this.ProxyPowder0 = orig.ProxyPowder0;
            this.ProxyPowder1 = orig.ProxyPowder1;
            this.ProxyPowderPiles = orig.ProxyPowderPiles;
            this.EjectedObjectPrefabs = orig.EjectedObjectPrefabs;
            this.LoadedElementSizes = orig.LoadedElementSizes;
            this.ProjectilePrefab = orig.ProjectilePrefab;
            this.Spread = orig.Spread;
            this.VelocityMult = orig.VelocityMult;
            this.IgniteProjectile_Visible = orig.IgniteProjectile_Visible;
            this.IgniteProjectile_NotVisible = orig.IgniteProjectile_NotVisible;
            this.PowderToVelMultCurve = orig.PowderToVelMultCurve;
            this.AudEvent_Tamp = orig.AudEvent_Tamp;
            this.AudEvent_TampEnd = orig.AudEvent_TampEnd;
            this.AudEvent_Squib = orig.AudEvent_Squib;
            this.AudEvent_InsertByType = orig.AudEvent_InsertByType;
            this.DefaultMuzzleEffectSize = orig.DefaultMuzzleEffectSize;
            this.MuzzleEffects = orig.MuzzleEffects;
            this.MuzzleOverFireSystem = orig.MuzzleOverFireSystem;
            this.MuzzleOverFireSystemScaleRange = orig.MuzzleOverFireSystemScaleRange;
            this.MuzzleOverFireSystemEmitRange = orig.MuzzleOverFireSystemEmitRange;
            this.FlashBlastSmokeRange = orig.FlashBlastSmokeRange;
            this.FlashBlastFireRange = orig.FlashBlastFireRange;
        }
    }
}
