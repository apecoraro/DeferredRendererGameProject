/*!------------------------------------------------------------------
 * Copyright (c) 2009 Digipen (USA) Corporation, All rights reserved
 * 
 * File Name : StateMachine.cs
 * Purpose : State Machine System
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
    public class StateMachine
    {
        gfx.com.MsgEffect _explosionEffectMsg = new gfx.com.MsgEffect("Explosion");
        gfx.com.MsgEffect _weaponEffectMsg = new gfx.com.MsgEffect("Weapon");
        sound.com.MsgSoundEffectRequest _msgSoundReq = new sound.com.MsgSoundEffectRequest();

        public class FriendTeamFilter : core.ComponentTypeFilter
        {
            #region ComponentTypeFilter Members

            public bool Accept(DeskWars.core.GameObject gameObject,
                               DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                {
                    if (unit.UnitRatings.Team != "Enemy")
                        return true;
                }

                return false;
            }

            #endregion
        }

        public StateMachine(Lua _lua)
        {
            _msgSoundReq.Request = new DeskWars.sound.SoundComponent.Request();
            _lua.RegisterFunction("OnMsg", this, this.GetType().GetMethod("onMsg"));
            _lua.RegisterFunction("Moving", this, this.GetType().GetMethod("moving"));
            _lua.RegisterFunction("UpdateObject", this, this.GetType().GetMethod("updatePosition"));
            _lua.RegisterFunction("PrintState", this, this.GetType().GetMethod("printstate"));
            _lua.RegisterFunction("Chase", this, this.GetType().GetMethod("chase"));
            _lua.RegisterFunction("Move", this, this.GetType().GetMethod("move"));
            _lua.RegisterFunction("Animate", this, this.GetType().GetMethod("animate"));
            _lua.RegisterFunction("Halt", this, this.GetType().GetMethod("halt"));
            _lua.RegisterFunction("GetClosestCharacter", this, this.GetType().GetMethod("getClosestObject"));
            _lua.RegisterFunction("GetTower", this, this.GetType().GetMethod("getTowerObject"));
            _lua.RegisterFunction("GoalChanged", this, this.GetType().GetMethod("goalChanged"));
            _lua.RegisterFunction("GetDistance", this, this.GetType().GetMethod("getDistance"));
            _lua.RegisterFunction("AttackEnemy", this, this.GetType().GetMethod("targetAttack"));
            _lua.RegisterFunction("Fire", this, this.GetType().GetMethod("attack"));
            _lua.RegisterFunction("Explode", this, this.GetType().GetMethod("explode"));
            _lua.RegisterFunction("GotHit", this, this.GetType().GetMethod("getHitPoints"));
            _lua.RegisterFunction("Dead", this, this.GetType().GetMethod("die"));
            _lua.RegisterFunction("Vector3", this, this.GetType().GetMethod("createVec"));
            _lua.RegisterFunction("Normalize", this, this.GetType().GetMethod("normalize"));
            _lua.RegisterFunction("VecSum", this, this.GetType().GetMethod("sum"));
            _lua.RegisterFunction("VecMult", this, this.GetType().GetMethod("multiply"));
            _lua.RegisterFunction("Look", this, this.GetType().GetMethod("look"));

        }

        #region MemberFunctions

        public bool onMsg(string message, UnitComponent.Stats stats)
        {
            bool temp = stats.msgList[((int)Enum.Parse(typeof(UnitComponent.Msg), message, true))];
            stats.msgList[((int)Enum.Parse(typeof(UnitComponent.Msg), message, true))] = false;
            return temp;
        }

        public bool moving(UnitComponent.Stats stats)
        {
            if (stats.MoveVector == Vector3.Zero)
                return false;
            else 
                return true;
        }

        public void animate(core.GameObject UnitObject,
                            string state,
                            float speedFactor, bool loop)
        {
            gfx.com.MsgAnimate _animMsg = new gfx.com.MsgAnimate();
            _animMsg.Animation = state;
            _animMsg.SpeedFactor = speedFactor;
            _animMsg.Loop = loop;
            UnitObject.Send(_animMsg);
        }

        public void getHitPoints(core.GameObject UnitObject)
        {
            gfx.com.MsgChangeState msg = new gfx.com.MsgChangeState();
            gfx.ComponentState state = new gfx.ComponentState();
            state.DiffuseColor = Vector3.One;
            state.Alpha = 0.5f;
            msg.NewState = state;
            msg.TimeInSeconds = 0.25f;//stay in this state for half a second
            UnitObject.Send(msg);
        }

        public core.GameObject getClosestObject(core.GameObject UnitObject,UnitComponent.Ratings ratings)
        {
            core.ComponentTypeFilter compFilter;
            core.Game game = core.Game.instance();
            if (ratings.Team != "Enemy")
            {
                compFilter = new Engine.EnemyTeamFilter();
            }
            else
            {
                compFilter = new FriendTeamFilter();
            }
            core.GameObjectTypeFilter filter =
                new core.GameObjectTypeFilter(compFilter);
            return game.GameObjectDB.GetClosestGameObject(UnitObject, filter);
        }

        public core.GameObject getTowerObject()
        {
            core.Game game = core.Game.instance();
            return game.GameObjectDB.GetGameObject("Commander Good");
        }

        public double getDistance(core.GameObject UnitObject, Vector3 Position)
        {
            if (UnitObject != null)
                return (Position - UnitObject.Position).Length();
            else
                return Position.Length();
        }

        public bool goalChanged(core.GameObject UnitObject, Vector3 PrevPosition, Vector3 NewPosition)
        {
            Vector3 v1 = PrevPosition - UnitObject.Position;
            Vector3 v2 = NewPosition - UnitObject.Position;

            v1.Normalize();
            v2.Normalize();
            if ((Math.Acos(Vector3.Dot(v1, v2))) / (Math.PI / 4) >= 1)
                return true;
            else
                return false;
        }

        public void updatePosition(core.GameObject UnitObject, UnitComponent.Stats stats, UnitComponent.Ratings ratings)
        {
            //stats.MoveVector.Normalize();
            com.MsgPhysics message = new com.MsgPhysics();
            
            message.Velocity = stats.MoveVector * ratings.TopSpeed;
            UnitObject.Send(message);
            if (stats.MoveVector != Vector3.Zero)
            {
                UnitObject.LookVector = stats.MoveVector;
            }
            else if (stats.LookVector != Vector3.Zero)
            { 
                UnitObject.LookVector = stats.LookVector;
                //stats.LookVector = Vector3.Zero;
            }

        }

        //Dont know why this is used
        public bool isAlive(core.GameObject Object)
        {
            if (Object.IsActive == false)
                return false;
            return true;
        }

        public void move(core.GameObject UnitObject, Vector3 Position)
        {
            com.MsgFindPath msg = new com.MsgFindPath();
            msg.Goal = Position;
            msg.Max = Position;
            msg.Max.Y = 0;
            msg.Min = Position;
            msg.Min.Y = 0;
            msg.stop = false;
            UnitObject.Send(msg);
        }

        public void chase(core.GameObject UnitObject, core.GameObject ChaseObject)
        {
            com.MsgFindPath msg = new com.MsgFindPath();
            msg.Goal = ChaseObject.Position;
            msg.Min = ChaseObject.BoundingBox.Min;
            msg.Min.Y = -10;
            msg.Max = ChaseObject.BoundingBox.Max;
            msg.Max.Y = -10;
            msg.stop = false;
            UnitObject.Send(msg);
        }

        public void targetAttack(core.GameObject UnitObject, core.GameObject OpponentObject, UnitComponent.Stats stats)
        {
            //changing the way the unit looks
            Vector3 lookVec = OpponentObject.Position - UnitObject.Position;
            lookVec.Y = 0.0f;
            stats.LookVector = lookVec;
            stats.LookVector.Normalize();
            UnitObject.LookVector = stats.LookVector;
            //------------------------------------
            core.Game game = core.Game.instance();
            core.GameObject weapon = game.CreateGameObject(stats._weapon, stats._weapon);
            Vector3 pos = UnitObject.Position + UnitObject.LookVector * UnitObject.BoundingSphere.Radius * 2;
            pos.Y = 20;
            weapon.Position = pos;
            com.MsgTarget msg = new com.MsgTarget();
            msg.AttackObject = OpponentObject;
            msg.UnitObject = UnitObject;
            weapon.Send(msg);

            //com.MsgLook look = new com.MsgLook();
            //look.LookVector = OpponentObject.Position - UnitObject.Position;
            //look.LookVector.Y = 0.0f;
            //UnitObject.PostOffice.Send(look);
        }

        public void look(core.GameObject UnitObject, core.GameObject OpponentObject, UnitComponent.Stats stats)
        {
            //changing the way the unit looks
            Vector3 lookVec = OpponentObject.Position - UnitObject.Position;
            lookVec.Y = 0.0f;
            stats.LookVector = lookVec;
            stats.LookVector.Normalize();
            UnitObject.LookVector = stats.LookVector;
            //------------------------------------
        }

        public void halt(core.GameObject UnitObject)
        {
            com.MsgFindPath msg = new com.MsgFindPath();
            msg.Goal = -Vector3.One;
            msg.Max = Vector3.Zero;
            msg.Min = Vector3.Zero;
            msg.stop = true;
            UnitObject.Send(msg);
        }

        public void die(core.GameObject UnitObject)
        {
            //UnitObject.IsAlive = false;
            core.Game.instance().DeleteGameObject(UnitObject);
        }

        public void printstate(string _currState)
        {
            core.Game game = core.Game.instance();
            core.DebugService debugSvc = (core.DebugService)game.Services.GetService(typeof(core.DebugService));
            debugSvc.AddDebugText("State: " + _currState, 5.0f);
        }

        #endregion

        public Vector3 createVec(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        public Vector3 sum(Vector3 vec1, Vector3 vec2)
        {
            Vector3 v = vec1 + vec2;
            return v; 
        }

        public Vector3 multiply(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z);
        }

        public Vector3 normalize(Vector3 vec)
        {
            Vector3 v = vec;
            v.Normalize();
            return v;
        }

        public void attack(core.GameObject UnitObject, core.GameObject OpponentObject, WeaponComponent.Ratings ratings)
        {
            if (OpponentObject != null && OpponentObject.IsActive)
            {
                com.MsgAttack msg = new com.MsgAttack();
                msg.Attacker = UnitObject;
                msg.AttackPower = ratings._attackPower;
                OpponentObject.Send(msg);
            }
        }

        public void explode(core.GameObject WeaponObject)
        {
            //gfx.com.MsgEffect _explosionEffectMsg = new gfx.com.MsgEffect("Explosion");
            //gfx.com.MsgEffect _weaponEffectMsg = new gfx.com.MsgEffect("Weapon");
            //sound.com.MsgSoundEffectRequest _msgSoundReq = new sound.com.MsgSoundEffectRequest();
            
            _explosionEffectMsg.Enable = true;
            _weaponEffectMsg.Enable = false;
            WeaponObject.PostOffice.Send(_explosionEffectMsg);
            WeaponObject.PostOffice.Send(_weaponEffectMsg);
            _msgSoundReq.Request.SoundName = "Explosion";
            _msgSoundReq.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
            WeaponObject.PostOffice.Send(_msgSoundReq);
        }
    }
}
