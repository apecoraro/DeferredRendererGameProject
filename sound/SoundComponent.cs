using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace DeskWars.sound
{
    public class SoundComponent : core.Component, DeskWars.com.MessageHandler
    {
        public class Request
        {
            public enum Action
            {
                START,
                STOP,
                PAUSE
            }
            public Action RequestedAction;
            public string SoundName;
        }

        Queue<Request> _requestQueue = new Queue<Request>();
        Dictionary<string, SoundEffectInstance> _soundEffects = new Dictionary<string, SoundEffectInstance>();

        public virtual void AddSoundEffect(string soundName, SoundEffectInstance soundEffectInstance)
        {
            _soundEffects[soundName] = soundEffectInstance;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
            gameObject.PostOffice.RegisterForMessages(typeof(sound.com.MsgSoundEffectRequest), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
            gameObject.PostOffice.UnRegisterForMessages(typeof(sound.com.MsgSoundEffectRequest), this);
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (_requestQueue.Count == 0)
                return;

            Request request = _requestQueue.Dequeue();
            if (_soundEffects.ContainsKey(request.SoundName))
            {
                SoundEffectInstance seInst = _soundEffects[request.SoundName];
                if (request.RequestedAction == Request.Action.STOP)
                    seInst.Stop();
                else
                {

                    if (request.RequestedAction == Request.Action.START)
                    {
                        if (seInst.State != SoundState.Playing)
                            seInst.Play();
                    }
                    else// if(request.Action == Request.Action.PAUSE)
                        seInst.Pause();
                }
            }
        }

        protected void AddRequest(Request req)
        {
            _requestQueue.Enqueue(req);
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            sound.com.MsgSoundEffectRequest seMsg = (sound.com.MsgSoundEffectRequest)msg;
            AddRequest(seMsg.Request);
        }

        #endregion

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }
    }
}
