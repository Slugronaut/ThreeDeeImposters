using UnityEngine;

namespace ThreeDee
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "Imposter Asset", menuName = "ThreeDeeSprites/Imposter Asset")]
    public class ImposterAsset : ScriptableObject
    {
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
    }
}
