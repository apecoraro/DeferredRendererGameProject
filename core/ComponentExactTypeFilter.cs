using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public class ComponentExactTypeFilter : ComponentTypeFilter
    {
        Type _filterType;
        public Type FilterType
        {
            get { return _filterType; }
            set { _filterType = value; }
        }

        public ComponentExactTypeFilter(Type filterType) { _filterType = filterType; }
        #region ComponentTypeFilter Members

        public bool Accept(DeskWars.core.GameObject gameObject,
                           DeskWars.core.Component comp)
        {
            return comp.GetType() == _filterType;
        }

        #endregion
    }
}
