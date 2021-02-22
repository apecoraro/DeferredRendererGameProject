using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace DeskWars.core
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game, GameObjectFactory
    {
        //performance stats
        Timer _timer = new Timer();

        double _engineUpdateTime = 0;
        public double EngineUpdateTime
        {
            get { return _engineUpdateTime; }
        }

        double _gameObjectDBUpdateTime = 0;
        public double GameObjectDBUpdateTime
        {
            get { return _gameObjectDBUpdateTime; }
        }

        double _renderTime = 0;
        public double RenderTime
        {
            get { return _renderTime; }
        }

        public int FrameCounter = 0;

        GraphicsSettingsManager _graphics;

        GameObjectDB _gameObjectDB;
        public GameObjectDB GameObjectDB
        {
            get { return _gameObjectDB; }
            set { _gameObjectDB = value; }
        }
        com.PostOffice _postOffice = new com.PostOffice();
        public com.PostOffice PostOffice
        {
            get { return _postOffice; }
        }

        public class KeyboardMouseInput
        {
            public MouseState GetMouseState()
            {
                return Mouse.GetState();
            }

            public bool MouseInputHandled = false;

            public KeyboardState GetKeyboardState()
            {
                return Keyboard.GetState();
            }

            public bool KeyboardInputHandled = false;
        }

        KeyboardMouseInput _keyboardMouseInput = new KeyboardMouseInput();
        public KeyboardMouseInput Input
        {
            get { return _keyboardMouseInput; }
        }

        List<Engine> _engines;
        List<Renderer> _renderers;
        Dictionary<String, GameObjectConfig> _gameObjectConfigs;

        static Game s_pGame = null;
        public static Game instance()
        {
            return s_pGame;
        }

        public Game()
        {
            Content.RootDirectory = "Content";
            s_pGame = this;
            _graphics = new GraphicsSettingsManager(this);

            //uncomment variables belw for performance analysis
            //IsFixedTimeStep = false;
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.Configure(false, 1024, 768, true);
            
            _gameObjectDB = new GameObjectDB();
            _engines = new List<Engine>();
            _renderers = new List<Renderer>();
            _gameObjectConfigs = new Dictionary<string, GameObjectConfig>();
            this.IsMouseVisible = false;
            
            this.Services.AddService(typeof(GameObjectFactory), this);
        }

        public void ToggleFullscreen()
        {
            if (_graphics.IsFullScreen)
            {
                _graphics.Configure(false, 800, 600, true);
                _graphics.ApplyChanges();
            }
            else
            {
                _graphics.Configure(true, 1024, 768, true);
                _graphics.ApplyChanges();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            String configFilePath = System.IO.Path.Combine("config", "GameConfig.xml");
            BuildEngines(configFilePath);

            configFilePath = System.IO.Path.Combine("config", "GameObjectConfig.xml");
            BuildGameObjectConfigs(configFilePath);

            base.Initialize();
        }

        protected void BuildEngines(String configFilePath)
        {
            XmlTextReader reader = new XmlTextReader(configFilePath);

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "Engine":
                        {
                            Engine engine = BuildEngine(reader);
                            _engines.Add(engine);
                        }
                        break;
                }
            }
        }

        protected Engine BuildEngine(XmlTextReader reader)
        {
            switch (reader.GetAttribute("name"))
            {
                case "GraphicsEngine":
                    {
                        gfx.Engine graphicsEngine = new gfx.Engine();
                        graphicsEngine.Configure(reader.GetAttribute("config"));
                        _renderers.Add(graphicsEngine);
                        return graphicsEngine;
                    }
                case "AIEngine":
                    {
                        ai.Engine aiEngine = new ai.Engine();
                        aiEngine.Configure(reader.GetAttribute("config"));
                        return aiEngine;
                    }
                case "PhysicsEngine":
                    {
                        physics.Engine physicsEngine = new physics.Engine();
                        physicsEngine.Configure(reader.GetAttribute("config"));
                        return physicsEngine;
                    }
                case "GameplayEngine":
                    {
                        gameplay.Engine gamePlayEngine = new gameplay.Engine();
                        gamePlayEngine.Configure(reader.GetAttribute("config"));
                        return gamePlayEngine;
                    }
                case "GUIEngine":
                    {
                        gui.Engine gui = new gui.Engine();
                        gui.Configure(reader.GetAttribute("config"));
                        _renderers.Add(gui);
                        return gui;
                    }
                case "SoundEngine":
                    {
                        sound.Engine sound = new sound.Engine();
                        sound.Configure(reader.GetAttribute("config"));
                        return sound;
                    }
                default:
                    return null;
            }
        }

        protected void BuildGameObjectConfigs(String configFile)
        {
            XmlTextReader reader = new XmlTextReader(configFile);

            GameObjectConfig config = null;
            ComponentConfig compCfg = null;
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "GameObject":
                        {
                            config = new GameObjectConfig(reader.GetAttribute("name"));

                            _gameObjectConfigs[config.Name] = config;
                        }
                        break;
                    case "Component":
                        {
                            compCfg = new ComponentConfig(reader.GetAttribute("name"));

                            config.AddComponentConfig(compCfg);
                        }
                        break;
                    case "Param":
                        {
                            compCfg.SetParameter(reader.GetAttribute("name"), 
                                                 reader.GetAttribute("value"));
                        }
                        break;
                }
            }
        }

        class GameObjectPool
        {
            public string ClassName;
            public HashSet<GameObject> Pool = new HashSet<GameObject>();
            public LinkedList<GameObject> FreePool = new LinkedList<GameObject>();
            public Game GameObjectFactory;
            public void Add(GameObject gameObject)
            {
                this.Pool.Add(gameObject);
                this.FreePool.AddFirst(gameObject);
            }

            public bool ReturnToPool(GameObject gameObject)
            {
                if (this.Pool.Contains(gameObject))
                {
                    gameObject.IsActive = false;
                    this.FreePool.AddFirst(gameObject);

                    return true;
                }

                return true;
            }

            public GameObject GetFromPool()
            {
                if (this.FreePool.Count == 0)
                {
                    this.AllocatePool(this.Pool.Count + this.Pool.Count);
                }

                GameObject gameObject = this.FreePool.First.Value;
                gameObject.IsActive = true;

                this.FreePool.RemoveFirst();

                return gameObject;
            }

            public void AllocatePool(int count)
            {
                int total = this.Pool.Count + count;
                for (int i = this.Pool.Count; i < total; ++i)
                {
                    GameObject gameObject = GameObjectFactory.ConstructGameObject(this.ClassName,
                                                                this.ClassName + "_Pool_" + i.ToString(),
                                                                null);
                    gameObject.IsActive = false;
                    this.Add(gameObject);
                }
            }
        }

        Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();

        public void CreateGameObjectPool(String className, int initialCount)
        {
            GameObjectPool pool;
            if (!_pools.TryGetValue(className, out pool))
            {
                pool = new GameObjectPool();
                pool.ClassName = className;
                pool.GameObjectFactory = this;
                _pools[className] = pool;
            }

            pool.AllocatePool(initialCount);
        }

        protected bool ConstructFromPool(String className, ref GameObject gameObject)
        {
            GameObjectPool pool;
            if (_pools.TryGetValue(className, out pool))
            {
                gameObject = pool.GetFromPool();
                return true;
            }

            return false;
        }

        protected bool ReturnToPool(GameObject gameObject)
        {
            GameObjectPool pool;
            if (_pools.TryGetValue(gameObject.Class, out pool))
            {
                return pool.ReturnToPool(gameObject);
            }

            return false;
        }

        public GameObject CreateGameObject(String className, String objName)
        {
            return CreateGameObjectWithParams(className, objName, null);
        }

        public GameObject CreateGameObjectWithParams(String className, 
                                                     String objName, 
                                                     Dictionary<String, String> paramValues)
        {
            
            GameObject gameObject = null;
            if(!ConstructFromPool(className, ref gameObject))
                gameObject = ConstructGameObject(className, objName, paramValues);

            if(gameObject != null)
                _gameObjectDB.AddGameObject(gameObject);

            return gameObject;
        }

        protected GameObject ConstructGameObject(String className,
                                           String objName,
                                           Dictionary<String, String> paramValues)
        {
            GameObjectConfig config;
            if (_gameObjectConfigs.TryGetValue(className, out config))
            {
                config.ParamValues = paramValues;

                GameObject gameObject = new GameObject(objName, className);
                for (int i = 0; i < _engines.Count; ++i)
                {
                    Engine engine = _engines[i];
                    engine.CreateGameObjectComponents(config, gameObject);
                }

                config.ParamValues = null;
                return gameObject;
            }

            return null;
        }

        public void DeleteAllGameObjects()
        {
            for (int i = 0; i < _gameObjectDB.GetGameObjectCount(); ++i)
            {
                GameObject gameObj = _gameObjectDB.GetGameObject(i);
                DeleteGameObject(gameObj);
            }
        }

        public void DeleteGameObject(String objName)
        {
            core.GameObject gameObject = _gameObjectDB.GetGameObject(objName);
            if(gameObject != null)
                DeleteGameObject(gameObject);
        }

        public void DeleteGameObject(GameObject gameObject)
        {
            _gameObjectDB.DeleteGameObject(gameObject);

            if(!ReturnToPool(gameObject))//if this is a pool object then just return and deactivate it
                gameObject.Delete();//this calls OnRemoveFromGameObject on each Component

            DeleteGameObjectFromEngines(gameObject);
        }

        protected void DeleteGameObjectFromEngines(GameObject gameObject)
        {
            foreach (Engine engine in _engines)
            {
                engine.OnGameObjectDeleted(gameObject);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            foreach (Engine engine in _engines)
            {
                engine.LoadContent(this);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        public void Quit()
        {
            this.Exit();
        }

        enum GameState
        {
            PLAY,
            PAUSE
        }

        GameState State = GameState.PLAY;

        public void Pause()
        {
            this.State = GameState.PAUSE;
            this.IsMouseVisible = true;
        }

        public void UnPause()
        {
            this.State = GameState.PLAY;
            this.IsMouseVisible = false;
        }

        public void OnLoadLevel(int level)
        {
            for (int i = 0; i < _engines.Count; ++i)
            {
                Engine engine = _engines[i];
                engine.OnLoadLevel(level);
            }

            _gameObjectDB.OnLoadLevel(level);
        }

        public void OnUnLoadLevel(int level)
        {
            for (int i = 0; i < _engines.Count; ++i)
            {
                Engine engine = _engines[i];
                engine.OnUnloadLevel(level);
            }
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            _postOffice.Deliver();

            _keyboardMouseInput.KeyboardInputHandled = false;
            _keyboardMouseInput.MouseInputHandled = false;

            _timer.Start();
            for(int i = 0; i < _engines.Count; ++i)
            {
                Engine engine = _engines[i];
                engine.Update(_gameObjectDB, gameTime);
            }
            _timer.Stop();
            _engineUpdateTime = _timer.Duration;

            _timer.Start();
            if(this.State == GameState.PLAY)
                _gameObjectDB.Update(gameTime);
            _timer.Stop();
            _gameObjectDBUpdateTime = _timer.Duration;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _timer.Start();
            foreach (Renderer renderer in _renderers)
            {
                renderer.Draw(_gameObjectDB, gameTime);
            }
            _timer.Stop();
            _renderTime = _timer.Duration;

            base.Draw(gameTime);

            ++FrameCounter;
        }
    }
}
