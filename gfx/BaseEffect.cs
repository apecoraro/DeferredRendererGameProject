using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;

namespace DeskWars.gfx
{
    /// <summary>
    /// This class is designed to be used with the ShadowMapEffect.fx effect, which implements dynamic
    /// shadows via shadow maps.
    /// </summary>
    /// <remarks>
    /// [Class additional details]
    /// </remarks>
    public class BaseEffect : Effect
    {
        EffectParameter _world;
        EffectParameter _view;
        EffectParameter _projection;
        EffectParameter _worldViewProjection;
        EffectParameter _diffuseColor;
        EffectParameter _specularColor;
        EffectParameter _specularPower;
        EffectParameter _emissiveColor;
        EffectParameter _alpha;
        EffectParameter _basicTexture;
        EffectParameter _textureEnabled;
        EffectParameter _activeLightCount;
        EffectParameter _lights;
        EffectParameter _ambientLightColor;
        EffectParameter _eyePosition;
        EffectTechnique _defaultTechnique;
        int _effectID;

        public BaseEffect(GraphicsDevice device, Effect copy)
            : base(device, copy)
        {
            InitEffectParams();

            InitDefaults();
        }

        public void SetID(int id)
        {
            _effectID = id;
        }

        public int GetID()
        {
            return _effectID;
        }

        public bool UseDefaultTechnique()
        {
            return this.CurrentTechnique == _defaultTechnique;
        }
        
        public BaseEffect(BaseEffect prototype) : base(prototype.GraphicsDevice, prototype) 
        {
            this.InitEffectParams();
            CopyBaseEffect(prototype);
        }

        public void CopyBaseEffect(BaseEffect copy)
        {
            this.Alpha = copy.Alpha;
            this.DiffuseColor = copy.DiffuseColor;
            this.SpecularColor = copy.SpecularColor;
            this.SpecularPower = copy.SpecularPower;
            this.Texture = copy.Texture;
            this._defaultTechnique = copy._defaultTechnique;
        }

        public void CopyBasicEffect(BasicEffect copy)
        {
            this.Alpha = copy.Alpha;
            this.DiffuseColor = copy.DiffuseColor;
            this.SpecularColor = copy.SpecularColor;
            this.SpecularPower = copy.SpecularPower;
            this.Texture = copy.Texture;
        }

        public void CopyBasicPaletteEffect(BasicPaletteEffect copy)
        {
            this.Alpha = copy.Alpha;
            this.DiffuseColor = copy.DiffuseColor;
            this.SpecularColor = copy.SpecularColor;
            this.SpecularPower = copy.SpecularPower;
            this.Texture = copy.Texture;
        }

        public override bool Equals(object obj)
        {
            BaseEffect compare = (BaseEffect)obj;
            if (this.Alpha != compare.Alpha)
                return false;
            if (this.DiffuseColor != compare.DiffuseColor)
                return false;
            if (this.SpecularColor != compare.SpecularColor)
                return false;
            if (this.SpecularPower != compare.SpecularPower)
                return false;
            if (this.Texture != compare.Texture)
                return false;

            return true;
        }

        protected void InitEffectParams()
        {
            _world = this.Parameters["World"];

            _view = this.Parameters["View"];

            _projection = this.Parameters["Projection"];

            _worldViewProjection = this.Parameters["WorldViewProjection"];

            _diffuseColor = this.Parameters["DiffuseColor"];
            
            _specularColor = this.Parameters["SpecularColor"];
            
            _specularPower = this.Parameters["SpecularPower"];
            
            _emissiveColor = this.Parameters["EmissiveColor"];
            
            _alpha = this.Parameters["Alpha"];
            
            _basicTexture = this.Parameters["BasicTexture"];
            _textureEnabled = this.Parameters["TextureEnabled"];
            
            _activeLightCount = this.Parameters["ActiveLightCount"];
            _lights = this.Parameters["Lights"];
            _ambientLightColor = this.Parameters["AmbientLightColor"];
            _eyePosition = this.Parameters["EyePosition"];
        }

        public void InitDefaults()
        {
            DiffuseColor = Vector3.One;
            SpecularColor = new Vector3(0.01f, 0.01f, 0.01f);
            SpecularPower = 1.0f;
            EmissiveColor = Vector3.Zero;
            Alpha = 1.0f;
            Texture = null;
        }

