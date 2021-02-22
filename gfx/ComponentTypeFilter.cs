using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.core;

namespace DeskWars.gfx
{
    public class ComponentTypeFilter : core.ComponentTypeFilter
    {
        #region ComponentTypeFilter Members

        public bool Accept(DeskWars.core.GameObject gameObject,
                           DeskWars.core.Component comp)
        {
            gfx.Component gfxComp = comp as gfx.Component;
            if (gfxComp != null)
            {
                GameObjectGfxCompPair pair = new GameObjectGfxCompPair(gameObject, gfxComp);
                GfxComponents.Add(pair);
                return true;
            }

            return false;
        }

        public struct GameObjectGfxCompPair
        {
            public GameObjectGfxCompPair(GameObject gameObj, gfx.Component gfx)
            {
                GameObject = gameObj;
                GfxComponent = gfx;
            }
            public GameObject GameObject;
            public gfx.Component GfxComponent;
        }

        public List<GameObjectGfxCompPair> GfxComponents = new List<GameObjectGfxCompPair>();
        public List<GameObject> GameObjects = new List<GameObject>();

        public void Reset()
        {
            GfxComponents.Clear();
            GameObjects.Clear();
        }
        #endregion
    }
}
