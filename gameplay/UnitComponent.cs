/*!------------------------------------------------------------------
 * Copyright (c) 2009 Digipen (USA) Corporation, All rights reserved
 * 
 * File Name : UnitComponent.cs
 * Purpose : Unit Gameplay Logic System
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
using System.IO;

namespace DeskWars.gameplay
{
    public class UnitComponent : core.Component
    {
        class UnitMessageHandler : com.MessageHandler
        {
            public UnitComponent _unit = null;
            com.MsgUnitSelected _msgUnitSelected = null;

            public UnitMessageHandler(UnitComponent unit)
            {
                _unit = unit;
                _msgUnitSelected = new com.MsgUnitSelected();
                _msgUnitSelected.Unit = unit;
            }

            #region MessageHandler Members

            void com.MessageHandler.receive(DeskWars.com.Message msg)
            {
                Type type = msg.GetType();
                if (type == typeof(com.MsgMove))
                {
                    _unit.UnitStats.msgList[(int)Msg.Move] = true;
                    ///hack
                    if (_unit.UnitStats.state != State.State_Dead)
                    {
                        _unit.UnitStats.MoveVector = (msg as com.MsgMove).MoveVector;
                        //if(_unit.UnitStats.MoveVector == Vector3.Zero)
                            //_unit.UnitStats.msgList[(int)Msg.Move] = false;
                    }
                }
                else if(type == typeof(com.MsgMoveTo))
                {
                    _unit.UnitStats.msgList[(int)Msg.Moveto] = true;
                    com.MsgMoveTo message = (com.MsgMoveTo)msg;
                        if (message.GoTo != _unit.Goal)// && message.GoTo != -Vector3.One)
                        {
                            _unit.Goal = message.GoTo;
                        }    
                }
                else if (type == typeof(com.MsgLook))
                {
                    //doubtful
                    //_unit.UnitStats.LookVector = (msg as com.MsgLook).LookVector;
                }
                else if (type == typeof(com.MsgChase))
                {
                    
                    com.MsgChase message = (com.MsgChase)msg;
                    if (_unit.Goal != message.GameObject.Position)
                    {
                        if (message.AttackObject != null)
                        {                       
                            _unit.Goal = message.AttackObject.Position;
                            _unit._attackObject = message.AttackObject;
                            _unit.UnitStats.msgList[(int)Msg.Chase] = true;
                        }
                    }
                }
                else if (type == typeof(com.MsgObjectSelected))
                {
                    com.MsgObjectSelected selMsg = (com.MsgObjectSelected)msg;
                    if (selMsg.SelectedObject == null)
                        _msgUnitSelected.Unit = null;
                    else
                        _msgUnitSelected.Unit = _unit;

                    core.Game.instance().PostOffice.Send(_msgUnitSelected);
                }
                else if (type == typeof(com.MsgAttack))
                {
                    _unit.UnitStats.msgList[(int)Msg.Attack] = true;
                    _unit._attackObject = ((com.MsgAttack)msg).Attacker;
                    _unit.hitPoint = ((com.MsgAttack)msg).AttackPower;
                }
            }

            #endregion
        }

        #region MemberVariables

        UnitMessageHandler _messageHandler = null;

        Stats _statsVars = null;
        Ratings _ratings;
        core.GameObject _attackObject = null;
        Lua script = null;
        State prevState;
        ObjectTime time = new ObjectTime();
        double _initTime;
        Vector3 Goal = -Vector3.One;
        Vector3 prevGoal;
        Vector3 prevPos;
        int hitPoint;
        LuaFunction lf;
        
        #endregion

        #region MemberClasses

        public enum State
        {
            State_Idle,
            State_Patrol,
            State_Attack,
            State_Hit,
            State_Chase,
            State_Flee,
            State_Dead
        }

        public enum Msg
        {
            Attack,
            Chase,
            Move,
            Moveto //ALWAYS KEEP THIS IN THE END
        }

        public class ObjectTime
        {
            public double currTime;
            public double prevTime = 0.0f;
            public double diffTime
            {
                get
                {
                    if (prevTime > currTime)
                        prevTime = currTime;//0;
                    //if (currTime < 0.5)
                        //currTime = 0.7;
                    return currTime - prevTime;
                }
            }
        }
        public class Stats
        {
            public Stats()
            {
                for (int i = 0; i < msgList.GetUpperBound(0); ++i)
                    msgList[i] = false;
            }
            public State state = State.State_Idle;
            public bool[] msgList = new bool[(int)Msg.Moveto + 1];
            public int health = 100;
            public int stamina = 100;
            public float lastAttackTime = -1.0f;
            //Shifted from Member variables
            Vector3 _moveVector = Vector3.Zero;
            Vector3 _lookVector = Vector3.Zero;
            public string _weapon;
            string _class;

            public Vector3 MoveVector
            {
                get { return _moveVector; }
                set { _moveVector = value; }
            }

            public Vector3 LookVector
            {
                get { return _lookVector; }
                set { _lookVector = value; }
            }

            public string Class
            {
                get { return _class; }
                set { _class = value; }
            }

        }

        public class Ratings
        {
            int _attackPower;
            //public int AttackPower { get { return _attackPower; } }
            int _defensePower;
            public int DefensePower { get { return _defensePower; } }
            float _reloadTimeInSecs;
            public float ReloadTimeInSecs { get { return _reloadTimeInSecs; } }
            float _attackRange;
            public float AttackRange { get { return _attackRange; } }
            float _viewRange;
            public float ViewRange { get { return _viewRange; } }
            protected float _topSpeed;
            public float TopSpeed { get { return _topSpeed; } }
            public string Name;
            public string Team;
            public Ratings(){}
            public Ratings(int attackPwr, 
                        int defensePwr, 
                        float reloadTimeInSecs, 
                        float attackRange, 
                        float viewRange,
                        float topSpeed,
                        string team)
            {
                _attackPower = attackPwr;
                _defensePower = defensePwr;
                _reloadTimeInSecs = reloadTimeInSecs;
                _attackRange = attackRange;
                _viewRange = viewRange;
                _topSpeed = topSpeed;
                Team = team;
            }
        }
        #endregion

        #region MemberProperties

        public Stats UnitStats
        {
            get { return _statsVars; }
        }

        public Ratings UnitRatings
        {
            get { return _ratings; }
            set { _ratings = value; } //should not be able to change ratings
        }

        public core.GameObject OpponentObject
        {
            get { return _attackObject; }
        }

        #endregion

        public UnitComponent(Ratings ratings,Lua _lua,LuaFunction _lf,string weapon)
        {
            _ratings = ratings;
            
            _statsVars = new Stats();
            prevState = UnitStats.state;
            UnitStats._weapon = weapon;
            _messageHandler = new UnitMessageHandler(this);
            script = _lua;
            lf = _lf;
        }

        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            //throw new NotImplementedException();
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgMove), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgMoveTo), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgLook), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgChase), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgObjectSelected), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgCollision), _messageHandler);
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgAttack), _messageHandler);
            UnitRatings.Name = gameObject.Name;
            UnitStats.Class = gameObject.Class;

        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgMove), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgMoveTo), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgLook), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgChase), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgObjectSelected), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgCollision), _messageHandler);
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgAttack), _messageHandler);
            //lf.Dispose();
            lf = null;
        }

        public override void Update(core.GameObject gameObject, GameTime gameTime)
        {

            //gameObject.Position += (_moveVector * this._ratings.TopSpeed);

            if (_initTime == 0)
                _initTime = gameObject.delay;

            time.currTime = gameTime.TotalGameTime.TotalSeconds;

                //Lua Script
            if (script != null && lf!=null)
            {
                initializeScript(gameObject);
                lf.Call();
                saveScript();
            }
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {

        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            if (gameObject.Class != "Commander")
            {
                _statsVars = new Stats();
                prevState = UnitStats.state;
                //_initTime = 0.0;
            }
        }

        #region MemberFunctions

        public void initializeScript(core.GameObject UnitObject)
        {
            script["unit"] = UnitObject;
            script["enemy"] = _attackObject;
            script["InitTime"] = Math.Abs(_initTime);
            script["prevState"] = prevState.ToString();
            script["currState"] = UnitStats.state.ToString();
            script["goal"] = Goal;
            script["prevgoal"] = prevGoal;
            script["stats"] = UnitStats;
            script["ratings"] = UnitRatings;
            script["position"] = prevPos;
            script["hits"] = hitPoint;
            script["Time"] = time;

        }

        public void saveScript()
        {
            prevState = (State)Enum.Parse(typeof(State), (string)script["prevState"], true);
            _statsVars.state = (State)Enum.Parse(typeof(State), (string)script["currState"], true);
            prevGoal = (Vector3)script["prevgoal"];
            prevPos = (Vector3)script["position"];
            _attackObject = (core.GameObject)script["enemy"];
            _initTime = (double)script["InitTime"];
        }

        #endregion

    }

}
