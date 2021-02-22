using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class BaseEffectConfig
    {
        int _activeLightCount = 1;
        public int ActiveLightCount 
        { 
            get 
            {
                return Math.Min(_activeLightCount, _lights.Count);
            } 
            set 
            { 
                _activeLightCount = value; 
            } 
        }

        List<LightComponent> _lights = new List<LightComponent>();

        public List<LightComponent> Lights { get { return _lights; } }

        //int _currentLightIndex = -1;
        //public int CurrentLightIndex { get { return _currentLightIndex; } set { _currentLightIndex = value; } }

        Vector3 _eyePos = Vector3.Zero;
        public Vector3 EyePosition 
        { 
            get { return _eyePos; } 
            set { _eyePos = value; } 
        }

        Vector3 _ambientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 AmbientLightColor 
        { 
            get { return _ambientLightColor; } 
            set { _ambientLightColor = value; } 
        }

        public Matrix ViewMat;
        public Matrix ProjMat;
    }
}
