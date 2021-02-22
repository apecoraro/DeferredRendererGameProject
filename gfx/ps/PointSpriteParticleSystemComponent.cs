using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx.ps
{
    class PointSpriteParticleSystemComponent : core.Component, DeskWars.com.MessageHandler
    {
        PointSpriteParticleSystem _particleSystem;
        public PointSpriteParticleSystemComponent(PointSpriteParticleSystem ps)
        {
            _particleSystem = ps;
            _isAutoEmitter = _particleSystem.Emitter.EmitParticlesAutomatically;
            _particleSystem.Emitter.BurstComplete += OnBurstComplete;
        }

        public string EffectName = "NoNameSpecified";
        //two ways to do a burst - either burst a certain amount
        public int BurstCount = 0;
        //or burst for a certain amount of time
        public float BurstTime = 0.0f;

        //if burst repeat is set to 0.0f then only burst on command
        float _burstRepeatTime = 0.0f;
        float _timeSinceLastBurst = 0.0f;
        public float BurstRepeatTime
        {
            get { return _burstRepeatTime; }
            set
            {
                _burstRepeatTime = value;
                //do an initial burst then delay
                _timeSinceLastBurst = _burstRepeatTime + 1.0f;
            }
        }

        bool _doBurst = false;//set to true to cause a non repeat burst
        bool _burstComplete = true;
        bool _isAutoEmitter = false;

        public enum ParticleEmitterOrientMode
        {
            LOOK_VECTOR,
            OPPOSITE_LOOK_VECTOR,
            NONE
        };

        ParticleEmitterOrientMode _emitterOrientMode = ParticleEmitterOrientMode.LOOK_VECTOR;
        public ParticleEmitterOrientMode EmitterOrientMode
        {
            get { return _emitterOrientMode; }
            set
            {
                _emitterOrientMode = value;
                switch(_emitterOrientMode)
                {
                case ParticleEmitterOrientMode.LOOK_VECTOR:
                    {
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = true;
                    }
                    break;
                case ParticleEmitterOrientMode.OPPOSITE_LOOK_VECTOR:
                    {
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = true;
                    }
                    break;
                case ParticleEmitterOrientMode.NONE:
                    {
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = false;
                    }
                    break;
                }
            }
        }

        public void SetEmitterOrientMode(string mode)
        {
            switch (mode.ToUpper())
            {
                case "LOOK_VECTOR":
                    {
                        EmitterOrientMode = ParticleEmitterOrientMode.LOOK_VECTOR;
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = true;
                    }
                    break;
                case "OPPOSITE_LOOK_VECTOR":
                    {
                        EmitterOrientMode = ParticleEmitterOrientMode.OPPOSITE_LOOK_VECTOR;
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = true;
                    }
                    break;
                case "NONE":
                    {
                        EmitterOrientMode = ParticleEmitterOrientMode.NONE;
                        _particleSystem.InitialProperties.VelocityIsAffectedByEmittersOrientation = false;
                    }
                    break;
            }
        }

        public void OnBurstComplete(object sender, EventArgs e)
        {
            _burstComplete = true;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(gfx.com.MsgEffect), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            ps.ParticleSystemManager.instance().RemoveParticleSystem(_particleSystem);
            gameObject.PostOffice.UnRegisterForMessages(typeof(gfx.com.MsgEffect), this);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            if (gameObject.IsActive)
            {
                _particleSystem.Emitter.PositionData.Position = gameObject.Position;
                switch (EmitterOrientMode)
                {
                    case ParticleEmitterOrientMode.LOOK_VECTOR:
                        {
                            _particleSystem.Emitter.OrientationData.Up = gameObject.LookVector;
                        }
                        break;
                    case ParticleEmitterOrientMode.OPPOSITE_LOOK_VECTOR:
                        {
                            _particleSystem.Emitter.OrientationData.Up = -gameObject.LookVector;
                        }
                        break;
                }

                if (!_particleSystem.Emitter.EmitParticlesAutomatically &&
                     _particleSystem.Emitter.Enabled &&
                    (BurstCount > 0 || BurstTime > 0.0f))
                {
                    UpdateBurst(gameTime);
                }
            }
        }

        void UpdateBurst(GameTime gameTime)
        {
            _timeSinceLastBurst += (float)gameTime.ElapsedRealTime.TotalSeconds;
            //only do a burst if not in the middle of a burst
            if (_burstComplete &&
                //and we are in single burst mode (repeat == 0) and doBurst is true
                ((BurstRepeatTime == 0.0f && _doBurst) ||
                //or we are in repeat burst mode and enough time has passed
                (BurstRepeatTime > 0.0f && _timeSinceLastBurst > BurstRepeatTime)))
            {
                _doBurst = false;
                if (BurstCount > 0)
                {
                    _burstComplete = false;
                    _timeSinceLastBurst = 0.0f;
                    _particleSystem.Emitter.BurstParticles = BurstCount;
                }
                else //if(BurstTime > 0.0f) //this function only called if BurstCount > 0 or BurstTime > 0.0f
                {
                    _burstComplete = false;
                    _timeSinceLastBurst = 0.0f;
                    _particleSystem.Emitter.BurstTime = BurstTime;
                }
            }
        }

        public bool IsOneTimeBurstEffect()
        {
            return BurstRepeatTime == 0.0f && BurstCount > 0;
        }

        public void OnEffectMessage(gfx.com.MsgEffect msg)
        {
            if (msg.EffectName == this.EffectName)
            {
                _particleSystem.Emitter.Enabled = msg.Enable;
                if (msg.Enable)
                {
                    if (IsOneTimeBurstEffect())
                    {
                        _doBurst = true;
                    }
                    else
                        _particleSystem.Emitter.EmitParticlesAutomatically = _isAutoEmitter;
                }
                else
                {
                    _particleSystem.Emitter.EmitParticlesAutomatically = false;
                }

            }
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            gfx.com.MsgEffect msgEffect = (gfx.com.MsgEffect)msg;
            OnEffectMessage(msgEffect);
        }

        #endregion

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            ps.ParticleSystemManager.instance().RemoveParticleSystem(_particleSystem);
            _particleSystem.Emitter.EmitParticlesAutomatically = false;
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            ps.ParticleSystemManager.instance().AddParticleSystem(_particleSystem);
            _particleSystem.Emitter.EmitParticlesAutomatically = _isAutoEmitter;
            
        }
    }
}
