using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LuaInterface;


namespace DeskWars.gameplay
{
    class Engine : DeskWars.core.Engine, com.MessageHandler
    {
        String m_configFile;
        int m_currentLevel;
        Lua _lua;
        Dictionary<string,LuaFunction> _luafunc = new Dictionary<string,LuaFunction>();
        StateMachine statemachine;

        SpriteFont _font;

        UnitComponent _commanderUnit = null;
        List<core.GameObject> _enemyUnits = new List<core.GameObject>();
        List<GremlinSpawner> _gremlinSpawners = new List<GremlinSpawner>();
        core.GameObject _endGameGUI = null;

        public class EnemyTeamFilter : core.ComponentTypeFilter
        {
            #region ComponentTypeFilter Members

            public bool Accept(DeskWars.core.GameObject gameObject,
                               DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                {
                    if (unit.UnitRatings.Team == "Enemy")
                        return true;
                }

                return false;
            }

            #endregion
        }

        EnemyTeamFilter _enemyTeamFilter = new EnemyTeamFilter();

        core.GameObjectTypeFilter _enemyTeamGameObjFilter = null;

        class UnitComponentFinder : core.ComponentVisitor
        {
            #region ComponentVisitor Members

            public void Apply(DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                    TheUnit = unit;
            }

            public void Reset()
            {
                TheUnit = null;
            }

            public gameplay.UnitComponent TheUnit = null;

            #endregion
        }

        UnitComponentFinder _finder = new UnitComponentFinder();

        class GameObjectInstanceConfig
        {
            public string Class;
            public string ObjectName;
            public string Position;
            public string LookAt;
            public string LookVector;
            public string delay;
            public Dictionary<String, String> ParamValues;
            public void SetParameter(String param, String value)
            {
                if (ParamValues == null)
                    ParamValues = new Dictionary<string, string>();

                ParamValues[param] = value;
            }
        }

        class GameObjectPool
        {
            public string Class;
            public int Count;
        }

        class LevelConfig
        {
            public List<GameObjectInstanceConfig> GameObjectInstanceConfigs;
            public List<GameObjectPool> GameObjectPools;
            public LevelConfig() 
            { 
                GameObjectInstanceConfigs = new List<GameObjectInstanceConfig>();
                GameObjectPools = new List<GameObjectPool>();
            }
        }

        List<LevelConfig> m_levelConfigs;

        public void Configure(String configFile)
        {
            m_configFile = configFile;
        }

        public void CreateGameObjectComponents(DeskWars.core.GameObjectConfig config,
                                               DeskWars.core.GameObject gameObject)
        {
            for(int i = 0; i < config.GetComponentConfigCount(); ++i)
            {
                core.ComponentConfig compCfg = config.GetComponentConfig(i);
                CreateGameObjectComponent(compCfg, gameObject);
            }
        }

        public void CreateGameObjectComponent(DeskWars.core.ComponentConfig compCfg, 
                                              DeskWars.core.GameObject gameObject)
        {
            core.Component comp = CreateComponent(compCfg);
            if (comp != null)
                gameObject.AddComponent(comp);
        }

