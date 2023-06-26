using Sirenix.OdinInspector.Editor.Validation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ThreeDee
{
    /// <summary>
    /// 
    /// </summary>
    public class ImposterPreRenderer : MonoBehaviour
    {
        public class RenderImposterCommand
        {
            public Mesh Mesh;
            public Material[] Mats;
            public RenderTexture Target;
            public Vector3 Pos = Vector3.zero;
            public Vector3 Scale = Vector3.one;
            public Vector3 Rot;
        }

        #region Public Fields and Props
        [Tooltip("The camera that will be used to render the mesh.")]
        public Camera Cam;

        [Tooltip("The layer to draw the 3D mesh on.")]
        public int Layer;

        [Tooltip("The material to apply to the 3D mesh that is being pre-rendered.")]
        public Material ThreeDeeModelMat;

        public static ImposterPreRenderer Instance { get; private set; }
        #endregion


        Queue<RenderImposterCommand> Commands;


        #region Unity Events
        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            Commands = new(128);
            Instance = this;
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += RenderPendingCommands;
            RenderPipelineManager.endFrameRendering += CleanupCommands;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= RenderPendingCommands;
            RenderPipelineManager.endFrameRendering -= CleanupCommands;
        }
        #endregion



        /// <summary>
        /// Queues a command to pre-render a skinned mesh to the given target texture during the next rendering update.
        /// </summary>
        /// <param name="rend"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        public void IssueCommand(SkinnedMeshRenderer rend, RenderTexture target, Vector3 offset, float scale, Vector3 rot)
        {
            //the animator should already be in the correct state at this point so just bake and go with it
            var bakedMesh = new Mesh();
            rend.BakeMesh(bakedMesh, true);

            Commands.Enqueue(new RenderImposterCommand()
            {
                Mesh = bakedMesh,
                Mats = rend.sharedMaterials,
                Target = target,
                Pos = offset,
                Scale = new Vector3(scale, scale, scale),
                Rot = rot,
            });

            Cam.enabled = true;
        }

        /// <summary>
        /// Queues a command to pre-render a mesh to the given target texture during the next rendering update.
        /// </summary>
        /// <param name="rend"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        public void IssueCommand(MeshFilter mesh, Material[] mats, RenderTexture target, Vector3 offset, float scale, Vector3 rot)
        {
            Commands.Enqueue(new RenderImposterCommand()
            {
                Mesh = mesh.sharedMesh,
                Mats = mats,
                Target = target,
                Pos = offset,
                Scale = new Vector3(scale, scale, scale),
                Rot = rot,
            });

            Cam.enabled = true;
        }

        void RenderPendingCommands(ScriptableRenderContext context, Camera camera)
        {
            if (camera == Cam)
                RenderPendingCommands(context);
        }

        void RenderPendingCommands(ScriptableRenderContext context)
        {
            //due to limitations in Unity we cannot process more than one command per frame
            //otherwise they'd all end up on the last rendertarget set.
            RenderCommand(context, Commands.Dequeue());
        }

        void RenderCommand(ScriptableRenderContext context, RenderImposterCommand com)
        {
            Cam.targetTexture = com.Target;
            Matrix4x4 meshW = Matrix4x4.TRS(com.Pos, Quaternion.Euler(com.Rot), com.Scale);
            for (int subIndex = 0; subIndex < com.Mesh.subMeshCount; subIndex++)
            {
                var mat = com.Mats[subIndex];
                Graphics.DrawMesh(com.Mesh, meshW, mat, Layer, Cam, subIndex);
            }
        }

        void CleanupCommands(ScriptableRenderContext context, Camera[] cameras)
        {
            //at the end of the frame if we have no commands left, we can finally disable the camera, thus stopping
            //this script from running until a new command is issued.
            if(Commands.Count == 0)
                Cam.enabled = false;
        }

    }
}
