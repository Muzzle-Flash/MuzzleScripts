using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;
using MuzzleScripts;

namespace MuzzleScripts
{
    public class AttachableFlintlockWeapon : AttachableFirearm
    {
        [Header("Flintlock")]
        public List<AttachableFlintlockFlashPan> FlashPans = new List<AttachableFlintlockFlashPan>();
        private int m_curFlashPan;
        public AttachableFlintlockPseudoRamRod RamRod;

        [Header("Trigger")]
        public Transform Trigger;
        public Vector2 TriggerRots = new Vector2(0f, 5f);
        private float m_triggerFloat;
        private float m_lastTriggerFloat;

        [Header("Hammer")]
        public Transform Hammer;
        public FlintlockWeapon.HState HammerState;
        public Vector3 HammerRots = new Vector3(0f, 20f, 45f);
        private float m_curHammerRot;
        private float m_tarHammerRot;

        [Header("Flint")]
        public FlintlockWeapon.FlintState FState;
        private Vector3 m_flintUses = Vector3.one;
        private bool m_hasFlint = true;
        public MeshFilter FlintMesh;
        public MeshRenderer FlintRenderer;
        public List<Mesh> FlintMeshes = new List<Mesh>();
        public AttachableFlintlockFlintScrew FlintlockScrew;
        public AttachableFlintlockFlintHolder FlintlockFlintHolder;
        public ParticleSystem Sparks;

        [Header("Audio")]
        public AudioEvent AudEvent_HammerCock;

        public AudioEvent AudEvent_HammerHalfCock;

        public AudioEvent AudEvent_HammerHit_Clean;

        public AudioEvent AudEvent_HammerHit_Dull;

        public AudioEvent AudEvent_Spark;

        public AudioEvent AudEvent_FlintBreak;

        public AudioEvent AudEvent_FlintHolderScrew;

        public AudioEvent AudEvent_FlintHolderUnscrew;

        public AudioEvent AudEvent_FlintRemove;

        public AudioEvent AudEvent_FlintReplace;

        [Header("Destruction")]
        public List<GameObject> DisableOnDestroy = new List<GameObject>();
        public List<GameObject> EnableOnDestroy = new List<GameObject>();
        public bool m_isDestroyed;
        public List<GameObject> SpawnOnDestroy = new List<GameObject>();
        public Transform SpawnOnDestroyPoint;
        public GameObject RamRodProj;
        private float FireReFire = 0.2f;

        public override void Awake()
        {
            base.Awake();
            for (int i = 0; i < this.FlashPans.Count; i++)
            {
                this.FlashPans[i].SetWeapon(this);
            }
            this.m_flintUses = new Vector3((float)UnityEngine.Random.Range(8, 15), (float)UnityEngine.Random.Range(5, 9), (float)UnityEngine.Random.Range(4, 8));
        }

        public bool HasFlint()
        {
            return this.m_hasFlint;
        }

        public void AddFlint(Vector3 uses)
        {
            this.m_hasFlint = true;
            this.m_flintUses = uses;
            this.FlintRenderer.enabled = true;
            if (this.m_flintUses.x > 0f) this.SetFlintState(FlintlockWeapon.FlintState.New);
            else if (this.m_flintUses.y > 0f) this.SetFlintState(FlintlockWeapon.FlintState.Used);
            else if (this.m_flintUses.z > 0f) this.SetFlintState(FlintlockWeapon.FlintState.Worn);
            else this.SetFlintState(FlintlockWeapon.FlintState.Broken);
        }

        public Vector3 RemoveFlint()
        {
            this.m_hasFlint = false;
            this.FlintRenderer.enabled = false;
            return this.m_flintUses;
        }

        private void SetFlintState(FlintlockWeapon.FlintState state)
        {
            this.FState = state;
            this.FlintMesh.mesh = this.FlintMeshes[(int)state];
        }

