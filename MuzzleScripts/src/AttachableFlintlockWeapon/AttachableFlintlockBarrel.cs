using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockBarrel : MonoBehaviour
    {
        private AttachableFlintlockWeapon m_weapon;
        private AttachableFlintlockFlashPan m_pan;

        [Header("Barrel")]
        public Transform Muzzle;

        public Transform LodgePoint_Paper;

        public Transform LodgePoint_Shot;

        public float BarrelLength = 0.21f;

        public List<FlintlockBarrel.LoadedElement> LoadedElements = new List<FlintlockBarrel.LoadedElement>();

        public List<Renderer> ProxyRends0;

        public List<Renderer> ProxyRends1;

        public MeshFilter ProxyPowder0;

        public MeshFilter ProxyPowder1;

        public List<Mesh> ProxyPowderPiles;

        public List<GameObject> EjectedObjectPrefabs;

        public Vector4 LoadedElementSizes = new Vector4(0.001f, 0.0175f, 0.0195f, 0.01f);

        [Header("Projectile Stuff")]
        public GameObject ProjectilePrefab;

        public float Spread = 0.6f;

        public float VelocityMult = 1f;

        public GameObject IgniteProjectile_Visible;

        public GameObject IgniteProjectile_NotVisible;

        public AnimationCurve PowderToVelMultCurve;

        [Header("Audio")]
        public AudioEvent AudEvent_Tamp;

        public AudioEvent AudEvent_TampEnd;

        public AudioEvent AudEvent_Squib;

        public List<AudioEvent> AudEvent_InsertByType;

        private float m_insertSoundRefire = 0.2f;

        [Header("MuzzleEffects")]
        public MuzzleEffectSize DefaultMuzzleEffectSize = MuzzleEffectSize.Standard;

        public MuzzleEffect[] MuzzleEffects;

        private List<MuzzlePSystem> m_muzzleSystems = new List<MuzzlePSystem>();

        public ParticleSystem MuzzleOverFireSystem;

        public Vector2 MuzzleOverFireSystemScaleRange = new Vector2(0.3f, 2f);

        public Vector2 MuzzleOverFireSystemEmitRange = new Vector2(3f, 20f);

        public Vector2 FlashBlastSmokeRange = new Vector2(4f, 10f);

        public Vector2 FlashBlastFireRange = new Vector2(2f, 10f);

        private bool m_isIgnited;

        private float m_igniteTick;

        private float TampRefire = 0.2f;

        public AttachableFlintlockWeapon GetWeapon()
        {
            return this.m_weapon;
        }

        public void SetWeapon(AttachableFlintlockWeapon w)
        {
            this.m_weapon = w;
        }

        public void SetPan(AttachableFlintlockFlashPan p)
        {
            this.m_pan = p;
        }

        public float GetLengthOfElement(FlintlockBarrel.LoadedElementType Type, int PowderAmount)
        {
            switch (Type)
            {
                case FlintlockBarrel.LoadedElementType.Powder:
                    return this.LoadedElementSizes.x * (float)PowderAmount;
                case FlintlockBarrel.LoadedElementType.Shot:
                    return this.LoadedElementSizes.y;
                case FlintlockBarrel.LoadedElementType.ShotInPaper:
                    return this.LoadedElementSizes.z;
                case FlintlockBarrel.LoadedElementType.Wadding:
                    return this.LoadedElementSizes.w;
                default:
                    return 0f;
            }
        }

        private bool CanElementFit(FlintlockBarrel.LoadedElementType Type)
        {
            return this.LoadedElements.Count == 0 || this.LoadedElements[this.LoadedElements.Count - 1].Position > this.GetLengthOfElement(Type, 5);
        }

        private void Awake()
        {
            this.RegenerateMuzzleEffects();
        }

        private void RegenerateMuzzleEffects()
        {
            for (int i = 0; i < this.m_muzzleSystems.Count; i++)
            {
                UnityEngine.Object.Destroy(this.m_muzzleSystems[i].PSystem);
            }
            this.m_muzzleSystems.Clear();
            MuzzleEffect[] muzzleEffects = this.MuzzleEffects;
            for (int j = 0; j < muzzleEffects.Length; j++)
            {
                if (muzzleEffects[j].Entry != MuzzleEffectEntry.None)
                {
                    MuzzleEffectConfig muzzleConfig = FXM.GetMuzzleConfig(muzzleEffects[j].Entry);
                    MuzzleEffectSize size = muzzleEffects[j].Size;
                    GameObject gameObject;
                    if (GM.CurrentSceneSettings.IsSceneLowLight)
                    {
                        gameObject = UnityEngine.Object.Instantiate<GameObject>(muzzleConfig.Prefabs_Lowlight[(int)size], this.Muzzle.position, this.Muzzle.rotation);
                    }
                    else
                    {
                        gameObject = UnityEngine.Object.Instantiate<GameObject>(muzzleConfig.Prefabs_Highlight[(int)size], this.Muzzle.position, this.Muzzle.rotation);
                    }
                    if (muzzleEffects[j].OverridePoint == null)
                    {
                        gameObject.transform.SetParent(this.Muzzle.transform);
                    }
                    else
                    {
                        gameObject.transform.SetParent(muzzleEffects[j].OverridePoint);
                        gameObject.transform.localPosition = Vector3.zero;
                        gameObject.transform.localEulerAngles = Vector3.zero;
                    }
                    MuzzlePSystem muzzlePSystem = new MuzzlePSystem();
                    muzzlePSystem.PSystem = gameObject.GetComponent<ParticleSystem>();
                    muzzlePSystem.OverridePoint = muzzleEffects[j].OverridePoint;
                    int index = (int)size;
                    if (GM.CurrentSceneSettings.IsSceneLowLight)
                    {
                        muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Lowlight[index];
                    }
                    else
                    {
                        muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Highlight[index];
                    }
                    this.m_muzzleSystems.Add(muzzlePSystem);
                }
            }
        }

        public void FireMuzzleSmoke()
        {
            if (GM.CurrentSceneSettings.IsSceneLowLight)
            {
                FXM.InitiateMuzzleFlash(this.Muzzle.position, this.Muzzle.forward, 1f, new Color(1f, 0.9f, 0.77f), 1f);
            }
            for (int i = 0; i < this.m_muzzleSystems.Count; i++)
            {
                if (this.m_muzzleSystems[i].OverridePoint == null)
                {
                    this.m_muzzleSystems[i].PSystem.transform.position = this.Muzzle.position;
                }
                this.m_muzzleSystems[i].PSystem.Emit(this.m_muzzleSystems[i].NumParticlesPerShot);
            }
        }

        private bool IsBarrelPlugged()
        {
            return this.LoadedElements.Count != 0 && (this.LoadedElements[this.LoadedElements.Count - 1].Type != FlintlockBarrel.LoadedElementType.Powder && this.LoadedElements[this.LoadedElements.Count - 1].Position < 0.01f && (this.LoadedElements[this.LoadedElements.Count - 1].Type == FlintlockBarrel.LoadedElementType.Shot || this.LoadedElements[this.LoadedElements.Count - 1].Type == FlintlockBarrel.LoadedElementType.ShotInPaper));
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null)
            {
                return;
            }
            float num = Vector3.Angle(this.Muzzle.forward, Vector3.up);
            if (num > 90f)
            {
                return;
            }
            GameObject gameObject = other.attachedRigidbody.gameObject;
            if (gameObject.CompareTag("flintlock_shot"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.Shot))
                {
                    return;
                }
                if (this.m_weapon.RamRod.gameObject.activeSelf && this.m_weapon.RamRod.RState == FlintlockPseudoRamRod.RamRodState.Barrel && this.m_weapon.RamRod.GetCurBarrel() == this)
                {
                    return;
                }
                if (this.IsBarrelPlugged())
                {
                    return;
                }
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[1], this.Muzzle.position);
                this.InsertElement(FlintlockBarrel.LoadedElementType.Shot);
                UnityEngine.Object.Destroy(other.gameObject);
            }
            else if (gameObject.CompareTag("flintlock_paper"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.ShotInPaper))
                {
                    return;
                }
                if (this.m_weapon.RamRod.gameObject.activeSelf && this.m_weapon.RamRod.RState == FlintlockPseudoRamRod.RamRodState.Barrel && this.m_weapon.RamRod.GetCurBarrel() == this)
                {
                    return;
                }
                if (this.IsBarrelPlugged())
                {
                    return;
                }
                if (Vector3.Angle(this.Muzzle.forward, gameObject.transform.forward) > 80f)
                {
                    return;
                }
                FlintlockPaperCartridge component = gameObject.GetComponent<FlintlockPaperCartridge>();
                if (component.CState == FlintlockPaperCartridge.CartridgeState.Whole)
                {
                    return;
                }
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[2], this.Muzzle.position);
                for (int i = 0; i < component.numPowderChunksLeft; i++)
                {
                    this.InsertElement(FlintlockBarrel.LoadedElementType.Powder);
                }
                this.InsertElement(FlintlockBarrel.LoadedElementType.ShotInPaper);
                UnityEngine.Object.Destroy(other.gameObject);
            }
            else if (gameObject.CompareTag("flintlock_wadding"))
            {
                if (!this.CanElementFit(FlintlockBarrel.LoadedElementType.Wadding))
                {
                    return;
                }
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_InsertByType[3], this.Muzzle.position);
                this.InsertElement(FlintlockBarrel.LoadedElementType.Wadding);
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

        public void Tamp(float delta, float depth)
        {
            if (this.TampRefire < 0.15f)
            {
                return;
            }
            if (Mathf.Abs(delta) < 0.01f)
            {
                return;
            }
            if (this.LoadedElements.Count == 1)
            {
                if (Mathf.Abs(depth) > this.LoadedElements[0].Position)
                {
                    float position = this.LoadedElements[0].Position;
                    float num = this.LoadedElements[0].Position + Mathf.Abs(delta);
                    float max = this.BarrelLength - this.GetLengthOfElement(this.LoadedElements[0].Type, this.LoadedElements[0].PowderAmount);
                    num = Mathf.Clamp(num, num, max);
                    this.LoadedElements[0].Position = num;
                    if (Mathf.Abs(position - num) > 0.001f)
                    {
                        this.m_weapon.PlayAudioAsHandling(this.AudEvent_Tamp, this.Muzzle.position);
                    }
                    else
                    {
                        this.m_weapon.PlayAudioAsHandling(this.AudEvent_TampEnd, this.Muzzle.position);
                    }
                    this.TampRefire = 0f;
                }
            }
            else if (this.LoadedElements.Count > 1 && Mathf.Abs(depth) > this.LoadedElements[this.LoadedElements.Count - 1].Position)
            {
                float position2 = this.LoadedElements[this.LoadedElements.Count - 1].Position;
                float num2 = this.LoadedElements[this.LoadedElements.Count - 1].Position + Mathf.Abs(delta);
                float max2 = this.LoadedElements[this.LoadedElements.Count - 2].Position - this.GetLengthOfElement(this.LoadedElements[this.LoadedElements.Count - 1].Type, this.LoadedElements[this.LoadedElements.Count - 1].PowderAmount);
                num2 = Mathf.Clamp(num2, num2, max2);
                this.LoadedElements[this.LoadedElements.Count - 1].Position = num2;
                if (Mathf.Abs(position2 - num2) > 0.001f)
                {
                    this.m_weapon.PlayAudioAsHandling(this.AudEvent_Tamp, this.Muzzle.position);
                }
                else
                {
                    this.m_weapon.PlayAudioAsHandling(this.AudEvent_TampEnd, this.Muzzle.position);
                }
                this.TampRefire = 0f;
            }
        }

        public float GetMaxDepth()
        {
            if (this.LoadedElements.Count < 1)
            {
                return this.BarrelLength;
            }
            return this.LoadedElements[this.LoadedElements.Count - 1].Position;
        }

        private void InsertElement(FlintlockBarrel.LoadedElementType type)
        {
            if (this.LoadedElements.Count > 0)
            {
                if (type == FlintlockBarrel.LoadedElementType.Powder && this.LoadedElements[this.LoadedElements.Count - 1].Type == FlintlockBarrel.LoadedElementType.Powder)
                {
                    this.LoadedElements[this.LoadedElements.Count - 1].PowderAmount++;
                }
                else
                {
                    FlintlockBarrel.LoadedElement loadedElement = new FlintlockBarrel.LoadedElement();
                    loadedElement.Type = type;
                    loadedElement.Position = 0f;
                    loadedElement.PowderAmount = 1;
                    this.LoadedElements.Add(loadedElement);
                }
            }
            else
            {
                FlintlockBarrel.LoadedElement loadedElement2 = new FlintlockBarrel.LoadedElement();
                loadedElement2.Type = type;
                loadedElement2.Position = 0f;
                loadedElement2.PowderAmount = 1;
                this.LoadedElements.Add(loadedElement2);
            }
        }

        private int ExpellElement(FlintlockBarrel.LoadedElementType type, int PowderAmount)
        {
            if (type != FlintlockBarrel.LoadedElementType.Powder)
            {
                Vector3 position = this.Muzzle.position + this.Muzzle.forward * this.GetLengthOfElement(type, PowderAmount) * 0.75f;
                UnityEngine.Object.Instantiate<GameObject>(this.EjectedObjectPrefabs[(int)type], position, this.Muzzle.rotation);
                return 0;
            }
            int result = PowderAmount - 1;
            Vector3 position2 = this.Muzzle.position + UnityEngine.Random.onUnitSphere * 0.005f + this.Muzzle.forward * this.GetLengthOfElement(type, 1) * 0.75f;
            UnityEngine.Object.Instantiate<GameObject>(this.EjectedObjectPrefabs[(int)type], position2, this.Muzzle.rotation);
            return result;
        }

        public void Ignite()
        {
            if (this.LoadedElements.Count > 0 && this.LoadedElements[0].Type == FlintlockBarrel.LoadedElementType.Powder)
            {
                this.m_isIgnited = true;
                this.m_igniteTick = UnityEngine.Random.Range(0.01f, 0.03f);
            }
        }

        private void Update()
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

        private int GetNumProjectilesInBarrel()
        {
            int num = 0;
            for (int i = 0; i < this.LoadedElements.Count; i++)
            {
                if (this.LoadedElements[i].Type == FlintlockBarrel.LoadedElementType.Shot || this.LoadedElements[i].Type == FlintlockBarrel.LoadedElementType.ShotInPaper)
                {
                    num += this.LoadedElements[i].PowderAmount;
                }
            }
            return num;
        }

        private int GetNumPowder()
        {
            int num = 0;
            for (int i = 0; i < this.LoadedElements.Count; i++)
            {
                if (this.LoadedElements[i].Type == FlintlockBarrel.LoadedElementType.Powder)
                {
                    num += this.LoadedElements[i].PowderAmount;
                }
            }
            return num;
        }

        private float GetMinProjPos()
        {
            float num = this.BarrelLength;
            for (int i = 0; i < this.LoadedElements.Count; i++)
            {
                if (this.LoadedElements[i].Type != FlintlockBarrel.LoadedElementType.Powder)
                {
                    num = Mathf.Min(num, this.LoadedElements[i].Position);
                }
            }
            return num;
        }

        private float GetMaxProjPos()
        {
            float num = 0f;
            for (int i = 0; i < this.LoadedElements.Count; i++)
            {
                if (this.LoadedElements[i].Type != FlintlockBarrel.LoadedElementType.Powder)
                {
                    num = Mathf.Max(num, this.LoadedElements[i].Position);
                }
            }
            return num;
        }

        public void BurnOffOuter()
        {
            if (this.LoadedElements.Count == 0)
            {
                return;
            }
            if (this.LoadedElements[this.LoadedElements.Count - 1].Type != FlintlockBarrel.LoadedElementType.Powder)
            {
                return;
            }
            int powderAmount = this.LoadedElements[this.LoadedElements.Count - 1].PowderAmount;
            this.LoadedElements.RemoveAt(this.LoadedElements.Count - 1);
            float t = Mathf.Lerp(0f, 1f, (float)powderAmount / 60f);
            float spread = this.Spread;
            float num = Mathf.Lerp(this.MuzzleOverFireSystemScaleRange.x, this.MuzzleOverFireSystemScaleRange.y, t);
            float num2 = Mathf.Lerp(this.MuzzleOverFireSystemEmitRange.x, this.MuzzleOverFireSystemEmitRange.y, t);
            this.MuzzleOverFireSystem.transform.localEulerAngles = new Vector3(num, num, num);
            this.MuzzleOverFireSystem.Emit(Mathf.RoundToInt(num2));
            if (this.LoadedElements.Count == 0 && this.m_pan.GetPanContents() > 0f)
            {
                float panContents = this.m_pan.GetPanContents();
                this.m_pan.FlashBlast(Mathf.RoundToInt(panContents), Mathf.RoundToInt(panContents));
                this.m_pan.Ignite();
            }
            this.FireMuzzleSmoke();
            this.m_weapon.Fire(0f);
            if (this.m_weapon.RamRod.GetCurBarrel() == this)
            {
                this.m_weapon.RamRod.gameObject.SetActive(false);
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_weapon.RamRodProj, this.Muzzle.position, this.Muzzle.rotation);
                gameObject.GetComponent<BallisticProjectile>().Fire(this.Muzzle.forward, this.m_weapon.OverrideFA);
                this.m_weapon.RamRod.GameObject.SetActive(false);
                this.m_weapon.RamRod.MountToBarrel(null, null);
            }
            int num3 = 0;
            while ((float)num3 < num2)
            {
                Vector3 b = this.Muzzle.forward * 0.005f;
                GameObject igniteProjectile_Visible = this.IgniteProjectile_Visible;
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(igniteProjectile_Visible, this.Muzzle.position - b, this.Muzzle.rotation);
                gameObject2.transform.Rotate(new Vector3(UnityEngine.Random.Range(-spread * 2f, spread * 2f), UnityEngine.Random.Range(-spread * 2f, spread * 2f), 0f));
                BallisticProjectile component = gameObject2.GetComponent<BallisticProjectile>();
                component.Fire(component.MuzzleVelocityBase, gameObject2.transform.forward, this.m_weapon.OverrideFA, true);
                num3++;
            }
            this.m_weapon.PlayAudioGunShot(true, FVRTailSoundClass.Launcher, FVRTailSoundClass.SuppressedLarge, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
        }

        private void Fire()
        {
            if (this.LoadedElements.Count > 0 && this.LoadedElements[0].Type != FlintlockBarrel.LoadedElementType.Powder)
            {
                this.m_isIgnited = false;
                return;
            }
            bool flag = false;
            bool flag2 = false;
            int numProjectilesInBarrel = this.GetNumProjectilesInBarrel();
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
                gameObject.GetComponent<BallisticProjectile>().Fire(this.Muzzle.forward, this.m_weapon.OverrideFA);
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
                component.Fire(component.MuzzleVelocityBase, gameObject2.transform.forward, this.m_weapon.OverrideFA, true);
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
            for (int i = 0; i < numProjectilesInBarrel; i++)
            {
                Vector3 b2 = this.Muzzle.forward * 0.005f;
                GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(this.ProjectilePrefab, this.Muzzle.position - b2, this.Muzzle.rotation);
                gameObject3.transform.Rotate(new Vector3(UnityEngine.Random.Range(-num2, num2), UnityEngine.Random.Range(-num2, num2), 0f));
                BallisticProjectile component2 = gameObject3.GetComponent<BallisticProjectile>();
                component2.Fire(component2.MuzzleVelocityBase * num3 * this.VelocityMult, gameObject3.transform.forward, this.m_weapon.OverrideFA, true);
            }
            this.ClearBarrel();
            if (flag2)
            {
                this.m_weapon.Attachment.ForceBreakInteraction();
                this.m_weapon.Attachment.RootRigidbody.velocity = this.m_weapon.transform.forward * -8f + this.m_weapon.transform.up * 1f;
                this.m_weapon.Attachment.RootRigidbody.angularVelocity = this.m_weapon.transform.right * -5f;
            }
            if (flag)
            {
                this.m_weapon.Blowup();
            }
            this.m_isIgnited = false;
        }

        private void ClearBarrel()
        {
            this.LoadedElements.Clear();
        }

        private void BarrelContentsSim()
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
                                int num4 = this.ExpellElement(this.LoadedElements[i].Type, this.LoadedElements[i].PowderAmount);
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

        private void BarrelContentsDraw()
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
    }
}
