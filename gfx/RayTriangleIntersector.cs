using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class RayTriangleIntersector : core.ComponentVisitor
    {
        Ray _ray;
        Vector3 _isectPt = Vector3.Zero;
        Triangle _isectTri = null;
        float _isectDist;
        bool _isectFound = false;

        public RayTriangleIntersector(Ray ray)
        {
            _ray = ray;
            Reset();
        }

        public void Reset()
        {
            _isectPt = Vector3.Zero;
            _isectTri = null;
            _isectDist = float.MaxValue;
            _isectFound = false;
        }

        #region ComponentVisitor Members

        public void Apply(DeskWars.core.Component comp)
        {
            gfx.Component gfx = comp as gfx.Component;

            if (gfx != null)
            {
                TriangleIterator itr = gfx.GetTriangleIterator();
                if (itr != null)
                {
                    Triangle tri = null;
                    while ((tri = itr.GetNextTriangle()) != null)
                    {
                        float dist;
                        if (RayIntersectsTriangle(_ray, tri, out dist))
                        {
                            if (dist < _isectDist)
                            {
                                _isectPt = _ray.Position + (_ray.Direction * dist);
                                _isectTri = tri;
                                _isectFound = true;
                            }
                        }
                    }
                }
            }
        }

        float CalcPlaneD(Vector3 pointOnPlane,
                         Vector3 planeNormal)
        {
            float Ax = (planeNormal.X * pointOnPlane.X);
            float By = (planeNormal.Y * pointOnPlane.Y);
            float Cz = (planeNormal.Z * pointOnPlane.Z);
            return (-Ax - By - Cz);
        }

        void CalcTriNormal(ref Vector3 normal,
                           Vector3 p1,
                           Vector3 p2,
                           Vector3 p3)
        {
            float ux, uy, uz, vx, vy, vz;

            ux = p2.X - p1.X;
            uy = p2.Y - p1.Y;
            uz = p2.Z - p1.Z;
            vx = p3.X - p1.X;
            vy = p3.Y - p1.Y;
            vz = p3.Z - p1.Z;

            normal.X = uy*vz - uz*vy;
            normal.Y = uz*vx - ux*vz;
            normal.Z = ux*vy - uy*vx;
        }

        float DistRayPlane(Vector3 rayOrigin,
                           Vector3 rayDir,
                           Vector3 planeNormal,
                           float planeD)
        {
            float cosAlpha = Vector3.Dot(planeNormal, rayDir);

            // no intersection
            if (Math.Abs(cosAlpha) < 0.01f)
                return -1.0f;

            return (-(planeD + Vector3.Dot(planeNormal, rayOrigin)) / cosAlpha);
        }

        bool TriContainsPoint(Vector3 A,
                              Vector3 B,
                              Vector3 C,
                              Vector3 P)
        {
            //the description of this algorithm can be found here:
            //http://www.blackpawn.com/texts/pointinpoly/default.html
            //or in this book: http://realtimecollisiondetection.net/
            Vector3 v0 = new Vector3(C.X - A.X, C.Y - A.Y, C.Z - A.Z);
            Vector3 v1 = new Vector3(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
            Vector3 v2 = new Vector3(P.X - A.X, P.Y - A.Y, P.Z - A.Z);

            //Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            //Compute barycentric coordinates
            float invDenom = 1.0f/((dot00 * dot11) - (dot01 * dot01));
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            //Check if point is in triangle
            float tolerance = 0.00001f;
            return (u+tolerance >= 0) && (v+tolerance >= 0) &&
                   ((u-tolerance + v-tolerance) <= 1);
        }

        private bool RayIntersectsTriangle(Ray ray, Triangle tri, out float dist)
        {
            Vector3 normal = Vector3.Zero;
            CalcTriNormal(ref normal,
                          tri.Points(0), tri.Points(1), tri.Points(2));
            //calculate the plane equation for this triangle to see if the ray intersect

            float planeD = CalcPlaneD(tri.Points(0), normal);
            //these are two sided polys so we don't need to check
            //that ray starts on correct side of polygon
            //float dot_prod = dot(rayDir, normal);
            //if(dot_prod >= 0)
            //    return false;

            dist = DistRayPlane(ray.Position, 
                                ray.Direction, 
                                normal, 
                                planeD);
            if(dist < 0.0f)
                return false;//ray is in front of poly

            Vector3 primIsect = ray.Position + (ray.Direction * dist);

            return TriContainsPoint(tri.Points(0), tri.Points(1), tri.Points(2), primIsect);
        }

        public bool FoundIntersection() 
        { 
            return _isectFound; 
        }

        public Vector3 GetIntersection()
        {
            return _isectPt;
        }

        public float GetIntersectionDistance()
        {
            return _isectDist;
        }

        public Triangle GetIntersectionTriangle()
        {
            return _isectTri;
        }

        #endregion
    }
}
