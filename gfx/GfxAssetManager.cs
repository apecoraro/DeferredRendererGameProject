using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Xclna.Xna.Animation;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class GfxAssetManager : core.GraphicsAssetManager
    {
        public List<string> LoadModelList = new List<string>();
        public List<string> LoadTextureList = new List<string>();
        public List<string> LoadFontList = new List<string>();

        ShadowMapEffect _baseShadowMapEffect;

        public ShadowMapEffect BaseShadowMapEffect
        {
            get { return _baseShadowMapEffect; }
        }

        List<ShadowMapEffect> _effects = new List<ShadowMapEffect>();
        Dictionary<string, Model> _models = new Dictionary<string, Model>();
        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();

        ContentManager _contentManager = null;

        public GfxAssetManager()
        {
        }

        public void SetContentManager(ContentManager content)
        {
            _contentManager = content;
        }

        #region ModelLoading

        public void LoadModels(string shadowMapEffectPath)
        {
            _baseShadowMapEffect = new ShadowMapEffect(_contentManager.Load<Effect>(shadowMapEffectPath));

            _baseShadowMapEffect.SetDefaultTechnique("ShadowedScene");
            //_shadowMapEffect.SetDefaultTechnique("PerPixelLighting");
            _baseShadowMapEffect.SetShadowMapTechnique("ShadowMap");
            
            Texture2D lightShapeTexture = _contentManager.Load<Texture2D>("Textures\\LightShape");
            //this is a shared effect param
            _baseShadowMapEffect.Parameters["LightShapeTexture"].SetValue(lightShapeTexture);

            foreach (string file in LoadModelList)
            {
                Model model = LoadModel(file);
                _models.Add(file, model);
            }
        }

        protected Model LoadModel(string file)
        {
            Model model = _contentManager.Load<Model>(file);
            ReplaceEffectsWithShadowMapEffect(model, _baseShadowMapEffect);
            return model;
        }

        protected void ReplaceEffectsWithShadowMapEffect(Model model, ShadowMapEffect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect is BasicPaletteEffect)
                    {
                        BasicPaletteEffect oldEffect = (BasicPaletteEffect)part.Effect;
                        ShadowMapEffect newEffect = GetOrCreateShadowMapEffect(oldEffect, effect);
                        part.Effect = newEffect;
                    }
                    else if (part.Effect is BasicEffect)
                    {
                        BasicEffect oldEffect = (BasicEffect)part.Effect;
                        ShadowMapEffect newEffect = GetOrCreateShadowMapEffect(oldEffect, effect);
                        part.Effect = newEffect;
                    }
                }
            }
        }

        public ShadowMapEffect GetOrCreateShadowMapEffect(BasicPaletteEffect copyMe, ShadowMapEffect prototype)
        {
            prototype.CopyBasicPaletteEffect(copyMe);
            prototype.SetDefaultTechnique("ShadowedSkinnedModel");
            prototype.SetShadowMapTechnique("SkinnedShadowMap");
            foreach (ShadowMapEffect matchMe in _effects)
            {
                if (matchMe.Equals(prototype))
                    return matchMe;
            }
            _effects.Add((ShadowMapEffect)prototype.Clone(prototype.GraphicsDevice));
            _effects[_effects.Count - 1].SetID(_effects.Count - 1);
            return _effects[_effects.Count - 1];
        }

        public ShadowMapEffect GetOrCreateShadowMapEffect(BasicEffect copyMe, ShadowMapEffect prototype)
        {
            prototype.CopyBasicEffect(copyMe);
            prototype.SetDefaultTechnique("ShadowedScene");
            prototype.SetShadowMapTechnique("ShadowMap");
            foreach (ShadowMapEffect matchMe in _effects)
            {
                if (matchMe.Equals(prototype))
                    return matchMe;
            }
            _effects.Add((ShadowMapEffect)prototype.Clone(prototype.GraphicsDevice));
            _effects[_effects.Count - 1].SetID(_effects.Count - 1);
            return _effects[_effects.Count - 1];
        }

        public ShadowMapEffect GetOrCreateShadowMapEffect(Vector3 diffuseColor, float alpha)
        {
            _baseShadowMapEffect.InitDefaults();
            _baseShadowMapEffect.DiffuseColor = diffuseColor;
            _baseShadowMapEffect.Alpha = alpha;
            _baseShadowMapEffect.SetDefaultTechnique("ShadowedScene");
            _baseShadowMapEffect.SetShadowMapTechnique("ShadowMap");

            foreach (ShadowMapEffect matchMe in _effects)
            {
                if (matchMe.Equals(_baseShadowMapEffect))
                    return matchMe;
            }
            _effects.Add((ShadowMapEffect)_baseShadowMapEffect.Clone(_baseShadowMapEffect.GraphicsDevice));
            _effects[_effects.Count - 1].SetID(_effects.Count - 1);
            return _effects[_effects.Count - 1];

        }

        public Model GetOrCreateModel(string path)
        {
            Model model;
            if (_models.TryGetValue(path, out model))
            {
                return model;
            }

            model = LoadModel(path);
            _models.Add(path, model);
            return model;
        }

        #endregion
        #region TextureLoading
        public void LoadTextures()
        {
            foreach (string file in LoadTextureList)
            {
                Texture2D texture = LoadTexture(file);
                _textures.Add(file, texture);
            }
        }

        protected Texture2D LoadTexture(string file)
        {
            Texture2D texture = _contentManager.Load<Texture2D>(file);
            return texture;
        }

        public Texture2D GetOrCreateTexture(string path)
        {
            Texture2D texture;
            if (_textures.TryGetValue(path, out texture))
            {
                return texture;
            }

            texture = LoadTexture(path);
            _textures.Add(path, texture);
            return texture;
        }
        #endregion

        #region SpriteFontLoading
        public void LoadSpriteFonts()
        {
            foreach (string file in LoadFontList)
            {
                SpriteFont font = LoadSpriteFont(file);
                _fonts.Add(file, font);
            }
        }

        protected SpriteFont LoadSpriteFont(string file)
        {
            SpriteFont font = _contentManager.Load<SpriteFont>(file);
            return font;
        }

        public SpriteFont GetOrCreateSpriteFont(string path)
        {
            SpriteFont font;
            if (_fonts.TryGetValue(path, out font))
            {
                return font;
            }

            font = LoadSpriteFont(path);
            _fonts.Add(path, font);
            return font;
        }
        #endregion
    }
}