        public DeskWars.core.Component CreateComponent(DeskWars.core.ComponentConfig config)
        {
            core.Game game = core.Game.instance();
            switch (config.Name)
            {
                case "KeyboardMouseController":
                    {
                        Texture2D defaultMouse = game.Content.Load<Texture2D>("Textures\\DefaultMouse");
                        Texture2D movementMouse = game.Content.Load<Texture2D>("Textures\\MovementMouse");
                        Texture2D attackMouse = game.Content.Load<Texture2D>("Textures\\AttackMouse");
                        Texture2D selectMouse = game.Content.Load<Texture2D>("Textures\\SelectMouse");
                        Texture2D attackMouseFail = game.Content.Load<Texture2D>("Textures\\AttackMouseFail");
                        KeyboardMouseController controller = new KeyboardMouseController(defaultMouse,
                                                                                         movementMouse,
                                                                                         attackMouse,
                                                                                         selectMouse,
                                                                                         attackMouseFail);
                        controller.SetMoveScalars(10.0f,
                                                8.0f,
                                                3.0f);

                        return controller;
                    }
                case "CommanderComponent":
                    {
                        //TODO read this data from the config
                        UnitComponent.Ratings ratings = ConfigureRatings(config);
                        //UnitComponent.Ratings ratings = new UnitComponent.Ratings(10, 10, 1.0f, 1000.0f, 1000.0f, 0.1f);
                        string scriptName = config.GetParameterValue("ScriptName");
                        if(!_luafunc.ContainsKey(scriptName))
                            _luafunc.Add(scriptName, _lua.LoadFile("Content/scripts/" + scriptName + ".lua"));

                        CommanderComponent unit = new CommanderComponent(ratings, _lua, _luafunc[scriptName]);
                        
                        _commanderUnit = unit;

                        return unit;
                    }
                case "UnitComponent":
                    {
                        //TODO read this data from the config
                        UnitComponent.Ratings ratings = ConfigureRatings(config);
                        //UnitComponent.Ratings ratings = new UnitComponent.Ratings(10, 10, 1.0f, 1000.0f, 1000.0f, 0.1f);
                        string scriptName = config.GetParameterValue("ScriptName");
                        if (!_luafunc.ContainsKey(scriptName))
                            _luafunc.Add(scriptName, _lua.LoadFile("Content/scripts/" + scriptName + ".lua"));

                        string weapon = config.GetParameterValue("Weapon");
                        
                        UnitComponent unit = new UnitComponent(ratings, _lua, _luafunc[scriptName], weapon);

                        //if (unit.UnitRatings.Team == "Enemy")
                            //_enemyUnits.Add(unit);

                        return unit;
                    }
                case "GremlinSpawner":
                    {
                        string start = config.GetParameterValue("StartDelay");
                        float startFloat = 0.0f;
                        if (start != null)
                            startFloat = float.Parse(start);

                        string respawn = config.GetParameterValue("RespawnDelay");
                        float respawnFloat = 0.0f;
                        if (respawn != null)
                            respawnFloat = float.Parse(respawn);

                        string total = config.GetParameterValue("MaxGremlinsSpawned");
                        int totalInt = 0;
                        if (total != null)
                            totalInt = int.Parse(total);

                        // Get parameters via config.GetParameterValue("...") and use them to create an instance of the gremlin spawner
                        GremlinSpawner spawner = new GremlinSpawner(startFloat, respawnFloat, totalInt);

                        _gremlinSpawners.Add(spawner);

                        return spawner;

                    }
                case "MouseHoverComponent":
                    {
                        MouseHoverComponent mouseHoverComp = new MouseHoverComponent();

                        return mouseHoverComp;
                    }
                case "HaloComponent":
                    {
                        gameplay.HaloComponent halo = new gameplay.HaloComponent(core.Game.instance().GraphicsDevice,
                                                     1.0f, 50.0f, 30, 5.0f);
                        core.GraphicsAssetManager assetMgr = 
                                (core.GraphicsAssetManager)core.Game.instance().Services.GetService(typeof(core.GraphicsAssetManager));

                        Vector3 diffuseColor = Vector3.One;
                        float alpha = 1.0f;
                        halo.DefaultEffect = assetMgr.GetOrCreateShadowMapEffect(diffuseColor, alpha);
                        halo.InitializeDrawables();
                        return halo;
                    }
                case "DeskComponent":
                    {
                        gameplay.DeskComponent desk = new gameplay.DeskComponent();
                        return desk;
                    }
                case "StartGameClicker":
                    {
                        gameplay.StartGameClicker clicker = new gameplay.StartGameClicker(config);
                        return clicker;
                    }
                case "RepeatLevelClicker":
                    {
                        gameplay.RepeatLevelClicker clicker = new gameplay.RepeatLevelClicker(config);
                        return clicker;
                    }
                case "QuitButtonMessageHandler":
                    {
                        gameplay.QuitButtonMessageHandler handler = new gameplay.QuitButtonMessageHandler(config);
                        return handler;
                    }
                case "MainMenuButtonMessageHandler":
                    {
                        gameplay.MainMenuButtonMessageHandler handler = 
                                                            new gameplay.MainMenuButtonMessageHandler(config);
                        return handler;
                    }
                case "ResumeButtonMessageHandler":
                    {
                        gameplay.ResumeButtonMessageHandler handler =
                                                            new gameplay.ResumeButtonMessageHandler(config);
                        return handler;
                    }
                case "WeaponComponent":
                    {
                        int attackPower = int.Parse(config.GetParameterValue("AttackPower"));
                        string weaponName = config.GetParameterValue("Weapon");
                        string scriptName = config.GetParameterValue("ScriptName");
                        if (!_luafunc.ContainsKey(scriptName))
                            _luafunc.Add(scriptName, _lua.LoadFile("Content/scripts/" + scriptName + ".lua"));
                        gameplay.WeaponComponent weapon = new gameplay.WeaponComponent(attackPower, _lua, _luafunc[scriptName], weaponName);
                        return weapon;
                    }
                case "MiniMapWidget":
                    {
                        string textureFile = config.GetParameterValue("OwnUnitTexture");
                        Texture2D ownText = game.Content.Load<Texture2D>(textureFile);

                        textureFile = config.GetParameterValue("EnemyUnitTexture");
                        Texture2D enemyText = game.Content.Load<Texture2D>(textureFile);
                        
                        gameplay.MiniMapWidget miniMap = new gameplay.MiniMapWidget(ownText, enemyText, config);

                        return miniMap;
                    }
                case "SelectedUnitWidget":
                    {
                        string textureFile = config.GetParameterValue("Texture");
                        Texture2D texture = game.Content.Load<Texture2D>(textureFile);
                        
                        float[] colorArray = config.GetFloatArrayParameterValue("TextColor");
                        Color textColor = new Color(colorArray[0], colorArray[1], colorArray[2]);

                        string[] icons = config.GetArrayParameterValue("Icons");
                        string[] iconNames = config.GetArrayParameterValue("IconNames");
                        Dictionary<string, Texture2D> iconTextures = new Dictionary<string, Texture2D>();
                        LoadIconTextures(game.Content, icons, iconNames, iconTextures);

                        gameplay.SelectedUnitWidget guiComp = new gameplay.SelectedUnitWidget(_font, 
                                                                                              textColor, 
                                                                                              texture,
                                                                                              iconTextures,
                                                                                              config);
                        return guiComp;
                    }
                case "CommanderHealthBar":
                    {
                        string textureFile = config.GetParameterValue("Texture");
                        Texture2D texture = game.Content.Load<Texture2D>(textureFile);

                        gameplay.CommanderHealthBar guiComp = new gameplay.CommanderHealthBar(texture, config);
                        return guiComp;
                    }
                case "StartGameGUI":
                    {
                        string dpLogo = config.GetParameterValue("DigiPenLogo");
                        Texture2D dpLogoTexture = game.Content.Load<Texture2D>(dpLogo);

                        string tgLogo = config.GetParameterValue("TopGamesLogo");
                        Texture2D tgLogoTexture = game.Content.Load<Texture2D>(tgLogo);

                        string dwLogo = config.GetParameterValue("DeskWarsLogo");
                        Texture2D dwLogoTexture = game.Content.Load<Texture2D>(dwLogo);

                        string instructions = config.GetParameterValue("InstructionImage");
                        Texture2D instructionsTexture = game.Content.Load<Texture2D>(instructions);

                        gameplay.StartGameGUI guiComp = new gameplay.StartGameGUI(dpLogoTexture,
                                                                                  tgLogoTexture,
                                                                                  dwLogoTexture,
                                                                                  instructionsTexture,
                                                                                  config);
                        return guiComp;
                    }
                case "BoundingBox":
                    {
                        float width = float.Parse(config.GetParameterValue("width"));
                        float height = float.Parse(config.GetParameterValue("height"));
                        float length = float.Parse(config.GetParameterValue("length"));

                        gameplay.CustomBoundingBox bboxComp = new gameplay.CustomBoundingBox(width, height, length);
                        return bboxComp;
                    }
            }

            return null;
        }

