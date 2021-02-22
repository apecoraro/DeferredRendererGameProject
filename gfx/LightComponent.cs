using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class LightComponent : core.Component
    {
        Vector3 _lightPos;
        Vector3 _lightLookAt;
        public Vector3 Position { get { return _lightPos; } }
        public Vector3 LookAt { get { return _lightLookAt; } }

        Vector3 _color = Vector3.One;
        public Vector3 Color { get { return _color; } set { _color = value; } }

        Matrix _view;
        public Matrix View { get { return _view; } set { _view = value; } }

        Matrix _projection;
        public Matrix Projection { get { return _projection; } set { _projection = value; } }

        Texture2D _shadowMap;
        public Texture2D ShadowMap { get { return _shadowMap; } set { _shadowMap = value; } }

        RenderTarget2D _shadowRenderTarget;
        public RenderTarget2D ShadowRenderTarget { get { return _shadowRenderTarget; } set { _shadowRenderTarget = value; } }

        DepthStencilBuffer _shadowDepthBuffer;
        public DepthStencilBuffer ShadowDepthBuffer { get { return _shadowDepthBuffer; } set { _shadowDepthBuffer = value; } }

        public LightComponent(GraphicsDevice graphicsDevice, 
                              float fovDegrees, 
                              float nearPlane, 
                              float farPlane,
                              int shadowMapWidth,
                              int shadowMapHeight)
        {
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fovDegrees),
                                        graphicsDevice.Viewport.AspectRatio,
                                        nearPlane,
                                        farPlane);
            //Matrix.CreateOrthographic(640.0f, 480.0f, 50.0f, 1000.0f);

            SurfaceFormat shadowMapFormat = graphicsDevice.DisplayMode.Format;

            // Check to see if the device supports a 32 or 16 bit 
            // floating point render target
            /*if (GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(DeviceType.Hardware,
                               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format,
                               TextureUsage.Linear, QueryUsages.None,
                               ResourceType.RenderTarget, SurfaceFormat.Single) == true)
            {
                //for some reason the Single format only works sporadically for me
                shadowMapFormat = SurfaceFormat.Single;
            }
            else*/
            if (GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(
                        DeviceType.Hardware,
                        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format,
                        TextureUsage.Linear, QueryUsages.None,
                        ResourceType.RenderTarget, SurfaceFormat.HalfSingle) == true)
            {
                shadowMapFormat = SurfaceFormat.HalfSingle;
            }

            PresentationParameters pp = graphicsDevice.PresentationParameters;
            // Create new floating point render target
            this.ShadowRenderTarget = new RenderTarget2D(graphicsDevice,
                                                    shadowMapWidth,
                                                    shadowMapHeight,
                                                    1, shadowMapFormat);

            // Create depth buffer to use when rendering to the shadow map
            this.ShadowDepthBuffer = new DepthStencilBuffer(graphicsDevice,
                                                       shadowMapWidth,
                                                       shadowMapHeight,
                                                       DepthFormat.Depth24);
        }
        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            if (gameObject.Position != _lightPos || gameObject.LookAt != _lightLookAt)
            {
                _lightPos = gameObject.Position;
                _lightLookAt = gameObject.LookAt;
                _view = Matrix.CreateLookAt(_lightPos,
                                            _lightLookAt, Vector3.Up);
                                            //gameObject.UpVector);
            }
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }
    }
}
