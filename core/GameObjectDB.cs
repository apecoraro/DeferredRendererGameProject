using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    public class GameObjectDB
    {
        List<GameObject> _gameObjects = new List<GameObject>();
        List<GameObject> _newGameObjects = new List<GameObject>();
        List<GameObject> _deleteGameObjects = new List<GameObject>();
        public void AddGameObject(GameObject gameObject)
        {
            _newGameObjects.Add(gameObject);
        }

        public void DeleteGameObject(GameObject gameObject)
        {
            //add to the delete list, remove at next update
            _deleteGameObjects.Add(gameObject);
        }

        public int GetAllGameObjects(GameObject[] gameObjectArray)
        {
            _gameObjects.CopyTo(gameObjectArray);
            return _gameObjects.Count;
        }

        public int GetGameObjectCount()
        {
            return _gameObjects.Count;
        }

        public GameObject GetGameObject(int index)
        {
            return _gameObjects[index];
        }

        public GameObject GetGameObject(string name)
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject.Name == name)
                    return gameObject;
            }

            return null;
        }

        public GameObject GetGameObject(ref int index,
                                        GameObjectTypeFilter filter)
        {
            while (index < _gameObjects.Count)
            {
                GameObject gameObj = _gameObjects[index];
                if (filter.IsAcceptableType(gameObj))
                    return gameObj;
                ++index;
            }
            return null;
        }

        public void OnLoadLevel(int level)
        {
            //make sure that the list of game objects
            //is up to date
            Update(new GameTime());

            for (int i = 0; i < _gameObjects.Count; ++i)
            {
                GameObject gameObj = _gameObjects[i];
                gameObj.OnLoadLevel(level);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0 ; i < _deleteGameObjects.Count; ++i)
            {
                GameObject gameObj = _deleteGameObjects[i];
                _gameObjects.Remove(gameObj);
            }
            _deleteGameObjects.Clear();

            if (_newGameObjects.Count > 0)
            {
                _gameObjects.AddRange(_newGameObjects);
                _newGameObjects.Clear();
            }

            for (int i = 0; i < _gameObjects.Count; ++i)
            {
                GameObject gameObj = _gameObjects[i];
                gameObj.Update(gameTime);
            }

            if (_newGameObjects.Count > 0)
            {
                _gameObjects.AddRange(_newGameObjects);
                _newGameObjects.Clear();
            }
        }

        public int GetGameObjectsByType(GameObjectTypeFilter filter,
                                        List<GameObject> gameObjects)
        {
            foreach (GameObject gameObj in _gameObjects)
            {
                if (filter.IsAcceptableType(gameObj))
                    gameObjects.Add(gameObj);
            }
            return gameObjects.Count;
        }

        public GameObject GetGameObjectIntersectCircle(ref int index, 
                                                       Vector3 center, 
                                                       float radius)//input sphere, in/out list of game objects, returns num found
        {
            center.Y = 0.0f;
            while(index < _gameObjects.Count)
            {
                GameObject gameObj = _gameObjects[index];
                if (IntersectXZCircle(gameObj, center, radius))
                    return gameObj;
                ++index;
            }
            return null;
        }

        public int GetGameObjectsIntersectCircle(Vector3 center, 
                                                 float radius, 
                                                 List<GameObject> gameObjects)//input sphere, in/out list of game objects, returns num found
        {
            int origCount = gameObjects.Count;

            center.Y = 0.0f;
            foreach(GameObject gameObj in _gameObjects)
            {
                if (IntersectXZCircle(gameObj, center, radius))
                    gameObjects.Add(gameObj);
            }

            return gameObjects.Count - origCount;
        }

        public int GetGameObjectsIntersectFrustum(BoundingFrustum frustum,
                                                 GameObjectTypeFilter filter,
                                                 List<GameObject> gameObjects)//input sphere, in/out list of game objects, returns num found
        {
            int origCount = gameObjects.Count;

            foreach (GameObject gameObj in _gameObjects)
            {
                if (frustum.Intersects(gameObj.BoundingSphere) && filter.IsAcceptableType(gameObj))
                    gameObjects.Add(gameObj);
            }

            return gameObjects.Count - origCount;
        }

        protected bool IntersectXZCircle(GameObject gameObj, 
                                         Vector3 center, 
                                         float radius)
        {
            Vector3 xz = gameObj.Position;
            xz.Y = 0.0f;
            Vector3 diff = xz - center;

            float len2 = diff.LengthSquared();

            float radius2 = radius * gameObj.BoundingSphere.Radius;
            return (len2 < radius2);
        }

        public GameObject GetClosestGameObjectIntersectRay(Ray ray)
        {
            float shortestDist = float.MaxValue;
            GameObject closestObj = null;
            foreach (GameObject gameObj in _gameObjects)
            {
                float? dist = ray.Intersects(gameObj.BoundingSphere);
                if (dist < shortestDist)
                {
                    shortestDist = (float)dist;
                    closestObj = gameObj;
                }
            }

            return closestObj;
        }

        public GameObject GetClosestGameObjectIntersectRay(Ray ray,
                                                           GameObjectTypeFilter filter)
        {
            float shortestDist = float.MaxValue;
            GameObject closestObj = null;
            foreach (GameObject gameObj in _gameObjects)
            {
                if (!filter.IsAcceptableType(gameObj))
                    continue;

                float? dist = ray.Intersects(gameObj.BoundingSphere);
                if (dist < shortestDist)
                {
                    shortestDist = (float)dist;
                    closestObj = gameObj;
                }
            }

            return closestObj;
        }

        public GameObject GetClosestGameObject(GameObject gameObject,
                                                   GameObjectTypeFilter filter)
        {
            float shortestDist = float.MaxValue;
            GameObject closestObj = null;
            foreach (GameObject gameObj in _gameObjects)
            {
                if (gameObj == gameObject)
                    continue;
                if (!filter.IsAcceptableType(gameObj))
                    continue;
                //if (gameObj.Class != "Gremlin")
                    //continue;
                    //Hack to get ONLY gremlins

                    float? dist = (gameObject.Position - gameObj.Position).LengthSquared();
                    if (dist < shortestDist)
                    {
                        shortestDist = (float)dist;
                        closestObj = gameObj;
                    }
                
            }

            return closestObj;
        }

        public int GetGameObjectsIntersectRay(Ray ray, 
                                              List<GameObject> intersectedGameObjs)
        {
            foreach (GameObject gameObj in _gameObjects)
            {
                if (ray.Intersects(gameObj.BoundingSphere) != null)
                {
                    intersectedGameObjs.Add(gameObj);
                }
            }

            return intersectedGameObjs.Count;
        }

        public int GetGameObjectsIntersectRay(Ray ray,
                                              GameObjectTypeFilter filter,
                                              List<GameObject> intersectedGameObjs)
        {
            foreach (GameObject gameObj in _gameObjects)
            {
                if (!filter.IsAcceptableType(gameObj))
                    continue;

                if (ray.Intersects(gameObj.BoundingSphere) != null)
                {
                    intersectedGameObjs.Add(gameObj);
                }
            }

            return intersectedGameObjs.Count;
        }

