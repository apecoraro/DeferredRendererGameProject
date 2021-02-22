using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public class ComponentExactMultiTypeFilter : ComponentTypeFilter
    {
        List<Type> _filterTypes;
        public ComponentExactMultiTypeFilter(List<Type> filterTypes)
        {
            _filterTypes = filterTypes;
        }

        public ComponentExactMultiTypeFilter(Type type)
        {
            _filterTypes.Add(type);
        }

        public List<Type> FilterTypes { get { return _filterTypes; } }
    
        #region ComponentTypeFilter Members

        public bool Accept(DeskWars.core.GameObject gameObject,
                           DeskWars.core.Component comp)
        {
            foreach (Type type in _filterTypes)
            {
                if (comp.GetType() == type)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
