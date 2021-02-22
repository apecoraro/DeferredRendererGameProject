using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public interface ComponentVisitor
    {
        void Apply(core.Component comp);
    }
}
