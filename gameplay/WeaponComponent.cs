/*!------------------------------------------------------------------
 * Copyright (c) 2009 Digipen (USA) Corporation, All rights reserved
 * 
 * File Name : WeaponComponent.cs
 * Purpose : Weapon Logic System
 * Project : Desk Wars
 * Author : Ejaz Ahmed, ejaz.mudassir@digpen.edu
 * Creation date : 10/25/2009
 * ------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LuaInterface;

namespace DeskWars.gameplay
{
    public class WeaponComponent : physics.DynamicComponent
    {
        class WeaponMessageHandler : com.MessageHandler
        {
            public WeaponComponent _unit = null;

            public WeaponMessageHandler(WeaponComponent unit)
            {
                _unit = unit;

            }

            #region MessageHandler Members

            void com.MessageHandler.receive(DeskWars.com.Message msg)
            {
                Type type = msg.GetType();
                if (type == typeof(com.MsgMove))
                {
                    _unit._statsVars.MoveVector = (msg as com.MsgMove).MoveVector;

                }
                else if (type == typeof(com.MsgTarget))
                {
                    com.MsgTarget msgTarget = msg as com.MsgTarget;
                    _unit._attackObject = msgTarget.AttackObject;
                    _unit._unitObject = msgTarget.UnitObject;
                    //aim a litte in front
                    float dist = (msgTarget.AttackObject.Position - msgTarget.UnitObject.Position).Length();
                    Vector3 newgoal = msgTarget.AttackObject.Position + 
                                    ((msgTarget.AttackObject.Position - msgTarget.AttackObject.PrevPosition) * (dist*0.1f));

                    //_unit._statsVars._goal = goal;   // msgTarget.AttackObject.Position;
                    _unit._statsVars._start = msgTarget.UnitObject.Position;
                    Vector3 vec = newgoal - _unit._statsVars._start;
                    vec.Normalize();
                    vec.X *= (msgTarget.AttackObject.BoundingSphere.Radius);
                    vec.Z *= (msgTarget.AttackObject.BoundingSphere.Radius);
                    _unit._statsVars._goal = newgoal - (vec);
                    _unit._statsVars._goal.Y = msgTarget.AttackObject.Position.Y;
                }
            }

            #endregion
        }

        #region MemberVariables
        gfx.com.MsgEffect _explosionEffectMsg = new gfx.com.MsgEffect("Explosion");
        gfx.com.MsgEffect _weaponEffectMsg = new gfx.com.MsgEffect("Weapon");
        WeaponMessageHandler _messageHandler;
        core.GameObject _attackObject;
        core.GameObject _unitObject;
        Lua script;
        LuaFunction lf;
        Stats _statsVars = null;
        Ratings _ratings;
        float _timeAttackHasBeenFinished = -1.0f;
        ObjectTime time;// = new ObjectTime();
        sound.com.MsgSoundEffectRequest _msgSoundReq = new sound.com.MsgSoundEffectRequest();

        public class ObjectTime
        {
            public double currTime;
            public double prevTime = 0.0f;
            public double diffTime
            {
                get
                {
                    if (prevTime > currTime)
                        prevTime = 0;
                    return currTime - prevTime;
                }
            }
        }

        public class Ratings : UnitComponent.Ratings
        {
            public Ratings(string weapon, int attackpower, float topspeed)
            {
                base.Name = weapon;
                _attackPower = attackpower;
                base._topSpeed = topspeed;
            }
            public int _attackPower;
            public int AttackPower { get { return _attackPower; } }
            public float TopSpeed { get { return base.TopSpeed; } }
            public string _weapon;
            public string Name { get { return base.Name; } }
        }

        public class Stats : UnitComponent.Stats
        {
            //public double distv = 0;
            //public double dotp = 0;
            public Vector3 _goal = Vector3.Zero;
            public Vector3 _start = Vector3.Zero;
            public Vector3 _posn = Vector3.Zero;
            public int Hit = 0;
            public Vector3 Trail
            {
                get
                {
                    return _goal - _start;
                }
            }

            public double Distance
            {
                get
                {
                    _posn.Y = Trail.Y;
                    return (_goal - _posn).Length();
                }
            }

            public double DotProduct
            {
                get
                {
                    Vector3 vec = _goal - _posn;
                    vec.Y = Trail.Y;
                    return Vector3.Dot(Trail, vec);
                }
            }

        }

        #endregion

        #region MemberProperties

        #endregion

        public WeaponComponent(int attackPower, Lua _lua, LuaFunction _lf, string weapon)
        {
            
            if(weapon=="InkThrower")
                _ratings = new Ratings(weapon, attackPower, 0.2f);
            else
                _ratings = new Ratings(weapon, attackPower, 1.0f);
            _statsVars = new Stats();
            time = new ObjectTime();
            script = _lua;
            lf = _lf;
            _messageHandler = new WeaponMessageHandler(this);

            _msgSoundReq.Request = new DeskWars.sound.SoundComponent.Request();
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgMove), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgTarget), _messageHandler);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgMove), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgTarget), _messageHandler);
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //reset state vars
            time.prevTime = 0f;
            _timeAttackHasBeenFinished = -1.0f;
            _statsVars = new Stats();
            _attackObject = null;
            _unitObject = null;
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
            //throw new NotImplementedException();
            time.currTime = gameTime.TotalGameTime.TotalSeconds;
            //if (_timeAttackHasBeenFinished > 0.0f)
            //{
                //_timeAttackHasBeenFinished += (float)gameTime.ElapsedGameTime.TotalSeconds;
                //if (_timeAttackHasBeenFinished > 5.0f)
                    //deleteMe(gameObject);
            //}
            //else
            {
                    //updatePosition(gameObject, _statsVars, _ratings);
                    if (lf != null)
                    {
                        initializeScript(gameObject);
                        lf.Call();
                        saveScript();
                    }
                    //if (_statsVars.DotProduct <= 0)
                    //{
                        //_timeAttackHasBeenFinished = 0.01f;
                        //attack(_unitObject, _attackObject, _ratings);
                        //explode(gameObject);
                    //}
            }

            base.Update(gameObject, gameTime);
        }

        public void initializeScript(core.GameObject WeaponObject)
        {
            script["weapon"] = WeaponObject;
            script["unit"] = _unitObject; 
            script["enemy"] = _attackObject;
            script["stats"] = _statsVars;
            script["ratings"] = _ratings;
            script["Time"] = time;
            script["TimeLeft"] = _timeAttackHasBeenFinished;
        }

        public void saveScript()
        {
            script["Time"] = time;
            _timeAttackHasBeenFinished = (float)(double)script["TimeLeft"];
        }

        public void deleteMe(core.GameObject WeaponObject)
        {
            core.Game.instance().DeleteGameObject(WeaponObject);  //Self Destrucitng of game object
        }

        //public void updatePosition(core.GameObject WeaponObject, Stats stats, Ratings ratings)
        //{
        //    stats._posn = WeaponObject.Position;
        //    stats.MoveVector = stats.Trail;
        //    if (stats.Distance > stats.Trail.Length() / 2)
        //        stats.MoveVector.Y = stats.Trail.Length() / 3;
        //    else
        //        stats.MoveVector.Y = -stats.Trail.Length() / 3;
        //    stats.MoveVector.Normalize();

        //    if (stats.MoveVector != Vector3.Zero)
        //    {
        //        WeaponObject.LookVector = stats.MoveVector;
        //    }
        //    else if (stats.LookVector != Vector3.Zero)
        //    {
        //        WeaponObject.LookVector = stats.LookVector;
        //        stats.LookVector = Vector3.Zero;
        //    }

        //    com.MsgPhysics message = new com.MsgPhysics();

        //    message.Velocity = stats.MoveVector * ratings.TopSpeed;
        //    WeaponObject.Send(message);
        //}

    }
}
