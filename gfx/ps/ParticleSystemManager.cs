using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DeskWars.core;

namespace DeskWars.gfx.ps
{
    class ParticleSystemManager
    {
        static ParticleSystemManager _instance = new ParticleSystemManager();
        public static ParticleSystemManager instance()
        {
            return _instance;
        }

        DPSF.ParticleSystemManager _psMgr;
        private ParticleSystemManager()
        {
            _psMgr = new DPSF.ParticleSystemManager();
        }

        public void Update(GameTime gameTime)
        {
            _psMgr.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void AddParticleSystem(IDPSFParticleSystem ps)
        {
            _psMgr.AddParticleSystem(ps);
        }

        public void RemoveParticleSystem(IDPSFParticleSystem ps)
        {
            _psMgr.RemoveParticleSystem(ps);
        }

        public DPSF.ParticleSystemManager Renderer()
        {
            return _psMgr;
        }

        public ps.Billboard CreateBillboard(GraphicsDevice device,
                                            ContentManager content,
                                            ComponentConfig compCfg)
        {
            ps.Billboard bb = new ps.Billboard(device, content);

            AddParticleSystem(bb);

            return bb;
        }

        public ps.PointSpriteParticleSystem CreatePointSpriteParticleSystem(GraphicsDevice device,
                                                                     ContentManager content,
                                                                     ComponentConfig compCfg)
        {
            string texture = compCfg.GetParameterValue("Texture");
            Texture2D particleTexture = content.Load<Texture2D>(texture);

            int maxNumParticles = int.Parse(compCfg.GetParameterValue("MaxParticles"));

            ps.PointSpriteParticleSystem pointSpritePS = new ps.PointSpriteParticleSystem(device,
                                                                                          content,
                                                                                          particleTexture,
                                                                                          maxNumParticles);

            int emitNumParticlesPerSecond = int.Parse(compCfg.GetParameterValue("ParticlesPerSecond"));
            string burst = compCfg.GetParameterValue("EmitBursts");
            bool emitParticlesAutomatically = true;
            if (burst != null && (burst.ToUpper() == "YES" || burst.ToUpper() == "TRUE"))
                emitParticlesAutomatically = false;

            pointSpritePS.Emitter.EmitParticlesAutomatically = emitParticlesAutomatically;
            pointSpritePS.Emitter.ParticlesPerSecond = emitNumParticlesPerSecond;

            float[] particleStartSize = compCfg.GetFloatArrayParameterValue("ParticleStartSize");
            if (particleStartSize != null)
            {
                float[] particleEndSize = compCfg.GetFloatArrayParameterValue("ParticleEndSize");

                bool interpMinMaxSize = false;
                if (particleStartSize.Length > 2 && particleEndSize.Length > 2)
                {
                    string interp = compCfg.GetParameterValue("InterpolateMinMaxColors");
                    if (interp != null && (interp.ToUpper() == "YES" || interp.ToUpper() == "TRUE"))
                        interpMinMaxSize = true;
                }

                pointSpritePS.InitSize(particleStartSize, particleEndSize, interpMinMaxSize);
            }
            else
                pointSpritePS.InitSize(1.0f);


            Color[] startColors = compCfg.GetColorArrayParameterValue("ParticleStartColor");
            if (startColors != null)
            {
                Color[] endColors = compCfg.GetColorArrayParameterValue("ParticleEndColor");

                bool interpMinMaxColors = false;
                if (startColors.Length > 2 && endColors.Length > 2)
                {
                    string interp = compCfg.GetParameterValue("InterpolateMinMaxColors");
                    if (interp != null && (interp.ToUpper() == "YES" || interp.ToUpper() == "TRUE"))
                        interpMinMaxColors = true;
                }
                pointSpritePS.InitColor(startColors, endColors, interpMinMaxColors);
            }
            else
                pointSpritePS.InitColor(Color.White);

            float[] particleLifetime = compCfg.GetFloatArrayParameterValue("ParticleLifetime");
            pointSpritePS.InitLifetime(particleLifetime);

            Vector3[] velocity = compCfg.GetVector3ArrayParameterValue("ParticleVelocity");
            if (velocity != null)
                pointSpritePS.InitVelocity(velocity);

            Vector3[] acceleration = compCfg.GetVector3ArrayParameterValue("ParticleAcceleration");
            if (acceleration != null)
                pointSpritePS.InitAcceleration(acceleration);

            Vector3[] extForce = compCfg.GetVector3ArrayParameterValue("ParticleExternalForce");
            if (extForce != null)
                pointSpritePS.InitExternalForce(extForce);

            float[] friction = compCfg.GetFloatArrayParameterValue("ParticleFriction");
            if (friction != null)
                pointSpritePS.InitFriction(friction);

            float[] rotationalVelocity = compCfg.GetFloatArrayParameterValue("ParticleRotationalVelocity");
            if (rotationalVelocity != null)
                pointSpritePS.InitRotationalVelocity(rotationalVelocity);

            float[] rotationalAcceleration = compCfg.GetFloatArrayParameterValue("ParticleRotationalAcceleration");
            if (rotationalAcceleration != null)
                pointSpritePS.InitRotationalAcceleration(rotationalAcceleration);

            string fadeMode = compCfg.GetParameterValue("ParticleFadeMode");
            if (fadeMode != null)
                pointSpritePS.InitFadeMode(fadeMode);

            string enabled = compCfg.GetParameterValue("Enabled");
            if (enabled != null && (enabled.ToUpper() == "NO" || enabled.ToUpper() == "FALSE"))
                pointSpritePS.Emitter.Enabled = false;

            AddParticleSystem(pointSpritePS);

            return pointSpritePS;
        }
    }
}