        private void LoadIconTextures(ContentManager content, 
                                 string[] icons, 
                                 string[] iconNames, 
                                 Dictionary<string, Texture2D> iconTextures)
        {
            for(int i = 0; i < icons.Length; ++i)
            {
                Texture2D iconTexture = content.Load<Texture2D>(icons[i]);
                iconTextures.Add(iconNames[i], iconTexture);
            }
        }

        private UnitComponent.Ratings ConfigureRatings(core.ComponentConfig config)
        {
            int iAttackPower = int.Parse(config.GetParameterValue("AttackPower"));
            int iDefensePower = int.Parse(config.GetParameterValue("DefensePower"));
            float fReloadTimeInSecs = float.Parse(config.GetParameterValue("ReloadTimeInSeconds"));
            float fAttackRange = float.Parse(config.GetParameterValue("AttackRange"));
            float fViewRange = float.Parse(config.GetParameterValue("ViewRange"));
            float fTopSpeed = float.Parse(config.GetParameterValue("TopSpeed"));
            string team = config.GetParameterValue("Team");

            UnitComponent.Ratings ratings = new UnitComponent.Ratings(iAttackPower,
                                                                      iDefensePower,
                                                                      fReloadTimeInSecs,
                                                                      fAttackRange,
                                                                      fViewRange,
                                                                      fTopSpeed,
                                                                      team);
            return ratings;
        }