        public void Blowup()
        {
            if (this.m_isDestroyed) return;
            this.m_isDestroyed = true;
            
            for (int i = 0; i<this.DisableOnDestroy.Count; i++)
            {
                this.DisableOnDestroy[i].SetActive(false);
            }
            for (int j = 0; j <this.DisableOnDestroy.Count; j++)
            {
                this.EnableOnDestroy[j].SetActive(true);
            }
            for (int k = 0; k < this.DisableOnDestroy.Count; k++)
            {
                UnityEngine.Object.Instantiate<GameObject>(this.SpawnOnDestroy[k], this.SpawnOnDestroyPoint.position, this.SpawnOnDestroyPoint.rotation);
            }
        }

        public void Fire(float recoilMult = 1f)
        {
            if (this.FireReFire < 0.1f) return;

            this.FireReFire = 0f;
            FVRViveHand hand = base.Attachment.m_hand;
            if (hand != null)
            {
                hand.Buzz(hand.Buzzer.Buzz_GunShot);
                if (base.Attachment.curMount.Parent.m_hand != null && base.Attachment.curMount.Parent != null)
                {
                    base.Attachment.curMount.Parent.m_hand.Buzz(base.Attachment.curMount.Parent.m_hand.Buzzer.Buzz_GunShot);
                }
                
            }
            if (base.IsSuppressed())
            {
                GM.CurrentPlayerBody.VisibleEvent(0.1f);
            }
            else
            {
                GM.CurrentPlayerBody.VisibleEvent(2f);
            }
            FVRFireArm fvrfirearm = null;
            if (this.OverrideFA != null)
            {
                fvrfirearm = this.OverrideFA;
                GM.CurrentSceneSettings.OnShotFired(fvrfirearm);
                this.Recoil(true, fvrfirearm);
            }
        }

        public override void ProcessInput(FVRViveHand hand, bool fromInterface, FVRInteractiveObject o)
        {
            base.ProcessInput(hand, fromInterface, o);
            if (o.m_hasTriggeredUpSinceBegin)
            {
                this.m_triggerFloat = hand.Input.TriggerFloat;
            }
            else
            {
                this.m_triggerFloat = 0f;
            }
            if (!this.m_isDestroyed && o.m_hasTriggeredUpSinceBegin)
            {
                if (hand.IsInStreamlinedMode)
                {
                    if (hand.Input.BYButtonDown)
                    {
                        if (this.HammerState == FlintlockWeapon.HState.Uncocked) 
                        { 
                            this.MoveToHalfCock();
                        }
                        else if (this.HammerState == FlintlockWeapon.HState.Halfcock) 
                        {
                            this.MoveToFullCock();
                        }
                    }
                }
                else
                {
                    if (hand.Input.TouchpadDown && (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f || Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f))
                    {
                        if (this.HammerState == FlintlockWeapon.HState.Uncocked)
                        {
                            this.MoveToHalfCock();
                        }
                        else if (this.HammerState == FlintlockWeapon.HState.Halfcock)
                        {
                            this.MoveToFullCock();
                        }
                    }
                }
            }
        }

        private void MoveToFullCock()
        {
            if (this.HammerState == FlintlockWeapon.HState.Fullcock)
            {
                return;
            }
            this.HammerState = FlintlockWeapon.HState.Fullcock;
            this.m_tarHammerRot = this.HammerRots.z;
            base.PlayAudioAsHandling(this.AudEvent_HammerCock, this.Hammer.position);
        }

        private void MoveToHalfCock()
        {
            if (this.HammerState != FlintlockWeapon.HState.Uncocked)
            {
                return;
            }
            this.HammerState = FlintlockWeapon.HState.Halfcock;
            this.m_tarHammerRot = this.HammerRots.y;
            base.PlayAudioAsHandling(this.AudEvent_HammerHalfCock, this.Hammer.position);
        }

