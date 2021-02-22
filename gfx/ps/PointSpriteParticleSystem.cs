using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#region File Description
//===================================================================
// PointSpriteParticleSystem.cs
//===================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DPSF;
#endregion

namespace DeskWars.gfx.ps
{
    //-----------------------------------------------------------
    // TODO: Rename/Refactor the Particle System class
    //-----------------------------------------------------------
    /// <summary>
    /// Create a new Particle System class that inherits from a
    /// Default DPSF Particle System
    /// </summary>
    public class PointSpriteParticleSystem : DPSF.DefaultPointSpriteParticleSystem
    {
        public enum FadeMode
        {
            FADE_OUT,
            FADE_IN,
            FADE_IN_QUICK_OUT_SLOW
        };

        int _positionUpdateGroup = 1;
        int _updateTransparencyGroup = 2;
        int _rotationUpdateGroup = 3;

        int _positionUpdateOrder = 0;
        int _rotationUpdateOrder = 10;
        int _frictionUpdateOrder = 20;
        int _fadeUpdateOrder = 30;

        bool _updatePositionByAcceleration = false;
        bool _updateRotationByRotationalAcceleration = false;

        /// <summ
        /// Constructor
        /// </summary>
        /// <param name="cGame">Handle to the Game object being used. Pass in null for this 
        /// parameter if not using a Game object.</param>
        /// <param name="cGraphicsDevice">The Graphics Device to draw to</param>
        /// <param name="cContentManager">The Content Manager to use to load Textures and Effect files</param>
        
        public PointSpriteParticleSystem(GraphicsDevice graphicsDevice, 
                                         ContentManager contentManager,
                                         Texture2D texture, 
                                         int maxNumParticles) : base(null)
        {
            // Initialize the Particle System before doing anything else
            InitializePointSpriteParticleSystem(graphicsDevice, contentManager, maxNumParticles, maxNumParticles, 
                                                UpdateVertexProperties, texture);

            // Setup the Emitter
            Emitter.PositionData.Position = Vector3.Zero;
            Emitter.OrientationData.Up = Vector3.Up;
            
            Reset();
        }

        /// <summary>
        /// Reset the Particle System Events and any other settings
        /// </summary>
        public void Reset()
        {
            ParticleInitializationFunction = InitializeParticleUsingInitialProperties;
            
            // Remove all Events first so that none are added twice if this function is called again
            ParticleEvents.RemoveAllEvents();
            ParticleSystemEvents.RemoveAllEvents();
        }

        public void UpdateParticleColor(Color smin, Color smax, Color emin, Color emax)
        {
            InitialProperties.StartColorMin = smin;
            InitialProperties.StartColorMax = smax;

            InitialProperties.EndColorMin = emin;
            InitialProperties.EndColorMax = emax;
        }

        public void InitColor(Color startColor)
        {
            UpdateParticleColor(startColor, startColor, startColor, startColor);
        }
        
        public void InitColor(Color[] start, Color[] end, bool interpMinMax)
        {
            int numElems = start.Length + end.Length;

            switch(numElems)
            {
                case 1:
                    {
                        InitColor(start[0]);
                    }
                    break;
                case 2:
                    {
                        InitColor(start[0], end[0]);
                    }
                    break;
                case 4:
                    {
                        InitColor(start[0], start[1], end[0], end[1], interpMinMax);
                    }
                    break;
            }
        }

        public void InitColor(Color startColor, Color endColor)
        {
            UpdateParticleColor(startColor, startColor, endColor, endColor);

            ParticleEvents.AddEveryTimeEvent(UpdateParticleColorUsingLerp);
        }

        public void InitColor(Color smin, Color smax, Color emin, Color emax, bool interpMinMaxColors)
        {
            UpdateParticleColor(smin, smax, emin, emax);
            InitialProperties.InterpolateBetweenMinAndMaxColors = interpMinMaxColors;

            ParticleEvents.AddEveryTimeEvent(UpdateParticleColorUsingLerp);
        }

        public void UpdateParticleSize(float smin, float smax, float emin, float emax)
        {
            InitialProperties.StartSizeMin = smin;
            InitialProperties.StartSizeMax = smax;

            InitialProperties.EndSizeMin = emin;
            InitialProperties.EndSizeMax = emax;
        }

        public void InitSize(float startSize)
        {
            UpdateParticleSize(startSize, startSize, startSize, startSize);
        }
        
