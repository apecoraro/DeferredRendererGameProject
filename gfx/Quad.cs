using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    class Quad
    {
        public enum EProjectionPlane
        {
            PlaneYZ,
            PlaneXZ,
            PlaneXY
        }
        
        public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            _width = width;
            _height = height;
            _vertices = new VertexPositionNormalTexture[4];
            _indices = new int[6];
            _origin = origin;
            _normal = normal;
            _up = up;

            // Calculate the quad corners
            _left = Vector3.Cross(_normal, _up);
            Vector3 uppercenter = (_up * height / 2) + _origin;
            _upperLeft = uppercenter + (_left * width / 2);
            _upperRight = uppercenter - (_left * width / 2);
            _lowerLeft = _upperLeft - (_up * height);
            _lowerRight = _upperRight - (_up * height);

            if (Math.Abs(_normal.X) >= Math.Abs(_normal.Y) && Math.Abs(_normal.X) >= Math.Abs(_normal.Z))
            {
                _projPlane = EProjectionPlane.PlaneYZ;
            }
            else if (Math.Abs(_normal.Y) >= Math.Abs(_normal.Z))
            {
                _projPlane = EProjectionPlane.PlaneXZ;
            }
            else
            {
                _projPlane = EProjectionPlane.PlaneXY;
            }

            FillVertices();
        }

        //----------------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        /// The center point for the quad.
        /// </summary>
        public Vector3 Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// The normal vector for the quad.
        /// </summary>
        public Vector3 Normal
        {
            get { return _normal; }
        }

        /// <summary>
        /// Upper left point.
        /// </summary>
        public Vector3 UpperLeft
        {
            get { return _upperLeft; }
        }

        /// <summary>
        /// Upper right point.
        /// </summary>
        public Vector3 UpperRight
        {
            get { return _upperRight; }
        }

        /// <summary>
        /// Lower left point.
        /// </summary>
        public Vector3 LowerLeft
        {
            get { return _lowerLeft; }
        }

        /// <summary>
        /// Lower right point.
        /// </summary>
        public Vector3 LowerRight
        {
            get { return _lowerRight; }
        }

        /// <summary>
        /// List of texture coordinate positions.
        /// </summary>
        public VertexPositionNormalTexture[] Vertices
        {
            get { return _vertices; }
        }

        /// <summary>
        /// List of vertex indices.
        /// </summary>
        public int[] Indices
        {
            get { return _indices; }
        }

        /// <summary>
        /// A list of the 4 corners of the Quad.
        /// </summary>
        public List<Vector3> Corners
        {
            get
            {
                List<Vector3> corners = new List<Vector3>();
                corners.Add(_upperLeft);
                corners.Add(_upperRight);
                corners.Add(_lowerLeft);
                corners.Add(_lowerRight);
                return corners;
            }
        }

        public float Width
        {
            get { return _width; }
        }

        public float Height
        {
            get { return _height; }
        }

        #endregion

        //----------------------------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// Set the texture on the quad
        /// </summary>
        private void FillVertices()
        {
            //Fill in texture coordinates to display full texture on quad
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

            // Provide a normal for each vertex
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Normal = _normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            _vertices[0].Position = _lowerLeft;
            _vertices[0].TextureCoordinate = textureLowerLeft;
            _vertices[1].Position = _upperLeft;
            _vertices[1].TextureCoordinate = textureUpperLeft;
            _vertices[2].Position = _lowerRight;
            _vertices[2].TextureCoordinate = textureLowerRight;
            _vertices[3].Position = _upperRight;
            _vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            _indices[0] = 0;
            _indices[1] = 1;
            _indices[2] = 2;
            _indices[3] = 2;
            _indices[4] = 1;
            _indices[5] = 3;
        }


        #endregion

        //----------------------------------------------------------------------------

        #region Private Members

        private float _width;
        private float _height;
        private Vector3 _origin;
        private Vector3 _upperLeft;
        private Vector3 _lowerLeft;
        private Vector3 _upperRight;
        private Vector3 _lowerRight;
        private Vector3 _normal;
        private Vector3 _up;
        private Vector3 _left;

        private VertexPositionNormalTexture[] _vertices;
        private int[] _indices;

        public EProjectionPlane _projPlane;

        #endregion

    }
}