        public virtual void Configure(BaseEffectConfig configData)
        {
            this.ActiveLightCount = configData.ActiveLightCount;

            for (int index = 0; index < configData.ActiveLightCount; ++index)
            {
                this.SetLightView(index, configData.Lights[index].View);
                this.SetLightProjection(index, configData.Lights[index].Projection);
                this.SetLightPosition(index, configData.Lights[index].Position);
                this.SetLightColor(index, configData.Lights[index].Color);
            }

            this.AmbientLightColor = configData.AmbientLightColor;
            this.EyePosition = configData.EyePosition;
        }

        public void SetState(ComponentState state,
                             Matrix worldMat,
                             Matrix viewMat,
                             Matrix projMat,
                             GameTime gameTime)
        {
            this.World = worldMat;
            this.View = viewMat;
            this.Projection = projMat;
            this.WorldViewProjection = worldMat * viewMat * projMat;
            state.Apply(this);
            this.EmissiveColor = Vector3.Zero;
        }

        public void SetDefaultTechnique(string defTechName)
        {
            _defaultTechnique = this.Techniques[defTechName];
        }

        public void EnableDefaultTechnique()
        {
            this.CurrentTechnique = _defaultTechnique;
        }

        public Matrix World
        {
            get { return _world.GetValueMatrix(); }
            set { _world.SetValue(value); }
        }

        public Matrix View
        {
            get { return _view.GetValueMatrix(); }
            set { _view.SetValue(value); }
        }

        public Matrix Projection
        {
            get { return _projection.GetValueMatrix(); }
            set { _projection.SetValue(value); }
        }

        public Matrix WorldViewProjection
        {
            get { return _worldViewProjection.GetValueMatrix(); }
            set { _worldViewProjection.SetValue(value); }
        }

        public Texture2D Texture
        {
            get { return _basicTexture.GetValueTexture2D(); }
            set
            {
                _basicTexture.SetValue(value);
                _textureEnabled.SetValue(value != null);
            }
        }

        public Vector3 DiffuseColor
        {
            get { return _diffuseColor.GetValueVector3(); }
            set { _diffuseColor.SetValue(value); }
        }

        public Vector3 EmissiveColor
        {
            get { return _emissiveColor.GetValueVector3(); }
            set { _emissiveColor.SetValue(value); }
        }

        public Vector3 SpecularColor
        {
            get { return _specularColor.GetValueVector3(); }
            set { _specularColor.SetValue(value); }
        }

        public Vector3 AmbientLightColor
        {
            get { return _ambientLightColor.GetValueVector3(); }
            set { _ambientLightColor.SetValue(value); }
        }

        public Vector3 EyePosition
        {
            get { return _eyePosition.GetValueVector3(); }
            set { _eyePosition.SetValue(value); }
        }

        public float SpecularPower
        {
            get { return _specularPower.GetValueSingle(); }
            set { _specularPower.SetValue(value); }
        }

        public float Alpha
        {
            get { return _alpha.GetValueSingle(); }
            set { _alpha.SetValue(value); }
        }

        public int ActiveLightCount
        {
            get { return _activeLightCount.GetValueInt32(); }
            set { _activeLightCount.SetValue(value); }
        }

        public Vector3 GetLightPosition(int index)
        {
            return _lights.Elements[index].StructureMembers["Position"].GetValueVector3();
        }

        public void SetLightPosition(int index, Vector3 position)
        {
            _lights.Elements[index].StructureMembers["Position"].SetValue(position);
        }

        public Vector3 GetLightColor(int index)
        {
            return _lights.Elements[index].StructureMembers["Color"].GetValueVector3();
        }

        public void SetLightColor(int index, Vector3 color)
        {
            _lights.Elements[index].StructureMembers["Color"].SetValue(color);
        }

        public Matrix GetLightView(int index)
        {
            return _lights.Elements[index].StructureMembers["View"].GetValueMatrix();
        }

        public void SetLightView(int index, Matrix view)
        {
            _lights.Elements[index].StructureMembers["View"].SetValue(view);
        }

        public Matrix GetLightProjection(int index)
        {
            return _lights.Elements[index].StructureMembers["Projection"].GetValueMatrix();
        }

        public void SetLightProjection(int index, Matrix projection)
        {
            _lights.Elements[index].StructureMembers["Projection"].SetValue(projection);
        }
    }
}
