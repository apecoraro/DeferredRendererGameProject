using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    public interface Engine
    {
        void Configure(String configFile);
        void LoadContent(core.Game game);
        void CreateGameObjectComponents(GameObjectConfig config, 
                                        GameObject gameObject);
        //void CreateGameObjectComponent(ComponentConfig config,
                                        //GameObject gameObject);
        //Component CreateComponent(ComponentConfig config);
        void OnGameObjectDeleted(GameObject gameObject);
        void Update(GameObjectDB gameObjectDB, 
                    GameTime gameTime);
        void OnLoadLevel(int level);
        void OnUnloadLevel(int level);
    }
}