        public void InitSize(float[] start, float[] end, bool interpMinMax)
        {
            int numElems = start.Length + end.Length;

            switch(numElems)
            {
                case 1:
                    {
                        InitSize(start[0]);
                    }
                    break;
                case 2:
                    {
                        InitSize(start[0], end[0]);
                    }
                    break;
                case 4:
                    {
                        InitSize(start[0], start[1], end[0], end[1], interpMinMax);
                    }
                    break;
            }
        }

        public void InitSize(float startSize, float endSize)
        {
            UpdateParticleSize(startSize, startSize, endSize, endSize);

            ParticleEvents.AddEveryTimeEvent(UpdateParticleSizeUsingLerp);
        }

        public void InitSize(float smin, float smax, float emin, float emax, bool interpMinMaxColors)
        {
            UpdateParticleSize(smin, smax, emin, emax);
            InitialProperties.InterpolateBetweenMinAndMaxColors = interpMinMaxColors;

            ParticleEvents.AddEveryTimeEvent(UpdateParticleSizeUsingLerp);
        }

        public void UpdateParticleLifetime(float min, float max)
        {
            InitialProperties.LifetimeMin = min;
            InitialProperties.LifetimeMax = max;
        }
        
        public void InitLifetime(float[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitLifetime(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitLifetime(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitLifetime(float startLifetime)
        {
            UpdateParticleLifetime(startLifetime, startLifetime);
        }

        public void InitLifetime(float minLifetime, float maxLifetime)
        {
            UpdateParticleLifetime(minLifetime, maxLifetime);
        }

        public void UpdateParticleVelocity(Vector3 min, Vector3 max)
        {
            InitialProperties.VelocityMin = min;
            InitialProperties.VelocityMax = max;
        }
        
        public void InitVelocity(Vector3[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitVelocity(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitVelocity(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitVelocity(Vector3 velocity)
        {
            UpdateParticleVelocity(velocity, velocity);
            if (!_updatePositionByAcceleration)
            {
                ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionUsingVelocity, 
                                                 _positionUpdateOrder, 
                                                 _positionUpdateGroup);
            }
        }

        public void InitVelocity(Vector3 minVelocity, Vector3 maxVelocity)
        {
            UpdateParticleVelocity(minVelocity, maxVelocity);
            if (!_updatePositionByAcceleration)
            {
                ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionUsingVelocity, 
                                                 _positionUpdateOrder, 
                                                 _positionUpdateGroup);
            }
        }

        public void UpdateParticleAcceleration(Vector3 min, Vector3 max)
        {
            InitialProperties.AccelerationMin = min;
            InitialProperties.AccelerationMax = max;
        }
        
        public void InitAcceleration(Vector3[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitAcceleration(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitAcceleration(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitAcceleration(Vector3 minAcceleration)
        {
            UpdateParticleAcceleration(minAcceleration, minAcceleration);

            ParticleEvents.RemoveAllEventsInGroup(_positionUpdateGroup);
            ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionAndVelocityUsingAcceleration, 
                                             _positionUpdateOrder, 
                                             _positionUpdateGroup);
            _updatePositionByAcceleration = true;
        }

        public void InitAcceleration(Vector3 minAcceleration, Vector3 maxAcceleration)
        {
            UpdateParticleAcceleration(minAcceleration, maxAcceleration);

            ParticleEvents.RemoveAllEventsInGroup(_positionUpdateGroup);
            ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionAndVelocityUsingAcceleration, 
                                             _positionUpdateOrder, 
                                             _positionUpdateGroup);
            _updatePositionByAcceleration = true;
        }

        public void UpdateParticleExternalForce(Vector3 min, Vector3 max)
        {
            InitialProperties.ExternalForceMin = min;
            InitialProperties.ExternalForceMax = max;
        }
        
        public void InitExternalForce(Vector3[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitExternalForce(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitExternalForce(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitExternalForce(Vector3 externalForce)
        {
            UpdateParticleExternalForce(externalForce, externalForce);
            
            ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionUsingExternalForce);
        }

        public void InitExternalForce(Vector3 minExternalForce, Vector3 maxExternalForce)
        {
            UpdateParticleExternalForce(minExternalForce, maxExternalForce);

            ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionUsingExternalForce);
        }

        public void UpdateParticleFriction(float min, float max)
        {
            InitialProperties.FrictionMin = min;
            InitialProperties.FrictionMax = max;
        }
        
        public void InitFriction(float[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitFriction(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitFriction(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitFriction(float friction)
        {
            UpdateParticleFriction(friction, friction);

            ParticleEvents.AddEveryTimeEvent(UpdateParticleVelocityUsingFriction, 
                                             _frictionUpdateOrder);
        }

        public void InitFriction(float minFriction, float maxFriction)
        {
            UpdateParticleFriction(minFriction, maxFriction);

            ParticleEvents.AddEveryTimeEvent(UpdateParticleVelocityUsingFriction, 
                                             _frictionUpdateOrder);
        }

        public void UpdateParticleRotationalVelocity(float min, float max)
        {
            InitialProperties.RotationalVelocityMin = min;
            InitialProperties.RotationalVelocityMax = max;
        }

        public void InitRotationalVelocity(float[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitRotationalVelocity(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitRotationalVelocity(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitRotationalVelocity(float rotationalVelocity)
        {
            UpdateParticleRotationalVelocity(rotationalVelocity, rotationalVelocity);
            if (!_updateRotationByRotationalAcceleration)
            {
                ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationUsingRotationalVelocity, 
                                                 _rotationUpdateOrder, 
                                                 _rotationUpdateGroup);
            }
        }

        public void InitRotationalVelocity(float minRotationalVelocity, float maxRotationalVelocity)
        {
            UpdateParticleRotationalVelocity(minRotationalVelocity, maxRotationalVelocity);
            if (!_updateRotationByRotationalAcceleration)
            {
                ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationUsingRotationalVelocity, 
                                                 _rotationUpdateOrder, 
                                                 _rotationUpdateGroup);
            }
        }

        public void UpdateParticleRotationalAcceleration(float min, float max)
        {
            InitialProperties.RotationalAccelerationMin = min;
            InitialProperties.RotationalAccelerationMax = max;
        }

        public void InitRotationalAcceleration(float[] array)
        {
            switch(array.Length)
            {
                case 1:
                    {
                        InitRotationalAcceleration(array[0]);
                    }
                    break;
                case 2:
                    {
                        InitRotationalAcceleration(array[0], array[1]);
                    }
                    break;
            }
        }

        public void InitRotationalAcceleration(float startRotationalAcceleration)
        {
            UpdateParticleRotationalAcceleration(startRotationalAcceleration, startRotationalAcceleration);

            ParticleEvents.RemoveAllEventsInGroup(_rotationUpdateGroup);
            ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration, 
                                             _rotationUpdateOrder, 
                                             _rotationUpdateGroup);
            _updateRotationByRotationalAcceleration = true;
        }

        public void InitRotationalAcceleration(float minRotationalAcceleration, float maxRotationalAcceleration)
        {
            UpdateParticleRotationalAcceleration(minRotationalAcceleration, maxRotationalAcceleration);

            ParticleEvents.RemoveAllEventsInGroup(_rotationUpdateGroup);
            ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration, 
                                             _rotationUpdateOrder, 
                                             _rotationUpdateGroup);
            _updateRotationByRotationalAcceleration = true;
        }

        public void InitFadeMode(string mode)
        {
            switch (mode.ToUpper())
            {
                case "FADE_IN":
                    {
                        InitFadeMode(FadeMode.FADE_IN);
                    }
                    break;
                case "FADE_OUT":
                    {
                        InitFadeMode(FadeMode.FADE_OUT);
                    }
                    break;
                case "FADE_IN_QUICK_OUT_SLOW":
                    {
                        InitFadeMode(FadeMode.FADE_IN_QUICK_OUT_SLOW);
                    }
                    break;
            }
        }

        public void InitFadeMode(FadeMode mode)
        {
            switch (mode)
            {
                case FadeMode.FADE_IN:
                    {
                        ParticleEvents.AddEveryTimeEvent(UpdateParticleTransparencyToFadeInUsingLerp, 
                                                         _fadeUpdateOrder, 
                                                         _updateTransparencyGroup);
                    }
                    break;
                case FadeMode.FADE_OUT:
                    {
                        ParticleEvents.AddEveryTimeEvent(UpdateParticleTransparencyToFadeOutUsingLerp, 
                                                         _fadeUpdateOrder, 
                                                         _updateTransparencyGroup);
                    }
                    break;
                case FadeMode.FADE_IN_QUICK_OUT_SLOW:
                    {
                        ParticleEvents.AddEveryTimeEvent(UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 
                                                         _fadeUpdateOrder, 
                                                         _updateTransparencyGroup);
                    }
                    break;
            }
        }
    }
}
