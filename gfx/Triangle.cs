using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class Triangle
    {
        Vector3[] _points;
        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _points = new Vector3[3];
            _points[0] = p1;
            _points[1] = p2;
            _points[2] = p3;
        }

        public Vector3 Points(int index) { return _points[index]; }
    }
}
