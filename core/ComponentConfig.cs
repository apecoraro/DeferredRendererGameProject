using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    public class ComponentConfig
    {
        private Dictionary<String, String> _paramValues;
        private String _name;
        public GameObjectConfig GameObjectConfig;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ComponentConfig(String name)
        {
            _name = name;
            _paramValues = new Dictionary<String, String>();
        }

        public void SetParameter(String param, String value)
        {
            _paramValues[param] = value;
        }

        public String GetParameterValue(String param)
        {
            string paramValue;
            //game object's params take precendence
            if (GameObjectConfig.ParamValues != null &&
                GameObjectConfig.ParamValues.TryGetValue(param, out paramValue))
                return paramValue;

            if(_paramValues.TryGetValue(param, out paramValue))
                return paramValue;

            return null;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of strings
        /// Array values are specified as comma delimitted values 
        /// "string1,string2,string3" = {"string1", "string2", "string3" }
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public string[] GetArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            return parts;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of bools
        /// Array values are specified as comma delimitted values 
        /// "Yes,true,No,false" = {true, true, false, false }
        /// String values of yes, true, and on (not case sensitive) are interpreted as true
        /// all other values are interpreted as false
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public bool[] GetBoolArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName).ToUpper();
            if (strValue == null)
                return null;

            string[] parts = strValue.Split(',');

            bool[] valueParts = new bool[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                valueParts[i] = (parts[i] == "YES" || parts[i] == "TRUE" || parts[i] == "ON");
            }

            return valueParts;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of short ints
        /// Array values are specified as comma delimitted values 
        /// "255,0,255" = {255, 0, 255}
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public short[] GetShortArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;

            string[] parts = strValue.Split(',');

            short[] valueParts = new short[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                valueParts[i] = short.Parse(parts[i]);

            return valueParts;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of ints
        /// Array values are specified as comma delimitted values 
        /// "255,0,255" = {255, 0, 255}
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public int[] GetIntArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            int[] valueParts = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                valueParts[i] = int.Parse(parts[i]);

            return valueParts;
        }

        public Color[] GetColorArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            Color[] colors = new Color[(int)(parts.Length/3.0f)];

            for (int i = 0; i < colors.Length; i++)
            {
                int partIndex = i * 3;
                colors[i] = new Color(float.Parse(parts[partIndex]), 
                                      float.Parse(parts[partIndex+1]), 
                                      float.Parse(parts[partIndex+2]));
            }
            return colors;
        }

        public Vector3[] GetVector3ArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            Vector3[] vecs = new Vector3[(int)(parts.Length / 3.0f)];

            for (int i = 0; i < vecs.Length; i++)
            {
                int partIndex = i * 3;
                vecs[i] = new Vector3(float.Parse(parts[partIndex]),
                                      float.Parse(parts[partIndex + 1]),
                                      float.Parse(parts[partIndex + 2]));
            }

            return vecs;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of floats
        /// Array values are specified as comma delimitted values 
        /// "255.0,0.0,255.0" = {255.0f, 0.0f, 255.0f}
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public float[] GetFloatArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            float[] valueParts = new float[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                valueParts[i] = float.Parse(parts[i]);

            return valueParts;
        }

        /// <summary>
        /// Gets the value of the specified configuration parameter and returns as array of doubles
        /// Array values are specified as comma delimitted values 
        /// "255.0,0.0,255.0" = {255.0f, 0.0f, 255.0f}
        /// </summary>
        /// <param name="paramName">name of param</param>
        /// <returns>value of parameter</returns>
        public double[] GetDoubleArrayParameterValue(string paramName)
        {
            string strValue = GetParameterValue(paramName);
            if (strValue == null)
                return null;
            string[] parts = strValue.Split(',');

            double[] valueParts = new double[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                valueParts[i] = double.Parse(parts[i]);

            return valueParts;
        }

        public Object GetEnumParameterValue(string paramName, Type type)
        {
            string strValue = GetParameterValue(paramName);
            return Enum.Parse(type, strValue);
        }
    }
}
