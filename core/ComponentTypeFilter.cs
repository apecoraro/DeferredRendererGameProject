using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public interface ComponentTypeFilter
    {
        bool Accept(GameObject gameObject, Component comp);
    }
}
