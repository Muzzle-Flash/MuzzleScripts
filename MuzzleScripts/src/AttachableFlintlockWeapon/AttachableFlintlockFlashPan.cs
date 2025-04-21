using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using MuzzleScripts;
using UnityEngine;

namespace MuzzleScripts
{
    public class AttachableFlintlockFlashPan : MonoBehaviour, IFVRDamageable
    {
        public AttachableFlintlockWeapon m_weapon;

        [Header("Frizen")]
        public Transform Frizen;

        public Vector2 FrizenRots = new Vector2(0f, 45f);

        public FlintlockFlashPan.FState FrizenState = FlintlockFlashPan.FState.Up;

        [Header("VFX")]
        public GameObject GrainPrefab;

        public Transform GrainSpawnPoint;

        public List<Renderer> GrainPileGeo;

        public ParticleSystem Flash_Fire;

        public ParticleSystem Flash_Smoke;

        public ParticleSystem Shot_Fire;

        public ParticleSystem Shot_Smoke;

        [Header("Audio")]
        public AudioEvent AudEvent_FrizenUp;

        public AudioEvent AudEvent_FrizenDown;

        public AudioEvent AudEvent_FlashpanIgnite;

        public AudioEvent AudEvent_PowderLandOnFlashpan;

        [Header("Barrels")]
        public List<AttachableFlintlockBarrel> Barrels = new List<AttachableFlintlockBarrel>();

        private float numGrainsPowderOn;

        private bool m_isIgnited;

        private float timeSinceSpawn;

        private void AddGrain()
        {
            this.numGrainsPowderOn += 1f;
            this.SetGrainPileGeo(Mathf.CeilToInt(this.numGrainsPowderOn));
            this.m_weapon.PlayAudioAsHandling(this.AudEvent_PowderLandOnFlashpan, this.Frizen.position);
        }

        public void ClearPan()
        {
            this.numGrainsPowderOn = 0f;
            this.SetGrainPileGeo(-1);
        }

        public void Damage(Damage d)
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Up && (d.Dam_Thermal > 0f || d.Class == FistVR.Damage.DamageClass.Projectile))
            {
                this.Ignite();
            }
        }

        private void FireBarrels()
        {
            for (int i = 0; i < this.Barrels.Count; i++)
            {
                this.Barrels[i].Ignite();
            }
        }

        public void FlashBlast(int x, int y)
        {
            this.Shot_Fire.Emit(x);
            this.Shot_Smoke.Emit(y);
        }

        public float GetPanContents()
        {
            return this.numGrainsPowderOn;
        }

        public AttachableFlintlockWeapon GetWeapon()
        {
            return this.m_weapon;
        }

        public void HammerHit(FlintlockWeapon.FlintState f, bool Flint)
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Down && Flint)
            {
                this.Ignite();
            }
            this.SetFrizenUp();
        }

        public void Ignite()
        {
            if (this.numGrainsPowderOn > 0f)
            {
                this.m_isIgnited = true;
                this.m_weapon.PlayAudioAsHandling(this.AudEvent_FlashpanIgnite, this.Frizen.position);
            }
        }

        private bool IsPanFull()
        {
            return this.numGrainsPowderOn > 4f;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Down)
            {
                return;
            }
            if (this.IsPanFull())
            {
                return;
            }
            if (other.attachedRigidbody == null)
            {
                return;
            }
            float num = Vector3.Angle(base.transform.up, Vector3.up);
            if (num > 70f)
            {
                return;
            }
            GameObject gameObject = other.attachedRigidbody.gameObject;
            if (gameObject.CompareTag("flintlock_powdergrain"))
            {
                this.AddGrain();
                UnityEngine.Object.Destroy(other.gameObject);
            }
        }

        private void SetFrizenDown()
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Down)
            {
                return;
            }
            if (this.m_weapon.HammerState == FlintlockWeapon.HState.Uncocked)
            {
                return;
            }
            this.FrizenState = FlintlockFlashPan.FState.Down;
            this.m_weapon.Attachment.SetAnimatedComponent(this.Frizen, this.FrizenRots.x, FVRPhysicalObject.InterpStyle.Rotation, FVRPhysicalObject.Axis.X);
            this.m_weapon.PlayAudioAsHandling(this.AudEvent_FrizenDown, this.Frizen.position);
        }

        private void SetFrizenUp()
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Up)
            {
                return;
            }
            this.FrizenState = FlintlockFlashPan.FState.Up;
            this.m_weapon.Attachment.SetAnimatedComponent(this.Frizen, this.FrizenRots.y, FVRPhysicalObject.InterpStyle.Rotation, FVRPhysicalObject.Axis.X);
            this.m_weapon.PlayAudioAsHandling(this.AudEvent_FrizenUp, this.Frizen.position);
        }

        private void SetGrainPileGeo(int g)
        {
            for (int i = 0; i < this.GrainPileGeo.Count; i++)
            {
                if (i == g)
                {
                    this.GrainPileGeo[i].enabled = true;
                }
                else
                {
                    this.GrainPileGeo[i].enabled = false;
                }
            }
        }

        public void SetWeapon(AttachableFlintlockWeapon w)
        {
            this.m_weapon = w;
            for (int i = 0; i < this.Barrels.Count; i++)
            {
                this.Barrels[i].SetWeapon(w);
                this.Barrels[i].SetPan(this);
            }
        }

        public void ToggleFrizenState()
        {
            if (this.FrizenState == FlintlockFlashPan.FState.Down)
            {
                this.SetFrizenUp();
            }
            else
            {
                this.SetFrizenDown();
            }
        }

        private void Update()
        {
            if (this.m_isIgnited)
            {
                this.numGrainsPowderOn -= Time.deltaTime * 20f;
                this.numGrainsPowderOn = Mathf.Clamp(this.numGrainsPowderOn, 0f, this.numGrainsPowderOn);
                this.SetGrainPileGeo(Mathf.CeilToInt(this.numGrainsPowderOn));
                this.Flash_Fire.Emit(1);
                this.Flash_Smoke.Emit(2);
                if (this.numGrainsPowderOn <= 0f)
                {
                    this.SetGrainPileGeo(-1);
                    this.m_isIgnited = false;
                    this.FireBarrels();
                }
            }
            if (this.timeSinceSpawn < 1f)
            {
                this.timeSinceSpawn += Time.deltaTime;
            }
            if (this.FrizenState == FlintlockFlashPan.FState.Up)
            {
                float num = Vector3.Angle(this.m_weapon.transform.up, Vector3.up);
                if (num > 85f && this.timeSinceSpawn > 0.04f && this.numGrainsPowderOn > 0f)
                {
                    this.numGrainsPowderOn -= 1f;
                    this.numGrainsPowderOn = Mathf.Clamp(this.numGrainsPowderOn, 0f, this.numGrainsPowderOn);
                    if (this.numGrainsPowderOn <= 0f)
                    {
                        this.SetGrainPileGeo(-1);
                    }
                    else
                    {
                        this.SetGrainPileGeo(Mathf.CeilToInt(this.numGrainsPowderOn));
                    }
                    this.timeSinceSpawn = 0f;
                    UnityEngine.Object.Instantiate<GameObject>(this.GrainPrefab, this.GrainSpawnPoint.position, UnityEngine.Random.rotation);
                }
            }
            if (this.m_weapon.Attachment.IsHeld)
            {
            }
        }


    }
}
