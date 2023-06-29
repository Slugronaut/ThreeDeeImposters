using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace ThreeDee
{
    /// <summary>
    /// Flags an object for imposter management within the ThreeDee system.
    /// Imposters of each ThreeDeeSprite can be managed here
    /// </summary>
    public class ThreeDeeSpriteImposter : MonoBehaviour
    {

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

            //make sure our 3D model is active and then setup the animator state and pump it.
            //that way when processing the issued command we can bake the correct anim
            var anim = tdsRend.ModelTrans.GetComponentInChildren<Animator>(true);
            var info = anim.GetCurrentAnimatorStateInfo(0);
            bool oldState = tdsRend.ModelTrans.gameObject.activeSelf;
            var oldCull = anim.cullingMode;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (!tdsRend.ModelTrans.gameObject.activeSelf)
                tdsRend.ModelTrans.gameObject.SetActive(true);
            anim.Play(SharedData.DefaultAnim, 0);
            anim.Update(1f); //pump the animator

            //bake a new mesh using the state of the real one then setup initial rendering data
            var tempMesh = new Mesh();
            skinRend.BakeMesh(tempMesh, true);
            var meshW = Matrix4x4.TRS(SharedData.Offset, Quaternion.Euler(SharedData.Rot), Vector3.one * SharedData.Scale);
            var cameraVP = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 100);

            //render all submeshes
            BlitMesh(BillboardTarget, tempMesh, skinRend.sharedMaterials, meshW, cameraVP);

            //restore the real mesh to its previous state and cleanup
            anim.Play(info.fullPathHash);
            anim.Update(0);
            anim.cullingMode = oldCull;
            if (tdsRend.ModelTrans.gameObject.activeSelf != oldState)
                tdsRend.ModelTrans.gameObject.SetActive(oldState);
            Destroy(tempMesh);
        }

        /// <summary>
        /// Helper for drawing a one-off mesh to a render texture. This will render all submeshes and materials associated with it.
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <param name="meshW"></param>
        /// <param name="cameraVP"></param>
        /// <param name="subMeshIndex"></param>
        public static void BlitMesh(RenderTexture rt, Mesh mesh, Material[] materials, Matrix4x4 meshW, Matrix4x4 cameraVP)
        {
            //sometimes this will not be null and screw us.
            //special thanks goes to a random github and someone by the name of @guycalledfrank on the internet for figuring this out
            if (Camera.current != null)
                cameraVP *= Camera.current.worldToCameraMatrix.inverse;

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = rt; bool oldCulling = GL.invertCulling;
            GL.invertCulling = true; //another wonderful contribution from the internet!! no idea why but it works!!

            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(cameraVP);

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                materials[i].SetPass(0);
                Graphics.DrawMeshNow(mesh, meshW, i);
            }

            //now set everything back to the way it was before
            GL.PopMatrix();
            GL.invertCulling = oldCulling;
            RenderTexture.active = oldRt;

        }

        /// <summary>
        /// Helper for drawing a one-off mesh to a render texture.
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <param name="meshW"></param>
        /// <param name="cameraVP"></param>
        /// <param name="subMeshIndex"></param>
        public static void BlitMesh(RenderTexture rt, Mesh mesh, Material material, Matrix4x4 meshW, Matrix4x4 cameraVP, int subMeshIndex = 0)
        {
            //sometimes this will not be null and screw us.
            //special thanks goes to a random github and someone by the name of @guycalledfrank on the internet for figuring this out
            if (Camera.current != null)
                cameraVP *= Camera.current.worldToCameraMatrix.inverse;

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            bool oldCulling = GL.invertCulling;
            GL.invertCulling = true; //another wonderful contribution from the internet!! no idea why but it works!!

            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(cameraVP);

            material.SetPass(0);
            Graphics.DrawMeshNow(mesh, meshW, subMeshIndex);

            //now set everything back to the way it was before
            GL.PopMatrix();
            GL.invertCulling = oldCulling;
            RenderTexture.active = oldRt;

        }
    }


}