/*        public int GetGameObjectsIntersectBox(BoundingBox box, GameObjectTypeFilter filter, List<GameObject> intersectedGameObjs)
        {
            foreach (GameObject gameObj in _gameObjects)
            {
                if (!filter.IsAcceptableType(gameObj))
                    continue;

                if (box.Intersects(gameObj.BoundingBox))
                {
                    intersectedGameObjs.Add(gameObj);
                }
            }

            return intersectedGameObjs.Count;
        } */

        public int GetGameObjectsIntersectBox(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, 
            Vector3 centerPt, GameObjectTypeFilter filter, List<GameObject> intersectedGameObjs)
        {
            int leftLine = LocationRelatedToLine(topLeft, bottomLeft, centerPt);
            int topLine = LocationRelatedToLine(topLeft, topRight, centerPt);
            int rightLine = LocationRelatedToLine(topRight, bottomRight, centerPt);
            int bottomLine = LocationRelatedToLine(bottomLeft, bottomRight, centerPt);

            foreach (GameObject gameObj in _gameObjects)
            {
                if (!filter.IsAcceptableType(gameObj))
                    continue;

                int objPos = LocationRelatedToLine(topLeft, bottomLeft, gameObj.Position);
                if ((leftLine <= 0 && objPos > 0) || (leftLine >= 0 && objPos < 0))
                    continue;

                objPos = LocationRelatedToLine(topLeft, topRight, gameObj.Position);
                if ((topLine <= 0 && objPos > 0) || (topLine >= 0 && objPos < 0))
                    continue;

                objPos = LocationRelatedToLine(topRight, bottomRight, gameObj.Position);
                if ((rightLine <= 0 && objPos > 0) || (rightLine >= 0 && objPos < 0))
                    continue;

                objPos = LocationRelatedToLine(bottomLeft, bottomRight, gameObj.Position);
                if ((bottomLine <= 0 && objPos > 0) || (bottomLine >= 0 && objPos < 0))
                    continue;

                intersectedGameObjs.Add(gameObj);
            }

            return intersectedGameObjs.Count;
        }

        int LocationRelatedToLine(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            int dx1, dx2, dy1, dy2;

            dx1 = (int)(p1.X - p0.X); dy1 = (int)(p1.Z - p0.Z);
            dx2 = (int)(p2.X - p0.X); dy2 = (int)(p2.Z - p0.Z);

            if (dx1 * dy2 > dy1 * dx2)
                return +1;
            if (dx1 * dy2 < dy1 * dx2)
                return -1;
            if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0))
                return -1;
            if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2))
                return +1;
            return 0;
        }
    }
}