        public override void Update()
        {
            base.Update();
            if (this.FireReFire < 0.2f) this.FireReFire += Time.deltaTime;
            if (this.m_triggerFloat != this.m_lastTriggerFloat) this.m_lastTriggerFloat = this.m_triggerFloat;

            this.Attachment.SetAnimatedComponent(this.Trigger, Mathf.Lerp(this.TriggerRots.x, this.TriggerRots.y, this.m_triggerFloat), FVRPhysicalObject.InterpStyle.Rotation, FVRPhysicalObject.Axis.X);

            if (!this.m_isDestroyed)
            {
                if (this.m_curHammerRot != this.m_tarHammerRot)
                {
                    float num = 7200f;
                    if (this.HammerState == FlintlockWeapon.HState.Halfcock || this.HammerState == FlintlockWeapon.HState.Fullcock)
                    {
                        num = 360f;
                    }
                    this.m_curHammerRot = Mathf.MoveTowards(m_curHammerRot, m_tarHammerRot, Time.deltaTime * num);
                    this.Attachment.SetAnimatedComponent(this.Hammer, this.m_curHammerRot, FVRPhysicalObject.InterpStyle.Rotation, FVRPhysicalObject.Axis.X);
                }
                if (this.m_triggerFloat > 0.7f && this.HammerState == FlintlockWeapon.HState.Fullcock)
                {
                    this.ReleaseHammer();
                }
            }
        }

        private void ReleaseHammer()
        {
            if (this.HammerState != FlintlockWeapon.HState.Fullcock) return;
            this.HammerState = FlintlockWeapon.HState.Uncocked;
            this.m_tarHammerRot = this.HammerRots.x;
            if (this.HitWithFlint())
            {
                base.PlayAudioAsHandling(this.AudEvent_HammerHit_Clean, this.Hammer.position);
                if (this.FlashPans[this.m_curFlashPan].FrizenState == FlintlockFlashPan.FState.Down)
                {
                    base.PlayAudioAsHandling(this.AudEvent_Spark, this.Hammer.position);
                    this.Sparks.Emit(15);
                }
                this.FlashPans[this.m_curFlashPan].HammerHit(this.FState, true);
            }
            else
            {
                this.FlashPans[this.m_curFlashPan].HammerHit(this.FState, false);
                base.PlayAudioAsHandling(this.AudEvent_HammerHit_Dull, this.Hammer.position);
            }
        }

        private bool HitWithFlint()
        {
            if (!this.m_hasFlint) return false;
            
            switch (this.FState)
            {
                case FlintlockWeapon.FlintState.New:
                    this.m_flintUses.x --;
                    if (this.m_flintUses.x <= 0)
                    {
                        this.SetFlintState(FlintlockWeapon.FlintState.Used);
                        base.PlayAudioAsHandling(this.AudEvent_FlintBreak, this.Hammer.position);
                    }
                    return true;

                case FlintlockWeapon.FlintState.Used:
                    this.m_flintUses.y --;
                    if (this.m_flintUses.y <= 0)
                    {
                        this.SetFlintState(FlintlockWeapon.FlintState.Worn);
                        base.PlayAudioAsHandling(this.AudEvent_FlintBreak, this.Hammer.position);
                    }
                    return true;
                case FlintlockWeapon.FlintState.Worn:
                    this.m_flintUses.z --;
                    if (this.m_flintUses.z <= 0)
                    {
                        this.SetFlintState(FlintlockWeapon.FlintState.Broken);
                        base.PlayAudioAsHandling(this.AudEvent_FlintBreak, this.Hammer.position);
                    }
                    return true;
                case FlintlockWeapon.FlintState.Broken:
                    return false;
                default:
                    return false;
            }
        }

