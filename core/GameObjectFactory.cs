using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    interface GameObjectFactory
    {
        GameObject CreateGameObject(string className, string objName);
        void DeleteGameObject(GameObject gameObject);
        void DeleteGameObject(string objName);
    }
}
