using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// URP Renderer Feature for underwater post-process effect.
/// Applies blue tint and pixelated wave distortion when camera is below water level.
/// </summary>
public class UnderwaterRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Underwater post-process shader material")]
        public Material underwaterMaterial;

        [Tooltip("Y position of water surface")]
        public float waterLevel = 0f;

        [Header("Tint")]
        [Tooltip("Underwater color tint")]
        public Color tintColor = new Color(0.2f, 0.4f, 0.8f, 1f);

        [Tooltip("Tint strength (0 = none, 1 = full)")]
        [Range(0f, 1f)] public float tintStrength = 0.4f;

        [Header("Distortion")]
        [Tooltip("Wave distortion strength")]
        [Range(0f, 0.1f)] public float distortionStrength = 0.02f;

        [Tooltip("Wave distortion speed")]
        public float distortionSpeed = 2f;

        [Tooltip("Distortion block size in pixels (for retro look)")]
        [Range(1, 32)] public int blockSize = 8;

        [Tooltip("Enable/disable the effect at runtime")]
        public bool isEnabled = false;
    }

    public Settings settings = new Settings();
    UnderwaterRenderPass _pass;

    static UnderwaterRendererFeature _instance;
    public static UnderwaterRendererFeature Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFeatureInPipeline();
            return _instance;
        }
    }

    static UnderwaterRendererFeature FindFeatureInPipeline()
    {
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null) return null;

        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(pipeline) is ScriptableRendererData[] rendererDataList)
        {
            foreach (var rendererData in rendererDataList)
            {
                if (rendererData == null) continue;
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature is UnderwaterRendererFeature underwater)
                        return underwater;
                }
            }
        }
        return null;
    }

    public override void Create()
    {
        _pass = new UnderwaterRenderPass(settings);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled || settings.underwaterMaterial == null)
            return;

        // Only apply if camera is below water level
        if (renderingData.cameraData.camera.transform.position.y >= settings.waterLevel)
            return;

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _instance = null;
    }

    // Runtime control methods
    public void SetEnabled(bool enabled) => settings.isEnabled = enabled;
    public void SetWaterLevel(float level) => settings.waterLevel = level;
    public void SetTintColor(Color color) => settings.tintColor = color;
    public void SetTintStrength(float strength) => settings.tintStrength = Mathf.Clamp01(strength);
    public void SetDistortionStrength(float strength) => settings.distortionStrength = strength;
    public void SetBlockSize(int size) => settings.blockSize = Mathf.Clamp(size, 1, 32);
    public bool IsEnabled => settings.isEnabled;
    public float WaterLevel => settings.waterLevel;

    class UnderwaterRenderPass : ScriptableRenderPass
    {
        Settings _settings;

        static readonly int TintColorID = Shader.PropertyToID("_TintColor");
        static readonly int TintStrengthID = Shader.PropertyToID("_TintStrength");
        static readonly int DistortionStrengthID = Shader.PropertyToID("_DistortionStrength");
        static readonly int DistortionSpeedID = Shader.PropertyToID("_DistortionSpeed");
        static readonly int BlockSizeID = Shader.PropertyToID("_BlockSize");
        static readonly int ResolutionID = Shader.PropertyToID("_Resolution");

        public UnderwaterRenderPass(Settings settings)
        {
            _settings = settings;
            profilingSampler = new ProfilingSampler("Underwater");
        }

        public void Dispose() { }

        class PassData
        {
            internal TextureHandle source;
            internal Material material;
            internal Settings settings;
            internal Vector2 resolution;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_UnderwaterTex", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Underwater Pass", out var passData, profilingSampler))
            {
                passData.source = source;
                passData.material = _settings.underwaterMaterial;
                passData.settings = _settings;
                passData.resolution = new Vector2(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetColor(TintColorID, data.settings.tintColor);
                    data.material.SetFloat(TintStrengthID, data.settings.tintStrength);
                    data.material.SetFloat(DistortionStrengthID, data.settings.distortionStrength);
                    data.material.SetFloat(DistortionSpeedID, data.settings.distortionSpeed);
                    data.material.SetFloat(BlockSizeID, data.settings.blockSize);
                    data.material.SetVector(ResolutionID, data.resolution);

                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = destination;
        }
    }
}