        public void LoadContent(core.Game game)
        {
            _enemyTeamGameObjFilter = new core.GameObjectTypeFilter(_enemyTeamFilter);

            _font = game.Content.Load<SpriteFont>("Fonts\\Kootenay");
            //Lua engine
            _lua = new Lua();
            //To declare the statemaching functions used in lua
            statemachine = new StateMachine(_lua);

            XmlTextReader reader = new XmlTextReader(m_configFile);

            m_levelConfigs = new List<LevelConfig>();

            LevelConfig curLevelConfig = null;
            GameObjectInstanceConfig curGameObjInstCfg = null;
            GameObjectPool curGameObjPool = null;
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "Level":
                        {
                            curLevelConfig = new LevelConfig();
                            m_levelConfigs.Add(curLevelConfig);
                        }
                        break;
                    case "GameObjectPool":
                        {
                            curGameObjPool = new GameObjectPool();
                            curGameObjPool.Class = reader.GetAttribute("class");
                            curGameObjPool.Count = int.Parse(reader.GetAttribute("count"));

                            curLevelConfig.GameObjectPools.Add(curGameObjPool);
                        }
                        break;
                    case "GameObjectInstance":
                        {
                            curGameObjInstCfg = new GameObjectInstanceConfig();
                            curGameObjInstCfg.Class = reader.GetAttribute("class");
                            curGameObjInstCfg.ObjectName = reader.GetAttribute("name");
                            curGameObjInstCfg.Position = reader.GetAttribute("position");
                            curGameObjInstCfg.LookAt = reader.GetAttribute("lookAt");
                            curGameObjInstCfg.LookVector = reader.GetAttribute("lookVector");
                            curGameObjInstCfg.delay = reader.GetAttribute("delay");

                            curLevelConfig.GameObjectInstanceConfigs.Add(curGameObjInstCfg);
                        }
                        break;
                    case "Param":
                        {
                            curGameObjInstCfg.SetParameter(reader.GetAttribute("name"),
                                                           reader.GetAttribute("value"));
                        }
                        break;
                }
            }

            game.PostOffice.RegisterForMessages(typeof(com.MsgLoadLevel), this);
            game.PostOffice.RegisterForMessages(typeof(com.MsgReloadLevel), this);

