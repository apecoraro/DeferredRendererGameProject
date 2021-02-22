using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DeskWars.gfx.Animation;
using Xclna.Xna.Animation;
using DeskWars.core;
using DPSF;
using System.Xml;

namespace DeskWars.gfx
{
    public class Engine : DeskWars.core.Engine, DeskWars.core.Renderer
    {
        GraphicsDevice _graphicsDevice;
        CameraComponent _mainCamera;
        
        //gui.TextureWidget _shadowMapTextureWidget;

        BaseEffectConfig _effectConfig;

        gfx.ComponentTypeFilter _gfxCompFilter = new gfx.ComponentTypeFilter();
        List<gfx.ComponentTypeFilter.GameObjectGfxCompPair> _shadowCasters =
                            new List<gfx.ComponentTypeFilter.GameObjectGfxCompPair>();
        core.GameObjectTypeFilter _filter;
        GfxAssetManager _gfxAssetManager = null;

        public class RenderBin
        {
            Dictionary<int, List<Drawable.DrawablePart>> _drawablesOrganizedByEffect = 
                                        new Dictionary<int, List<Drawable.DrawablePart>>();

            public void AddDrawable(Drawable drawable)
            {
                List<Drawable.DrawablePart> list;
                for(int i = 0; i < drawable.DrawableParts.Length; ++i)
                {
                    if(!_drawablesOrganizedByEffect.TryGetValue(drawable.DrawableParts[i].Effect.GetID(), out list))
                    {
                        list = new List<Drawable.DrawablePart>();
                        _drawablesOrganizedByEffect.Add(drawable.DrawableParts[i].Effect.GetID(), list); 
                    }
                    list.Add(drawable.DrawableParts[i]);       
                }
            }

            public void Reset()
            {
                IEnumerator<KeyValuePair<int, List<Drawable.DrawablePart>>> itr = _drawablesOrganizedByEffect.GetEnumerator();

                while (itr.MoveNext())
                {
                    itr.Current.Value.Clear();
                }
            }

            public void Draw(GraphicsDevice graphicsDevice,
                             BaseEffectConfig effectConfig,
                             bool doShadowMap)
            {
                IEnumerator<KeyValuePair<int, List<Drawable.DrawablePart>>> itr = 
                                            _drawablesOrganizedByEffect.GetEnumerator();

                while (itr.MoveNext())
                {
                    List<Drawable.DrawablePart> drawableParts = itr.Current.Value;
                    if(drawableParts.Count > 0)
                    {
                        if (doShadowMap)
                            drawableParts[0].Effect.EnableShadowMapTechnique();
                        else
                            drawableParts[0].Effect.EnableDefaultTechnique();

                        //drawableParts[0].Effect.View = effectConfig.ViewMat;
                        //drawableParts[0].Effect.Projection = effectConfig.ProjMat;
                        //drawableParts[0].Effect.EyePosition = effectConfig.EyePosition;
                        
                        //drawableParts[0].Effect.Configure(effectConfig);

                        drawableParts[0].Effect.Begin();
                        drawableParts[0].Effect.CurrentTechnique.Passes[0].Begin();
                    }

                    Drawable curDrawable = null;
                    Drawable.DrawablePart curPart = null;
                    for (int i = 0; i < drawableParts.Count; ++i)
                    {
                        Drawable.DrawablePart part = drawableParts[i];
                        if (curPart != part)
                        {
                            if (curPart == null ||
                                curPart.VertexDeclaration != part.VertexDeclaration ||
                                curPart.Drawable.VertexBuffer != part.Drawable.VertexBuffer ||
                                curPart.StreamOffset != part.StreamOffset ||
                                curPart.VertexStride != part.VertexStride)
                            {
                                graphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                graphicsDevice.Vertices[0].SetSource(part.Drawable.VertexBuffer,
                                                                     part.StreamOffset,
                                                                     part.VertexStride);
                                curPart = part;
                            }
                        }

                        if (curDrawable != part.Drawable)
                        {
                            if (curDrawable == null ||
                                curDrawable.IndexBuffer != part.Drawable.IndexBuffer)
                            {
                                graphicsDevice.Indices = part.Drawable.IndexBuffer;
                            }

                            curDrawable = part.Drawable;
                            curDrawable.GfxComponent.ConfigureEffect(part.Effect, 
                                                                     curDrawable.WorldTransform, 
                                                                     part.ComponentData);
                            part.Effect.CommitChanges();

                        }

                        

                        if (curDrawable.IndexBuffer != null)
                            graphicsDevice.DrawIndexedPrimitives(part.PrimitiveType,
                                                                 part.BaseVertex,
                                                                 part.MinVertexIndex,
                                                                 part.NumVertices,
                                                                 part.StartIndex,
                                                                 part.PrimitiveCount);
                        else
                            graphicsDevice.DrawPrimitives(part.PrimitiveType, part.BaseVertex, part.PrimitiveCount);
                    }

                    if (drawableParts.Count > 0)
                    {
                        drawableParts[0].Effect.CurrentTechnique.Passes[0].End();
                        drawableParts[0].Effect.End();
                    }
                }
            }
        }

