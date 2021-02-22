using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using DeskWars.core;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    class CommanderComponent : gameplay.UnitComponent, com.MessageHandler
    {
        com.MsgCommanderAttacked _msgCommanderAttacked = null;
        sound.com.MsgSoundEffectRequest _msgSoundReq = new sound.com.MsgSoundEffectRequest();
        core.GameObject _smoke = null;

        float _lastPlayedSound = 5.0f;
        bool _attacked = false;

        public CommanderComponent(Ratings ratings, Lua lua, LuaFunction _lf) : 
            base(ratings, lua, _lf, "noweapon")
        {
            _msgCommanderAttacked = new com.MsgCommanderAttacked(this);
            _msgSoundReq.Request = new DeskWars.sound.SoundComponent.Request();
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgAttack), this);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgAttack), this);
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void Update(GameObject gameObject, GameTime gameTime)
        {
            _lastPlayedSound += (float)gameTime.ElapsedGameTime.TotalSeconds;
                
            if (_attacked)
            {
                gfx.com.MsgChangeState csMsg = new gfx.com.MsgChangeState();
                gfx.ComponentState state = new gfx.ComponentState();
                state.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                state.Alpha = 0.5f;
                csMsg.NewState = state;
                csMsg.TimeInSeconds = 0.25f;//stay in this state for half a second
                gameObject.PostOffice.Send(csMsg);
                _attacked = false;

                if (_lastPlayedSound > 15.0f)
                {
                    _msgSoundReq.Request.SoundName = "CommanderAttacked";
                    _msgSoundReq.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                    gameObject.PostOffice.Send(_msgSoundReq);
                    _lastPlayedSound = 0.0f;
                }
                if (this.UnitStats.health < 75)
                {
                    if (_smoke == null)
                    {
                        core.GameObjectFactory factory = 
                            (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
                        _smoke = factory.CreateGameObject("Smoke", "CommanderSmoke");
                        _smoke.Position = gameObject.BoundingSphere.Center + (Vector3.Up * gameObject.BoundingSphere.Radius * 0.5f);
                        _smoke.LookVector = Vector3.Up;
                    }
                }
            }

            base.Update(gameObject, gameTime);
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            com.MsgAttack attackedMsg = msg as com.MsgAttack;
            core.Game.instance().PostOffice.Send(_msgCommanderAttacked);

            if ((((com.MsgAttack)msg).AttackPower) - this.UnitRatings.DefensePower > 1)
                this.UnitStats.health -= ((com.MsgAttack)msg).AttackPower - this.UnitRatings.DefensePower;
            else
                this.UnitStats.health -= 1;
            if (this.UnitStats.health < 0)
            {
//                this.UnitStats.health = 0;
                core.Game.instance().DeleteGameObject(this.UnitRatings.Name);
            }

            _attacked = true;
        }

        #endregion
    }
}
