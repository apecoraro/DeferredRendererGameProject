using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    public class GremlinSpawner : core.Component
    {
        #region MemberVariables

        float startDelay;
        float respawnDelay;
        public int maxGremlinsSpawned;
        float spawnerTime;
        int gremlinNumber;

        #endregion

        public GremlinSpawner(float start, float respawn, int total)
        {
            startDelay = start;
            respawnDelay = respawn;
            maxGremlinsSpawned = total;
            spawnerTime = 0.0f;
            gremlinNumber = 1;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
//            throw new NotImplementedException();
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
//            throw new NotImplementedException();
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
//            throw new NotImplementedException();
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            // Conver to seconds
            float timeElapsed = gameTime.ElapsedGameTime.Milliseconds;
            timeElapsed = timeElapsed / 1000;
            spawnerTime += timeElapsed;
            
//            spawnerTime += gameTime.ElapsedGameTime.Milliseconds;

            if (startDelay >= 0.0f)
            {
                if (spawnerTime >= startDelay && maxGremlinsSpawned > 0)
                {
                    spawnerTime -= startDelay;
                    // This is just to differentiate between a spawner that spawns a gremlin right away, 
                    // and a spawner that has already spawned it's first gremlin.
                    startDelay = -1.0f; 

                    // Spawn first Gremlin.
                    core.GameObjectFactory factory = 
                        (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
                    string gremName = gameObject.Name + " : Gremlin #" + gremlinNumber;
                    core.GameObject gremlin = factory.CreateGameObject("Gremlin", gremName);
                    gremlin.Position = gameObject.Position;
                    gremlin.LookVector = gameObject.LookVector;

                    gremlinNumber++;

                    maxGremlinsSpawned--;
                }
            }
            else
            {
                if (spawnerTime >= respawnDelay)
                {
                    spawnerTime -= respawnDelay;

                    // Spawn new Gremlin.
                    core.GameObjectFactory factory =
                        (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
                    string gremName = gameObject.Name + " : Gremlin #" + gremlinNumber;
                    core.GameObject gremlin = factory.CreateGameObject("Gremlin", gremName);
                    gremlin.Position = gameObject.Position;
                    gremlin.LookVector = gameObject.LookVector;

                    gremlinNumber++;

                    maxGremlinsSpawned--;
                }
            }

            if (maxGremlinsSpawned == 0)
            {
                // Gremlin Spawner should delete itself. 
                core.Game.instance().DeleteGameObject(gameObject);
//                gameObject.Delete();
                
            }

/*            core.GameObjectFactory factory =
                            (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));
            core.GameObject gremlin = factory.CreateGameObject("Gremlin", "My Gremlin's Name");
            gremlin.Position = gameObject.Position;
            gremlin.LookVector = gameObject.LookVector; */

            return;
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }
    }
}