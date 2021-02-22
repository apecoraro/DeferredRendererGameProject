using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class ShadowMapEffect : gfx.BaseEffect
    {
        EffectParameter[] _shadowMaps;
        //EffectParameter _cameraPos;
        EffectParameter _depthBias;
        EffectTechnique _shadowMapTechnique = null;

        public ShadowMapEffect(GraphicsDevice device, ShadowMapEffect copy) : base(copy)
        {
            InitializeEffectParameters();

            this.ActiveLightCount = copy.ActiveLightCount;
            for (int i = 0; i < this.ActiveLightCount; ++i)
            {
                this.SetLightPosition(i, copy.GetLightPosition(i));
                this.SetLightColor(i, copy.GetLightColor(i));
                this.SetLightView(i, copy.GetLightView(i));
                this.SetLightProjection(i, copy.GetLightProjection(i));
            }

            this._shadowMapTechnique = copy._shadowMapTechnique;
            //this.AmbientLightColor = copy.AmbientLightColor;
            //this.DepthBias = copy.DepthBias;
        }

        public override Effect Clone(GraphicsDevice device)
        {
            return new ShadowMapEffect(device, this);
        }

        public override bool Equals(object obj)
        {
            ShadowMapEffect compare = (ShadowMapEffect)obj;
            if (_shadowMapTechnique != compare._shadowMapTechnique)
                return false;
            return base.Equals(obj);
        }

        protected void InitializeEffectParameters()
        {
            _shadowMaps = new EffectParameter[2];
            _shadowMaps[0] = this.Parameters["ShadowMap0"];
            _shadowMaps[1] = this.Parameters["ShadowMap1"];
            //_cameraPos = this.Parameters["g_CamPos"];
            _depthBias = this.Parameters["ShadowMapDepthBias"];
            //_shadowFactor = this.Parameters["g_ShadowFactor"];
        }

        public override void Configure(BaseEffectConfig configData)
        {
            this.ActiveLightCount = configData.ActiveLightCount;

            for (int index = 0; index < configData.ActiveLightCount; ++index)
            {
                this.SetLightView(index, configData.Lights[index].View);
                this.SetLightProjection(index, configData.Lights[index].Projection);
                this.SetLightPosition(index, configData.Lights[index].Position);
                this.SetLightColor(index, configData.Lights[index].Color);
                this.SetShadowMap(index, configData.Lights[index].ShadowMap);
            }

            this.AmbientLightColor = configData.AmbientLightColor;
            this.EyePosition = configData.EyePosition;
            this.View = configData.ViewMat;
            this.Projection = configData.ProjMat;
        }

        public void SetShadowMapTechnique(string name)
        {
            _shadowMapTechnique = this.Techniques[name];
        }

        public void EnableShadowMapTechnique()
        {
            this.CurrentTechnique = _shadowMapTechnique;
        }

        public ShadowMapEffect(Effect prototype) : base(prototype.GraphicsDevice, prototype) 
        {
            InitializeEffectParameters();
        }

        /*public Vector3 CameraPosition
        {
            get { return _cameraPos.GetValueVector3(); }
            set { _cameraPos.SetValue(value); }
        }*/

        public float DepthBias
        {
            get { return _depthBias.GetValueSingle(); }
            set { _depthBias.SetValue(value); }
        }

        public Texture2D GetShadowMap(int index)
        {
            return _shadowMaps[index].GetValueTexture2D();
        }

        public void SetShadowMap(int index, Texture2D value)
        {
            _shadowMaps[index].SetValue(value);
        }
    }
}