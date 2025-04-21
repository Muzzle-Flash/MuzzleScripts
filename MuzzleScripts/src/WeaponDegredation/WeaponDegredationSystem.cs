using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace MuzzleScripts
{
    public class WeaponDegredationSystem : MonoBehaviour
    {
        public FVRFireArm Firearm;
        public int ShotsToDestroy;
        public List<GrungeMaterial> GrungeMaterials;
        public List<GameObject> DisableOnDestroy;
        public List<GameObject> EnableOnDestroy;
        public List<GameObject> LooseParts;
        private List<GameObject> _origLooseParts;
        public AudioEvent AudEventDestroy;

        [Header("Special Handling")]
        public bool UsesSpecialHandling;
        [Header("Bolt Action")]
        public bool IsBoltAction;
        public GameObject PseudoBoltObject;
        [Header("Pump Action")]
        public bool IsPumpAction;
        public GameObject PseudoPumpObject;



        private int _shotsFired;
        private int _curComponentIndex;
        private bool _DestroyThis = false;
        private bool _isDestroyed = false;

        [Serializable]
        public class GrungeMaterial
        {
            public List<Material> Materials = new List<Material>();
            public List<MeshRenderer> Renderers = new List<MeshRenderer>();
        }

        public void Awake()
        {
            GM.CurrentSceneSettings.ShotFiredEvent += this.ShotFired;
            this._origLooseParts = this.LooseParts;
            this._shotsFired = UnityEngine.Random.Range(0, Mathf.RoundToInt(this.ShotsToDestroy/10));
            //this.UpdateMaterials();
            if (this.PseudoBoltObject != null) this.PseudoBoltObject.SetActive(false);
            if (this.PseudoPumpObject != null) this.PseudoPumpObject.SetActive(false);
        }
        public void Restore()
        {
            this.LooseParts = this._origLooseParts;
            this._shotsFired = UnityEngine.Random.Range(0, Mathf.RoundToInt(this.ShotsToDestroy / 10));
            for (int i = 0; i < this.LooseParts.Count; i++)
            {
                LooseParts[i].SetActive(false);
            }
            for (int j = 0; j < this.EnableOnDestroy.Count; j++)
            {
                EnableOnDestroy[j].SetActive(false);
            }
            for (int k = 0; k < this.DisableOnDestroy.Count; k++)
            {
                DisableOnDestroy[k].SetActive(true);
            }
        }
        public void OnDestroy()
        {
            GM.CurrentSceneSettings.ShotFiredEvent -= this.ShotFired;
        }
        private void ShotFired(FVRFireArm firearm)
        {
            if (firearm == this.Firearm)
            {
                this._shotsFired++;
                //this.UpdateMaterials();
                if (this._shotsFired >= this.ShotsToDestroy)
                {
                    this._DestroyThis = true;
                }
            }
        }
        public void Update()
        {
            if (this._DestroyThis && !this._isDestroyed)
            {
                if (this.IsBoltAction && this.UsesSpecialHandling)
                {
                    BoltActionRifle boltActionRifle = this.Firearm as BoltActionRifle;
                    if (boltActionRifle != null)
                    {
                        BoltActionRifle_Handle boltHandle = boltActionRifle.BoltHandle;
                        if (boltHandle.HandleState == BoltActionRifle_Handle.BoltActionHandleState.Rear &&
                            boltHandle.HandleRot == BoltActionRifle_Handle.BoltActionHandleRot.Up)
                        {
                            this.PseudoBoltObject.SetActive(true);
                            this.PseudoBoltObject.transform.SetParent(null);
                            FVRPhysicalObject bolt = this.PseudoBoltObject.GetComponent<FVRPhysicalObject>();
                            FVRViveHand hand = boltHandle.m_hand;
                            boltHandle.ForceBreakInteraction();
                            hand.ForceSetInteractable(bolt);
                            bolt.BeginInteraction(hand);
                            this.Destroy();
                        }
                    }
                }
                else if (this.IsPumpAction && this.UsesSpecialHandling)
                {
                    TubeFedShotgun tubeFedShotgun = this.Firearm as TubeFedShotgun;
                    if (tubeFedShotgun != null)
                    {
                        TubeFedShotgunHandle shotgunHandle = tubeFedShotgun.Handle;
                        if (tubeFedShotgun.Mode == TubeFedShotgun.ShotgunMode.PumpMode &&
                            shotgunHandle.CurPos == TubeFedShotgunHandle.BoltPos.Rear)
                        {
                            this.PseudoPumpObject.SetActive(true);
                            this.PseudoPumpObject.transform.SetParent(null);
                            FVRPhysicalObject pump = this.PseudoPumpObject.GetComponent<FVRPhysicalObject>();
                            FVRViveHand hand = shotgunHandle.m_hand;
                            shotgunHandle.ForceBreakInteraction();
                            hand.ForceSetInteractable(pump);
                            pump.BeginInteraction(hand);
                            this.Destroy();
                        }
                    }
                }
                else this.Destroy();
            }
        }
        private void UpdateMaterials()
        {
            //if (this.GrungeMaterials.Count == 0) return;
            //float _matLerp = Mathf.InverseLerp(0, this.ShotsToDestroy, this._shotsFired);
            //for (int i = 0; i > this.GrungeMaterials.Count; i++)
            //{
            //    if (this.GrungeMaterials[i].Materials.Count == 0 || this.GrungeMaterials[i].Renderers.Count == 0) continue;
            //    int index = Mathf.RoundToInt(_matLerp * this.GrungeMaterials[i].Materials.Count - 1);
            //    for (int j = 0; j > this.GrungeMaterials[i].Renderers.Count; j++)
            //    {
            //        var copyMaterials = this.GrungeMaterials[i].Renderers[j].sharedMaterials;
            //        copyMaterials[i] = this.GrungeMaterials[i].Materials[index];
            //        this.GrungeMaterials[i].Renderers[j].sharedMaterials = copyMaterials;
            //    }
            //}
        }
        private void Destroy()
        {
            this.Firearm.PlayAudioAsHandling(this.AudEventDestroy, this.Firearm.transform.position);
            for (int i = 0; i < this.DisableOnDestroy.Count; i++)
            {
                DisableOnDestroy[i].SetActive(false);
            }
            for (int j = 0; j < this.EnableOnDestroy.Count; j++)
            {
                EnableOnDestroy[j].SetActive(true);
            }
            for (int k = 0; k < this.LooseParts.Count; k++)
            {
                LooseParts[k].SetActive(true);
                Rigidbody rb = LooseParts[k].GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = LooseParts[k].AddComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
                float yVelocity = (UnityEngine.Random.Range(0.1f, 0.5f));
                if (LooseParts[k].transform.localPosition.y < this.Firearm.transform.localPosition.y) yVelocity *= -1;
                float xVelocity = (UnityEngine.Random.Range(0.1f, 0.5f));
                if (LooseParts[k].transform.localPosition.x < this.Firearm.transform.localPosition.x) xVelocity *= -1;
                float zVelocity = (UnityEngine.Random.Range(0.1f, 0.5f));
                if (LooseParts[k].transform.localPosition.z < this.Firearm.transform.localPosition.z) zVelocity *= -1;
                rb.gameObject.transform.SetParent(null);
                rb.velocity = new Vector3(xVelocity, yVelocity, zVelocity);
                rb.angularVelocity = new Vector3(xVelocity, yVelocity, zVelocity);
            }
            this._isDestroyed = true;
            this._DestroyThis = false;
        }

        [ContextMenu("Fill out Mesh Renderers")]
        public void FillMeshRenderes()
        {
            List<Material> materials = new List<Material>();
            List<MeshRenderer> renderers = (from m in base.GetComponentsInChildren<MeshRenderer>() where m.enabled select m).ToList();
            foreach (MeshRenderer renderer in renderers)
            {
                Debug.Log("Found renderer");
                if (!renderers.Contains(renderer)) renderers.Add(renderer);
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Debug.Log("Found material");
                    if (!materials.Contains(renderer.sharedMaterials[i])) materials.Add(renderer.sharedMaterials[i]);
                }
            }
            foreach (Material material in materials)
            {
                GrungeMaterial newGrungeMaterial = new GrungeMaterial();
                Debug.Log("Created new Grunge Material");
                newGrungeMaterial.Materials.Add(material);
                newGrungeMaterial.Renderers = (from rend in renderers where rend.sharedMaterials.Contains(material) select rend).ToList<MeshRenderer>();
                bool addGrungeMat = true;
                foreach (GrungeMaterial grungeMaterial in this.GrungeMaterials)
                {
                    if (grungeMaterial.Materials.Contains(material))
                    {
                        addGrungeMat = false;
                        break;
                    }
                }
                if (addGrungeMat) this.GrungeMaterials.Add(newGrungeMaterial);
                else continue;
            }
        }
    }
}