        RenderBin _renderBin = new RenderBin();

        core.Game _game;

        #region Engine Members
        public Engine()
        {
            _effectConfig = new BaseEffectConfig();
            _filter = new GameObjectTypeFilter(_gfxCompFilter);
        }

        public CameraComponent MainCamera
        {
            get { return _mainCamera; }
            set { _mainCamera = value; }
        }

        public void Configure(String configFilePath)
        {
            _gfxAssetManager = new GfxAssetManager();
            core.Game.instance().Services.AddService(typeof(GraphicsAssetManager), _gfxAssetManager);

            XmlTextReader reader = new XmlTextReader(configFilePath);

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "PreloadModel":
                        {
                            string path = reader.GetAttribute("path");
                            _gfxAssetManager.LoadModelList.Add(path);
                        }
                        break;
                    case "PreloadTexture":
                        {
                            string path = reader.GetAttribute("path");
                            _gfxAssetManager.LoadTextureList.Add(path);
                        }
                        break;
                    case "PreloadFont":
                        {
                            string path = reader.GetAttribute("path");
                            _gfxAssetManager.LoadFontList.Add(path);
                        }
                        break;
                }
            }
        }

        public void CreateGameObjectComponents(DeskWars.core.GameObjectConfig config, 
                                               DeskWars.core.GameObject gameObject)
        {
            for (int i = 0; i < config.GetComponentConfigCount(); ++i)
            {
                core.ComponentConfig compCfg = config.GetComponentConfig(i);
                CreateGameObjectComponent(compCfg, gameObject);
            }
        }

        public void CreateGameObjectComponent(ComponentConfig compCfg, 
                                              GameObject gameObject)
        {
            core.Component comp = null;
            switch (compCfg.Name)
            {
                case "BillboardComponent":
                    {
                        ps.Billboard billboard = ps.ParticleSystemManager.instance().CreateBillboard(_game.GraphicsDevice,
                                                                                                     _game.Content,
                                                                                                     compCfg);
                        ps.BillboardComponent bbComp = new DeskWars.gfx.ps.BillboardComponent(billboard);
                        comp = bbComp;
                    }
                    break;
                case "PointSpriteParticleSystem":
                    {
                        ps.PointSpriteParticleSystem pointSpritePS = 
                            ps.ParticleSystemManager.instance().CreatePointSpriteParticleSystem(_game.GraphicsDevice,
                                                                                                _game.Content,
                                                                                                compCfg);

                        ps.PointSpriteParticleSystemComponent psComp = new ps.PointSpriteParticleSystemComponent(pointSpritePS);
                        string burst = compCfg.GetParameterValue("EmitBursts");
                        if (burst != null && (burst.ToUpper() == "YES" || burst.ToUpper() == "TRUE"))
                        {
                            string burstCount = compCfg.GetParameterValue("BurstCount");
                            if (burstCount != null)
                            {
                                psComp.BurstCount = int.Parse(burstCount);
                            }
                            else
                            {
                                string burstTime = compCfg.GetParameterValue("BurstTime");
                                if (burstTime != null)
                                    psComp.BurstTime = float.Parse(burstTime);

                                psComp.BurstTime = 1.0f;
                            }
                            string repeatBursts = compCfg.GetParameterValue("RepeatBurstsTime");
                            if (repeatBursts != null)
                            {
                                psComp.BurstRepeatTime = float.Parse(repeatBursts);
                            }
                        }
                        string emitterOrientMode = compCfg.GetParameterValue("EmitterOrientMode");
                        if (emitterOrientMode != null)
                            psComp.SetEmitterOrientMode(emitterOrientMode);

                        psComp.EffectName = compCfg.GetParameterValue("EffectName");

                        comp = psComp;
                    }
                    break;
                case "ModelComponent":
                    {
                        gfx.ModelComponent modelComp = new gfx.ModelComponent();
                        //TODO this should be preloaded
                        modelComp.Model = _gfxAssetManager.GetOrCreateModel(compCfg.GetParameterValue("Model"));
                        string scale = compCfg.GetParameterValue("Scale");
                        if (scale != null)
                            modelComp.ApplyScale(float.Parse(scale));

                        string computeBBox = compCfg.GetParameterValue("ComputeBoundingBox");
                        if (computeBBox != null && 
                            (computeBBox.ToUpper() == "NO" || computeBBox.ToUpper() == "FALSE"))
                        {
                            modelComp.SetComputeBoundingBox(false);
                        }

                        modelComp.InitializeDrawables();
                        comp = modelComp;
                    }
                    break;
                case "AnimatedModelComponent":
                    {
                        Model model = _gfxAssetManager.GetOrCreateModel(compCfg.GetParameterValue("Model"));
                        AnimatedModelComponent modelAnimator = new AnimatedModelComponent(model);
                        string scale = compCfg.GetParameterValue("Scale");
                        if (scale != null)
                            modelAnimator.ApplyScale(float.Parse(scale));

                        modelAnimator.InitializeDrawables();
                        comp = modelAnimator;
                    }
                    break;
                case "PlaneComponent":
                    {
                         gfx.PlaneComponent plane = new gfx.PlaneComponent(_graphicsDevice,
                                                      Vector3.Zero, Vector3.UnitY,
                                                      Vector3.UnitZ,
                                                      5000.0f,
                                                      5000.0f);
                        ShadowMapEffect effect = GetOrCreateEffect(compCfg);
                        plane.DefaultEffect = effect;
                        plane.InitializeDrawables();
                        comp = plane;
                    }
                    break;
                case "BoxComponent":
                    {
                        string width = compCfg.GetParameterValue("Width");
                        string height = compCfg.GetParameterValue("Height");
                        string length = compCfg.GetParameterValue("Length");
                        gfx.BoxComponent box = new gfx.BoxComponent(_graphicsDevice,
                                                     Vector3.Zero,
                                                     float.Parse(width),
                                                     float.Parse(height),
                                                     float.Parse(length));

                        ShadowMapEffect effect = GetOrCreateEffect(compCfg);
                        box.DefaultEffect = effect;
                        box.InitializeDrawables();
                        comp = box;
                    }
                    break;
                case "PerspectiveCamera":
                    {
                        comp = _mainCamera = new CameraComponent(MathHelper.ToRadians(45.0f),
                                         _graphicsDevice.Viewport.AspectRatio,
                                         10.0f,
                                         10000.0f);
                        if (_game.Services.GetService(typeof(core.PickService)) != null)
                            _game.Services.RemoveService(typeof(core.PickService));

                        gfx.PickService pickService = new gfx.PickService(_graphicsDevice.Viewport, _mainCamera);
                        _game.Services.AddService(typeof(core.PickService), pickService);
                    }
                    break;
                case "LookLines":
                    {
                        gfx.LookLines gfxComp = new gfx.LookLines(_graphicsDevice);
                        ShadowMapEffect effect = GetOrCreateEffect(compCfg);
                        gfxComp.DefaultEffect = effect;
                        gfxComp.InitializeDrawables();
                        comp = gfxComp;
                    }
                    break;
                case "GridLines":
                    {
                        gfx.GridLines gfxComp = new gfx.GridLines(_graphicsDevice);
                        ShadowMapEffect effect = GetOrCreateEffect(compCfg);
                        gfxComp.DefaultEffect = effect;
                        gfxComp.InitializeDrawables();
                        comp = gfxComp;
                    }
                    break;
                case "BoundingBoxLines":
                    {
                        gfx.BoundingBoxLines gfxComp = new gfx.BoundingBoxLines(_graphicsDevice);
                        ShadowMapEffect effect = GetOrCreateEffect(compCfg);
                        gfxComp.DefaultEffect = effect;
                        gfxComp.InitializeDrawables();
                        comp = gfxComp;
                    }
                    break;
                case "FramesPerSecondText":
                    {
                        comp = new gfx.FramesPerSecondText(_gfxAssetManager.GetOrCreateSpriteFont("Fonts\\Kootenay"),
                                                           Color.White);
                    }
                    break;
                case "LightComponent":
                    {
                        float fov = float.Parse(compCfg.GetParameterValue("FOV"));
                        float nearPlane = float.Parse(compCfg.GetParameterValue("NearPlane"));
                        float farPlane = float.Parse(compCfg.GetParameterValue("FarPlane"));
                        int[] shadowMapWidthHeight = compCfg.GetIntArrayParameterValue("ShadowMapWidthHeight");
                        gfx.LightComponent lightComp = new gfx.LightComponent(_graphicsDevice,
                                                                              fov, nearPlane, farPlane,
                                                                              shadowMapWidthHeight[0], 
                                                                              shadowMapWidthHeight[1]);
                        _effectConfig.Lights.Add(lightComp);
                        comp = lightComp;
                    }
                    break;
                case "ShadowCaster":
                    {
                        comp = new gfx.ShadowCaster();
                    }
                    break;
                case "Text2DComponent":
                    {
                        comp = new gfx.Text2DComponent(_gfxAssetManager.GetOrCreateSpriteFont("Fonts\\Kootenay"), 
                                                       Color.White, "");
                    }
                    break;
                case "DebugService":
                    {
                        comp = new gfx.DebugService(_gfxAssetManager.GetOrCreateSpriteFont("Fonts\\Kootenay"), 
                                                    0.01f, 0.05f);
                    }
                    break;
            }

            if (comp != null)
            {
                gameObject.AddComponent(comp);
            }
        } 

        protected ShadowMapEffect GetOrCreateEffect(core.ComponentConfig compCfg)
        {
            float[] diffuseColorArray = compCfg.GetFloatArrayParameterValue("DiffuseColor");
            Vector3 diffuseColor = Vector3.One;
            if (diffuseColorArray != null)
            {
                diffuseColor = new Vector3(diffuseColorArray[0],
                                           diffuseColorArray[1],
                                           diffuseColorArray[2]);
            }

            string alphaVal = compCfg.GetParameterValue("Alpha");
            float alpha = 1.0f;
            if (alphaVal != null)
            {
                alpha = float.Parse(alphaVal);
            }

            return _gfxAssetManager.GetOrCreateShadowMapEffect(diffuseColor, alpha);
        }

        public void LoadContent(core.Game game)
        {
            _game = game;
            _graphicsDevice = _game.GraphicsDevice;
            _gfxAssetManager.SetContentManager(game.Content);

            string effectFile = "Effects\\ShadowMapEffect";
            if (_graphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion.Major < 3)
            {
                effectFile += "20";
            }
            string envShader = System.Environment.GetEnvironmentVariable("DeskWarsShader");
            if (envShader != null)
                effectFile = envShader;

            _gfxAssetManager.LoadModels(effectFile);
            _gfxAssetManager.LoadSpriteFonts();
            _gfxAssetManager.LoadTextures();

            /*core.GameObjectFactory factory = 
                (core.GameObjectFactory)game.Services.GetService(typeof(core.GameObjectFactory));

            core.GameObject gameObj = factory.CreateGameObject("EmptyObject", "ShadowMapTexture");
            
            _shadowMapTextureWidget = new DeskWars.gui.TextureWidget("ShadowMapTexture", 
                                                                     0.25f, 0.25f, 
                                                                     1.0f, 
                                                                     lightShapeTexture, 
                                                                     Color.White);

            _shadowMapTextureWidget.SetOffsetTranslation(0.75f, 0.0f);
            gameObj.AddComponent(_shadowMapTextureWidget);*/

        }

        public void Update(DeskWars.core.GameObjectDB gameObjectDB, Microsoft.Xna.Framework.GameTime gameTime)
        {
            ps.ParticleSystemManager.instance().Update(gameTime);
        }

        #endregion

        #region Renderer Members

        public void Draw(DeskWars.core.GameObjectDB gameObjectDB, GameTime gameTime)
        {
            _gfxCompFilter.Reset();
            gameObjectDB.GetGameObjectsByType(_filter, _gfxCompFilter.GameObjects);
            //BoundingFrustum frustum = _mainCamera.Frustum;
            //gameObjectDB.GetGameObjectsIntersectFrustum(frustum, _filter, gameObjects);

            _effectConfig.EyePosition = _mainCamera.Position;
            foreach (gfx.ComponentTypeFilter.GameObjectGfxCompPair pair in _gfxCompFilter.GfxComponents)
            {
                pair.GfxComponent.PreDraw(pair.GameObject.WorldTransform, gameTime);
            }

            GenerateShadowMaps(_gfxCompFilter.GfxComponents, gameTime);
            //if(_effectConfig.Lights.Count > 0)
                //_shadowMapTextureWidget.Texture = _effectConfig.Lights[0].ShadowMap;

            _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            _effectConfig.ViewMat = _mainCamera.ViewMat;
            _effectConfig.ProjMat = _mainCamera.ProjMat;

            DrawGameObjects(_gfxCompFilter.GfxComponents,
                            _effectConfig,
                            false);

            ps.ParticleSystemManager mgr = ps.ParticleSystemManager.instance();
            mgr.Renderer().SetWorldViewProjectionMatricesForAllParticleSystems(Matrix.Identity, 
                                                                               _mainCamera.ViewMat, 
                                                                               _mainCamera.ProjMat);
            mgr.Renderer().DrawAllParticleSystems();
        }

        protected void DrawGameObjects(List<gfx.ComponentTypeFilter.GameObjectGfxCompPair> gameObjects,
                                       BaseEffectConfig effectConfig,
                                       bool doShadowMap)
        {
            _renderBin.Reset();

            foreach (gfx.ComponentTypeFilter.GameObjectGfxCompPair pair in gameObjects)
            {
                if(!doShadowMap || pair.GameObject.IsType(typeof(gfx.ShadowCaster)))
                {
                    pair.GfxComponent.AddDrawablesToRenderBin(_renderBin);
                }
            }

            //this will configure the shared effect params
            _gfxAssetManager.BaseShadowMapEffect.Configure(effectConfig);
            _renderBin.Draw(_graphicsDevice, effectConfig, doShadowMap);
        }

        protected void GenerateShadowMaps(List<gfx.ComponentTypeFilter.GameObjectGfxCompPair> gameObjGfxComps, 
                                       GameTime gameTime)
        {
            RenderTarget2D origTarget = (RenderTarget2D)_graphicsDevice.GetRenderTarget(0);
            DepthStencilBuffer origDepthBuf = _graphicsDevice.DepthStencilBuffer;

            foreach (LightComponent lightComp in _effectConfig.Lights)
            {
                _graphicsDevice.SetRenderTarget(0, lightComp.ShadowRenderTarget);
                _graphicsDevice.DepthStencilBuffer = lightComp.ShadowDepthBuffer;
    
                _graphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Red, 1.0f, 0);

                _effectConfig.ViewMat = lightComp.View;
                _effectConfig.ProjMat = lightComp.Projection;

                DrawGameObjects(gameObjGfxComps, _effectConfig, true);

                _graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

                _graphicsDevice.SetRenderTarget(0, origTarget);
                _graphicsDevice.DepthStencilBuffer = origDepthBuf;

                lightComp.ShadowMap = lightComp.ShadowRenderTarget.GetTexture();
            }
        }

        public void OnGameObjectDeleted(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Engine Members

        public void OnLoadLevel(int level)
        {
        }

        public void OnUnloadLevel(int level)
        {
        }

        #endregion
    }
}
