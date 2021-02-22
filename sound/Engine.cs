using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace DeskWars.sound
{
    class Engine : core.Engine
    {
        Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();

        #region Engine Members

        public void Configure(string configFile)
        {
        }

        public void LoadContent(DeskWars.core.Game game)
        {
        }

        public void CreateGameObjectComponents(DeskWars.core.GameObjectConfig config, DeskWars.core.GameObject gameObject)
        {
            for(int i = 0; i <  config.GetComponentConfigCount(); ++i)
            {
                core.ComponentConfig compCfg = config.GetComponentConfig(i);
                core.Component comp = null;
                switch (compCfg.Name)
                {
                    case "SoundComponent":
                        {
                            comp = new SoundComponent();
                        }
                        break;
                    case "WidgetSoundEffectComponent":
                        {
                            comp = new WidgetSoundEffectComponent(compCfg);
                        }
                        break;
                }

                if(comp != null)
                {
                    gameObject.AddComponent(comp);
                    SoundComponent soundComp = comp as SoundComponent;
                    if (soundComp != null)
                    {
                        ConfigureSoundEffects(soundComp, compCfg);
                    }
                    
                }
            }
        }

        protected void ConfigureSoundEffects(SoundComponent soundComp, core.ComponentConfig compCfg)
        {
            string[] soundNames = compCfg.GetArrayParameterValue("Sound Names");
            string[] soundFiles = compCfg.GetArrayParameterValue("Sound Files");

            if (soundNames != null && soundFiles != null &&
                soundNames.Length == soundFiles.Length)
            {
                for (int i = 0; i < soundNames.Length; ++i)
                {
                    string soundName = soundNames[i];
                    string soundFile = soundFiles[i];

                    SoundEffectInstance soundEffectInstance = GetOrCreateSoundEffect(soundFile);

                    soundComp.AddSoundEffect(soundName, soundEffectInstance);
                }
            }
            else
            {
                throw new SystemException("Bad Sound Names or Sound Files parameter, they must be the same length");
            }
        }


        protected SoundEffectInstance GetOrCreateSoundEffect(string soundFile)
        {
            SoundEffect soundEffect = null;
            if (_soundEffects.ContainsKey(soundFile))
            {
                soundEffect = _soundEffects[soundFile];
            }
            else
            {
                soundEffect = core.Game.instance().Content.Load<SoundEffect>(soundFile);
            }

            return soundEffect.CreateInstance();
        }

        public void OnGameObjectDeleted(DeskWars.core.GameObject gameObject)
        {
        }

        public void Update(DeskWars.core.GameObjectDB gameObjectDB, Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        #endregion

        #region Engine Members

        public void OnLoadLevel(int level)
        {
        }

        public void OnUnloadLevel(int level)
        {
        }

        #endregion
    }
}
