using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public class GameObjectTypeFilter
    {
        ComponentTypeFilter _filter;
        public GameObjectTypeFilter(ComponentTypeFilter filter)
        {
            _filter = filter;
        }

        public ComponentTypeFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
#if DEBUG
        public bool IsAcceptableType(GameObject gameObject)
        {
            bool ret = false;
            for (int i = 0; i < gameObject.GetComponentCount(); ++i)
            {
                if (_filter.Accept(gameObject, gameObject.GetComponent(i)))
                {
                    ret = true;
                }
            }

            return ret;
        }
#else
        public bool IsAcceptableType(GameObject gameObject)
        {
            for (int i = 0; i < gameObject.GetComponentCount(); ++i)
            {
                if (_filter.Accept(gameObject, gameObject.GetComponent(i)))
                {
                    return true;
                }
            }

            return false;
        }
#endif
    }
}