        [ContextMenu("Copy Existing Weapon Components")]
        public void CopyFromHierarchy()
        {
            Debug.Log("Copying Flintlock Components...");
            FlintlockWeapon flintlockWeapon = base.GetComponents<FlintlockWeapon>().Single((FlintlockWeapon c) => c != this);

            Debug.Log("Copying Flashpans...");
            List<FlintlockFlashPan> flashpans = flintlockWeapon.FlashPans;
            foreach (FlintlockFlashPan flashpan in flashpans)
            {
                AttachableFlintlockFlashPan A_FlashPan = flashpan.gameObject.AddComponent<AttachableFlintlockFlashPan>();
                if (flashpan.Frizen != null) A_FlashPan.Frizen = flashpan.Frizen;
                if (flashpan.FrizenRots != null) A_FlashPan.FrizenRots = flashpan.FrizenRots;
                A_FlashPan.FrizenState = flashpan.FrizenState;

                FlintlockFrizenTrigger frizenTrigger = flashpan.Frizen.gameObject.GetComponent<FlintlockFrizenTrigger>();
                AttachableFlintlockFrizenTrigger A_FrizenTrgger = frizenTrigger.gameObject.AddComponent<AttachableFlintlockFrizenTrigger>();
                A_FrizenTrgger.IsSimpleInteract = true;
                A_FrizenTrgger.FlashPan = A_FlashPan as AttachableFlintlockFlashPan;
                A_FrizenTrgger.DistPoint = frizenTrigger.DistPoint;

                if (flashpan.GrainPrefab != null) A_FlashPan.GrainPrefab = flashpan.GrainPrefab;
                if (flashpan.GrainPileGeo != null) A_FlashPan.GrainPileGeo = flashpan.GrainPileGeo;
                if (flashpan.Flash_Fire != null) A_FlashPan.Flash_Fire = flashpan.Flash_Fire;
                if (flashpan.Flash_Smoke != null) A_FlashPan.Flash_Smoke = flashpan.Flash_Smoke;
                if (flashpan.Shot_Fire != null) A_FlashPan.Shot_Fire = flashpan.Shot_Fire;
                if (flashpan.Shot_Smoke != null) A_FlashPan.Shot_Smoke = flashpan.Shot_Smoke;
                A_FlashPan.AudEvent_FrizenUp = flashpan.AudEvent_FrizenUp;
                A_FlashPan.AudEvent_FrizenDown = flashpan.AudEvent_FrizenDown;
                A_FlashPan.AudEvent_FlashpanIgnite = flashpan.AudEvent_FlashpanIgnite;
                A_FlashPan.AudEvent_PowderLandOnFlashpan = flashpan.AudEvent_PowderLandOnFlashpan;
                if (flashpan.Barrels != null)
                {
                    foreach (FlintlockBarrel barrel in flashpan.Barrels)
                    {
                        Debug.Log("Copying Barrel...");
                        AttachableFlintlockBarrel A_Barrel = barrel.gameObject.AddComponent<AttachableFlintlockBarrel>();
                        A_Barrel.Muzzle = barrel.Muzzle;
                        A_Barrel.LodgePoint_Paper = barrel.LodgePoint_Paper;
                        A_Barrel.LodgePoint_Shot = barrel.LodgePoint_Shot;
                        A_Barrel.BarrelLength = barrel.BarrelLength;
                        A_Barrel.ProxyRends0 = barrel.ProxyRends0;
                        A_Barrel.ProxyRends1 = barrel.ProxyRends1;
                        A_Barrel.ProxyPowder0 = barrel.ProxyPowder0;
                        A_Barrel.ProxyPowder1 = barrel.ProxyPowder1;
                        A_Barrel.ProxyPowderPiles = barrel.ProxyPowderPiles;
                        A_Barrel.EjectedObjectPrefabs = barrel.EjectedObjectPrefabs;
                        A_Barrel.LoadedElementSizes = barrel.LoadedElementSizes;
                        A_Barrel.ProjectilePrefab = barrel.ProjectilePrefab;
                        A_Barrel.Spread = barrel.Spread;
                        A_Barrel.VelocityMult = barrel.VelocityMult;
                        A_Barrel.IgniteProjectile_Visible = barrel.IgniteProjectile_Visible;
                        A_Barrel.IgniteProjectile_NotVisible = barrel.IgniteProjectile_NotVisible;
                        A_Barrel.PowderToVelMultCurve = barrel.PowderToVelMultCurve;
                        A_Barrel.AudEvent_Tamp = barrel.AudEvent_Tamp;
                        A_Barrel.AudEvent_TampEnd = barrel.AudEvent_TampEnd;
                        A_Barrel.AudEvent_Squib = barrel.AudEvent_Squib;
                        A_Barrel.AudEvent_InsertByType = barrel.AudEvent_InsertByType;
                        A_Barrel.DefaultMuzzleEffectSize = barrel.DefaultMuzzleEffectSize;
                        A_Barrel.MuzzleEffects = barrel.MuzzleEffects;
                        A_Barrel.MuzzleOverFireSystem = barrel.MuzzleOverFireSystem;
                        A_Barrel.MuzzleOverFireSystemScaleRange = barrel.MuzzleOverFireSystemScaleRange;
                        A_Barrel.MuzzleOverFireSystemEmitRange = barrel.MuzzleOverFireSystemEmitRange;
                        A_Barrel.FlashBlastSmokeRange = barrel.FlashBlastSmokeRange;
                        A_Barrel.FlashBlastFireRange = barrel.FlashBlastFireRange;
                        A_Barrel.gameObject.GetComponent<FlintlockBarrel>().enabled = false;
                        if (A_Barrel == null)
                        {
                            Debug.Log("Found the problem! Now how do I fix A_Barrel being null?");
                        }
                        else if (A_FlashPan == null)
                        {
                            Debug.Log("Found the problem! Now how do I fix A_FlashPan being null?");
                        }
                        else if (A_FlashPan.Barrels == null)
                        {
                            Debug.Log("Found the problem! Now how do I fix A_FlashPan.Barrels being null?");
                        }
                        Debug.Log("Adding A_Barrel to Flashpan");
                        A_FlashPan.Barrels.Add(A_Barrel as AttachableFlintlockBarrel);
                        Debug.Log("Finished Copying Barrel");

                        FlintlockDamageableSensor damageableSensor = A_Barrel.gameObject.GetComponentInChildren<FlintlockDamageableSensor>();
                        if (damageableSensor != null)
                        {
                            Debug.Log("Copying Sensor...");
                            AttachableFlintlockDamagableSensor A_DamageableSensor = damageableSensor.gameObject.AddComponent<AttachableFlintlockDamagableSensor>();
                            A_DamageableSensor.Pan = A_FlashPan;
                            A_DamageableSensor.Barrel = A_Barrel;
                            DestroyImmediate(A_DamageableSensor.gameObject.GetComponent<FlintlockDamageableSensor>());
                            Debug.Log("Finished Copying Sensor");
                        }   
                    }
                }
                A_FlashPan.gameObject.GetComponent<FlintlockFlashPan>().enabled = false;
                this.FlashPans.Add(A_FlashPan);
            }
            Debug.Log("Finished Copying Flashpans");

            Debug.Log("Copying PseudoRamRod...");
            FlintlockPseudoRamRod ramrod = flintlockWeapon.RamRod;
            AttachableFlintlockPseudoRamRod A_RamRod = ramrod.gameObject.AddComponent<AttachableFlintlockPseudoRamRod>();
            A_RamRod.Root = ramrod.Root;
            A_RamRod.RState = ramrod.RState;
            A_RamRod.Point_Lower_Rear = ramrod.Point_Lower_Rear;
            A_RamRod.Point_Lower_Forward = ramrod.Point_Lower_Forward;
            A_RamRod.RamRodPrefab = ramrod.RamRodPrefab;
            A_RamRod.AudEvent_Grab = ramrod.AudEvent_Grab;
            A_RamRod.AudEvent_ExtractHolder = ramrod.AudEvent_ExtractHolder;
            A_RamRod.AudEvent_ExtractBarrel = ramrod.AudEvent_ExtractBarrel;
            A_RamRod.AudEvent_InsertHolder = ramrod.AudEvent_InsertHolder;
            A_RamRod.AudEvent_InsertBarrel = ramrod.AudEvent_InsertBarrel;
            A_RamRod.gameObject.GetComponent<FlintlockPseudoRamRod>().enabled = false;
            this.RamRod = A_RamRod;
            Debug.Log("Finished Copying PseudoRamRod");

            Debug.Log("Copying RamRodHolder...");
            FlintlockRamRodHolder ramRodHolder = this.gameObject.GetComponentInChildren<FlintlockRamRodHolder>();
            if (ramRodHolder == null) Debug.Log("Found the problem! Now how do I fix ramRodHolder being null?");
            AttachableFlintlockRamRodHolder A_RamRodHolder = ramRodHolder.gameObject.AddComponent<AttachableFlintlockRamRodHolder>();
            if (A_RamRodHolder == null) Debug.Log("Found the problem! Now how do I fix A_RamRodHolder being null?");
            DestroyImmediate(A_RamRodHolder.gameObject.GetComponent<FlintlockRamRodHolder>());
            A_RamRodHolder.Weapon = this as AttachableFlintlockWeapon;
            Debug.Log("Finished Copying RamRodHolder");

            this.Trigger = flintlockWeapon.Trigger;
            this.TriggerRots = flintlockWeapon.TriggerRots;

            this.Hammer = flintlockWeapon.Hammer;
            this.HammerRots = flintlockWeapon.HammerRots;
            this.HammerState = flintlockWeapon.HammerState;

            this.FState = flintlockWeapon.FState;
            this.FlintMesh = flintlockWeapon.FlintMesh;
            this.FlintRenderer = flintlockWeapon.FlintRenderer;
            foreach(Mesh mesh in flintlockWeapon.FlintMeshes)
            {
                this.FlintMeshes.Add(mesh);
            }

            Debug.Log("Copying FlintScrew...");
            FlintlockFlintScrew flintScrew = flintlockWeapon.FlintlockScrew;
            if (flintScrew == null) Debug.Log("Found the problem! Now how do I fix flintScrew being null?");
            AttachableFlintlockFlintScrew A_FlintScrew = flintScrew.gameObject.AddComponent<AttachableFlintlockFlintScrew>();
            if (A_FlintScrew == null) Debug.Log("Found the problem! Now how do I fix A_FlintScrew being null?");
            A_FlintScrew.Weapon = this as AttachableFlintlockWeapon;
            A_FlintScrew.Screw = flintScrew.Screw;
            A_FlintScrew.SState = flintScrew.SState;
            A_FlintScrew.Heights = flintScrew.Heights;
            A_FlintScrew.AudEvent_Screw = flintScrew.AudEvent_Screw;
            A_FlintScrew.AudEvent_Unscrew = flintScrew.AudEvent_Unscrew;
            A_FlintScrew.gameObject.GetComponent<FlintlockFlintScrew>().enabled = false;
            this.FlintlockScrew = A_FlintScrew;
            Debug.Log("Finished Copying FlintScrew");

            Debug.Log("Copying FlintHolder...");
            FlintlockFlintHolder flintHolder = flintlockWeapon.FlintlockHolder;
            if (flintHolder == null) Debug.Log("Found the problem! Now how do I fix flintHolder being null?");
            AttachableFlintlockFlintHolder A_FlintHolder = flintHolder.gameObject.AddComponent<AttachableFlintlockFlintHolder>();
            if (A_FlintHolder == null) Debug.Log("Found the problem! Now how do I fix A_FlintHolder being null?");
            A_FlintHolder.Screw = this.FlintlockScrew;
            A_FlintHolder.FlintPrefab = flintHolder.FlintPrefab;
            A_FlintHolder.AudEvent_Remove = flintHolder.AudEvent_Remove;
            A_FlintHolder.AudEvent_Replace = flintHolder.AudEvent_Replace;
            A_FlintHolder.FlintPos = flintHolder.FlintPos;
            A_FlintHolder.gameObject.GetComponent<FlintlockFlintHolder>().enabled = false;
            this.FlintlockFlintHolder = A_FlintHolder;
            Debug.Log("Finished Copying FlintHolder");


            this.Sparks = flintlockWeapon.Sparks;

            this.AudEvent_HammerCock = flintlockWeapon.AudEvent_HammerCock;
            this.AudEvent_HammerHalfCock = flintlockWeapon.AudEvent_HammerHalfCock;
            this.AudEvent_HammerHit_Clean = flintlockWeapon.AudEvent_HammerHit_Clean;
            this.AudEvent_HammerHit_Dull = flintlockWeapon.AudEvent_HammerHit_Dull;
            this.AudEvent_Spark = flintlockWeapon.AudEvent_Spark;
            this.AudEvent_FlintBreak = flintlockWeapon.AudEvent_FlintBreak;

            this.DisableOnDestroy = flintlockWeapon.DisableOnDestroy;
            this.EnableOnDestroy = flintlockWeapon.EnableOnDestroy;
            this.SpawnOnDestroy = flintlockWeapon.SpawnOnDestroy;
            this.SpawnOnDestroyPoint = flintlockWeapon.SpawnOnDestroyPoint;
            this.RamRodProj = flintlockWeapon.RamRodProj;

            this.gameObject.GetComponent<FlintlockWeapon>().enabled = false;
            Debug.Log("Finished Copying Components! Double check just to be sure :D");

        }
    }
}
