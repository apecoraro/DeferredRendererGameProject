using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.gfx
{
    public interface TriangleIterator
    {
        Triangle GetNextTriangle();

        void Reset();
    }
}