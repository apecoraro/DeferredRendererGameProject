using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public class GameObjectConfig
    {
        private List<ComponentConfig> _componentConfigs;
        private String _name;

        public Dictionary<String, String> ParamValues;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public GameObjectConfig(String name)
        {
            _name = name;
            _componentConfigs = new List<ComponentConfig>();
        }

        public void SetParameter(String param, String value)
        {
            if (ParamValues == null)
                ParamValues = new Dictionary<string, string>();

            ParamValues[param] = value;
        }

        public void AddComponentConfig(ComponentConfig compCfg)
        {
            compCfg.GameObjectConfig = this;
            _componentConfigs.Add(compCfg);
        }

        public ComponentConfig GetComponentConfig(int index)
        {
            return _componentConfigs[index];
        }

        public int GetComponentConfigCount()
        {
            return _componentConfigs.Count;
        }
    }
}
