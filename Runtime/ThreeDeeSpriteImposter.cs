using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace ThreeDee
{
    /// <summary>
    /// Flags an object for imposter management within the ThreeDee system.
    /// Imposters of each ThreeDeeSprite can be managed here
    /// </summary>
    public class ThreeDeeSpriteImposter : MonoBehaviour
    {
        /*
        #region Shared
        //Eventually these shared values will be pushed into an SO that will be referenced by this class instead.
        [Tooltip("The RenderTexture to which this imposter will be rendered.")]
        public RenderTexture TargetPrefab;

        [Tooltip("The material that will be duplicated and applied to the billboard.")]
        public Material ImposterBillboardMatPrefab;

        [Tooltip("")]
        public Vector3 Offset = Vector3.zero;

        [Tooltip("")]
        public float Scale = 1;

        [Tooltip("")]
        public Vector3 Rot;

        [Tooltip("If true, this will attempt to bake a skinned mesh renderer on the sprite model. Otherwise it will search for and use a regualr mesh renderer.")]
        public bool SkinnedMesh = true;

        [Tooltip("The animation pose to use for the imposter.")]
        public string DefaultAnim = "Idle Stand 01";
        #endregion
        */

        [Tooltip("A reference to an ImposterAsset containing common state data that may be shared by many different imposters.")]
        public ImposterAsset SharedData;

        [Tooltip("A reference to the renderer that will display the imposter billboard. This should NOT be the same renderer or mesh used by the ThreeDeeSpriteRenderer itself!")]
        public MeshRenderer ImposterBillboardRenderer;

        RenderTexture BillboardTarget;



        private IEnumerator Start()
        {
            yield return null;
            ImposterBillboardRenderer.sharedMaterial = GenerateTexture();
            GenerateImposter();
        }

        static int index = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Material GenerateTexture()
        {
            BillboardTarget = new RenderTexture(SharedData.TargetPrefab);
            BillboardTarget.filterMode = SharedData.TargetPrefab.filterMode; //this isn't being duped properly

            var dupedMat = new Material(SharedData.ImposterBillboardMatPrefab);
            dupedMat.name = SharedData.ImposterBillboardMatPrefab.name + $" (duped {index++})";
            foreach (var id in ThreeDeeSpriteSurface.MainTexIds)
            {
                if (dupedMat.HasTexture(id))
                    dupedMat.SetTexture(id, BillboardTarget);
            }

            return dupedMat;
        }

        /// <summary>
        /// Generates the imposter.
        /// </summary>
        [Button("Generate Imposter")]
        public void GenerateImposter()
        {
            var tdsRend = GetComponentInChildren<IThreeDeeSpriteRenderer>(true);
            var skinRend = tdsRend.ModelTrans.GetComponentInChildren<SkinnedMeshRenderer>(true);

            //setup the animator state and pump it. that way when processing the issued command we can bake the correct anim
            var anim = tdsRend.ModelTrans.GetComponentInChildren<Animator>(true);
            var info = anim.GetCurrentAnimatorStateInfo(0);
            bool oldState = tdsRend.ModelTrans.gameObject.activeSelf;
            var oldCull = anim.cullingMode;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (!tdsRend.ModelTrans.gameObject.activeSelf)
                tdsRend.ModelTrans.gameObject.SetActive(true);
            anim.Play(SharedData.DefaultAnim, 0);
            anim.Update(1f);

            ImposterPreRenderer.Instance.IssueCommand(skinRend, BillboardTarget, SharedData.Offset, SharedData.Scale, SharedData.Rot);

            //restore
            anim.Play(info.fullPathHash);
            anim.Update(0);
            anim.cullingMode = oldCull;
            if (tdsRend.ModelTrans.gameObject.activeSelf != oldState)
                tdsRend.ModelTrans.gameObject.SetActive(oldState);
        }


    }
}
