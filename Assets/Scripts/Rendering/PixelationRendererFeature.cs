using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// URP Renderer Feature that applies a pixelation post-process effect.
/// Uses RenderGraph API (Unity 6+).
///
/// Supports two modes:
/// 1. Static: Always-on at fixed resolution (e.g., DOOM at 320x200)
/// 2. Animated: Block count controlled at runtime (e.g., menu transitions)
/// </summary>
public class PixelationRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Pixelation shader material")]
        public Material pixelationMaterial;

        [Tooltip("Number of pixel blocks horizontally (e.g., 320 for Atari ST resolution)")]
        [Range(32, 640)] public int blockCountX = 320;

        [Tooltip("Enable/disable the effect at runtime")]
        public bool isEnabled = false;
    }

    public Settings settings = new Settings();
    PixelationRenderPass _pass;

    // Static instance for easy runtime access
    static PixelationRendererFeature _instance;

    /// <summary>
    /// Gets the PixelationRendererFeature instance. Uses cached instance or finds via reflection.
    /// </summary>
    public static PixelationRendererFeature Instance
    {
        get
        {
            if (_instance != null) return _instance;

            // Fallback: find via URP pipeline
            _instance = FindFeatureInPipeline();
            return _instance;
        }
    }

    static PixelationRendererFeature FindFeatureInPipeline()
    {
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null) return null;

        // Access renderer data list via reflection
        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(pipeline) is ScriptableRendererData[] rendererDataList)
        {
            foreach (var rendererData in rendererDataList)
            {
                if (rendererData == null) continue;
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature is PixelationRendererFeature pixelation)
                        return pixelation;
                }
            }
        }
        return null;
    }

    public override void Create()
    {
        _pass = new PixelationRenderPass(settings);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled || settings.pixelationMaterial == null)
            return;

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _instance = null;
    }

    /// <summary>
    /// Runtime control: Set the horizontal block count (vertical is computed from aspect ratio)
    /// </summary>
    public void SetBlockCount(int blockCountX)
    {
        settings.blockCountX = Mathf.Clamp(blockCountX, 32, 640);
    }

    /// <summary>
    /// Runtime control: Enable/disable pixelation
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        settings.isEnabled = enabled;
    }

    /// <summary>
    /// Get current block count
    /// </summary>
    public int GetBlockCount() => settings.blockCountX;

    /// <summary>
    /// Check if pixelation is enabled
    /// </summary>
    public bool IsEnabled => settings.isEnabled;

    class PixelationRenderPass : ScriptableRenderPass
    {
        Settings _settings;
        static readonly int BlockCountID = Shader.PropertyToID("_BlockCount");
        static readonly int BlockSizeID = Shader.PropertyToID("_BlockSize");

        public PixelationRenderPass(Settings settings)
        {
            _settings = settings;
            profilingSampler = new ProfilingSampler("Pixelation");
        }

        public void Dispose() { }

        // Unity 6 RenderGraph API
        class PassData
        {
            internal TextureHandle source;
            internal Material material;
            internal Vector2 blockCount;
            internal Vector2 blockSize;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            // Compute block count respecting aspect ratio
            float aspect = (float)cameraData.camera.pixelWidth / cameraData.camera.pixelHeight;
            Vector2 blockCount = new Vector2(
                _settings.blockCountX,
                Mathf.RoundToInt(_settings.blockCountX / aspect)
            );
            Vector2 blockSize = new Vector2(1f / blockCount.x, 1f / blockCount.y);

            // Create destination texture
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_PixelationTex", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Pass", out var passData, profilingSampler))
            {
                passData.source = source;
                passData.material = _settings.pixelationMaterial;
                passData.blockCount = blockCount;
                passData.blockSize = blockSize;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetVector(BlockCountID, data.blockCount);
                    data.material.SetVector(BlockSizeID, data.blockSize);
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            // Swap to make destination the new camera color
            resourceData.cameraColor = destination;
        }
    }
}