            m_currentLevel = 0;
            LoadLevel(m_currentLevel);
        }

        void LoadLevel(int level)
        {
            core.Game game = core.Game.instance();

            game.OnUnLoadLevel(m_currentLevel);

            if (level >= m_levelConfigs.Count)
            {
                level = 0;
                m_currentLevel = 0;
            }

            _endGameGUI = null;
            _commanderUnit = null;

            game.DeleteAllGameObjects();
            
            _enemyUnits.Clear();
            _gremlinSpawners.Clear();

            LevelConfig levelConfig = m_levelConfigs[level];

            foreach(GameObjectPool poolConfig in levelConfig.GameObjectPools)
            {
                game.CreateGameObjectPool(poolConfig.Class, poolConfig.Count);
            }

            foreach (GameObjectInstanceConfig config in levelConfig.GameObjectInstanceConfigs)
            {
                double delay = 0; //for how late you have to start
                float[] posXYZ = parseFloatArrayValue(config.Position);
                Vector3 pos = new Vector3(posXYZ[0], 
                                          posXYZ[1], 
                                          posXYZ[2]);

                Vector3 lookAtVec = Vector3.Zero;
                if (config.LookAt != null)
                {
                    float[] lookXYZ = parseFloatArrayValue(config.LookAt);
                    lookAtVec = new Vector3(lookXYZ[0], lookXYZ[1], lookXYZ[2]);
                }
                else if (config.LookVector != null)
                {
                    float[] lookXYZ = parseFloatArrayValue(config.LookVector);
                    lookAtVec = new Vector3(pos.X + lookXYZ[0],
                                            pos.Y + lookXYZ[1],
                                            pos.Z + lookXYZ[2]);
                }
                if (config.delay != null)
                {
                    delay = Convert.ToDouble(config.delay);
                }

                core.GameObject gameObject = game.CreateGameObjectWithParams(config.Class, 
                                                                             config.ObjectName,
                                                                             config.ParamValues);

                gameObject.Position = pos;
                gameObject.LookAt = lookAtVec;
                gameObject.delay = delay;
            }

            game.OnLoadLevel(m_currentLevel);
        }

        public float[] parseFloatArrayValue(string strValue)
        {
            string[] parts = strValue.Split(',');

            float[] valueParts = new float[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                valueParts[i] = float.Parse(parts[i]);

            return valueParts;
        }

        protected void ComputeCameraStartingPosition(core.GameObject camera,
                                                     core.GameObject testModel)
        {
            BoundingSphere bsphere = testModel.BoundingSphere;
            camera.Position = bsphere.Center +
                        (Vector3.UnitZ * bsphere.Radius * 3.0f) +
                        (Vector3.UnitY * bsphere.Radius);
            camera.LookAt = bsphere.Center;
        }

        public void Update(DeskWars.core.GameObjectDB gameObjectDB, 
                          Microsoft.Xna.Framework.GameTime gameTime)
        {
            _enemyUnits.Clear();
            gameObjectDB.GetGameObjectsByType(_enemyTeamGameObjFilter, _enemyUnits);

            //if commander unit exists then we are in game
            if (_commanderUnit != null && _commanderUnit.UnitStats.health <= 0 && _endGameGUI == null)
            {
                core.GameObjectFactory factory =
                    (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
                _endGameGUI = factory.CreateGameObject("YouLoseWidget", "YouLose");
                //_endGameGUI.OnLoadLevel(m_currentLevel);
                _endGameGUI.Position = new Vector3(0.05f, 0.05f, 0.0f);
            }
            else if (_commanderUnit != null && AllEnemyUnitsDead() && _endGameGUI == null)
            {
                core.GameObjectFactory factory =
                    (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
                _endGameGUI = factory.CreateGameObject("YouWinWidget", "YouWin");
                //_endGameGUI.OnLoadLevel(m_currentLevel);
                _endGameGUI.Position = new Vector3(0.05f, 0.05f, 0.0f);
            }
        }

        protected bool AllEnemyUnitsDead()
        {
            foreach (core.GameObject enemy in _enemyUnits)
            {
                enemy.VisitComponents(_finder);
                if (_finder.TheUnit.UnitStats.health > 0)
                    return false;
            }

            foreach (gameplay.GremlinSpawner spawner in _gremlinSpawners)
            {
                if (spawner.maxGremlinsSpawned != 0)
                    return false;
            }

            return true;
        }

        public void OnGameObjectDeleted(DeskWars.core.GameObject gameObject)
        {
            gameObject.IsActive = false;
            //throw new NotImplementedException();
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            if (msg.GetType() == typeof(com.MsgLoadLevel))
            {
                ++m_currentLevel;
                LoadLevel(m_currentLevel);
            }
            else if (msg.GetType() == typeof(com.MsgReloadLevel))
            {
                LoadLevel(m_currentLevel);
            }
        }

        #endregion

        #region Engine Members

        public void OnLoadLevel(int level)
        {
            //_lua = new Lua();
        }

        public void OnUnloadLevel(int level)
        {
            //_lua.Dispose();
            //_lua.Close();
            
        }

        #endregion
    }
}
