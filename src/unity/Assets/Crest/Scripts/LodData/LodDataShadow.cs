﻿using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    public class LodDataShadow : LodData
    {
        public override SimType LodDataType { get { return SimType.Shadow; } }
        public override SimSettingsBase CreateDefaultSettings() { return null; }
        public override void UseSettings(SimSettingsBase settings) { }
        public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.RHalf; } }
        public override CameraClearFlags CamClearFlags { get { return CameraClearFlags.Color; } }
        public override RenderTexture DataTexture { get { return _shadowData; } }

        RenderTexture _shadowData;
        CommandBuffer _bufCopyShadowMap = null;
        Light _mainLight;
        Material _renderMaterial;

        protected override void Start()
        {
            base.Start();

            int res = OceanRenderer.Instance.LodDataResolution;
            _shadowData = new RenderTexture(res, res, 0);
            _shadowData.name = gameObject.name + "_oceanDepth";
            _shadowData.format = TextureFormat;
            _shadowData.useMipMap = false;
            _shadowData.anisoLevel = 0;

            var lightGO = GameObject.Find("Directional light");
            if (lightGO)
            {
                _mainLight = lightGO.GetComponent<Light>();
            }

            _renderMaterial = new Material(Shader.Find("Unlit/ShadowUpdate"));
        }

        private void Update()
        {
            if (!_mainLight) return;

            if (_bufCopyShadowMap == null)
            {
                _bufCopyShadowMap = new CommandBuffer();
                _bufCopyShadowMap.name = "Shadow data";
                _bufCopyShadowMap.SetRenderTarget(_shadowData);
                _bufCopyShadowMap.Blit(Texture2D.blackTexture, _shadowData, _renderMaterial);
                _mainLight.AddCommandBuffer(LightEvent.AfterShadowMap, _bufCopyShadowMap);
            }
        }

        void OnEnable()
        {
            RemoveCommandBuffers();
        }

        void OnDisable()
        {
            RemoveCommandBuffers();
        }

        void RemoveCommandBuffers()
        {
            if (_bufCopyShadowMap != null)
            {
                _mainLight.RemoveCommandBuffer(LightEvent.AfterShadowMap, _bufCopyShadowMap);
                _bufCopyShadowMap = null;
            }
        }
    }
}
