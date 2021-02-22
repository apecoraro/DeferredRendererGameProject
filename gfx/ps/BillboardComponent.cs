using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx.ps
{
    public class BillboardComponent : core.Component, DeskWars.com.MessageHandler
    {
        Billboard _billboard;
        public bool Enable = false;
        bool _isDisplayed = false;

        public BillboardComponent(Billboard billboard)
        {
            _billboard = billboard;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(gfx.com.MsgBillboard), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(gfx.com.MsgBillboard), this);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            ps.ParticleSystemManager.instance().AddParticleSystem(_billboard);
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            ps.ParticleSystemManager.instance().RemoveParticleSystem(_billboard);
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
            _billboard.Emitter.PositionData.Position = gameObject.Position;
            _billboard.Emitter.OrientationData.Up = Vector3.Up;
            if (Enable && !_isDisplayed)
            {
                _billboard.RemoveAllParticles();
                _billboard.BillboardParticles.Clear();
                _billboard.Emitter.BurstParticles = _billboard.Billboards.Count;
                _isDisplayed = true;
            }
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            gfx.com.MsgBillboard bbMsg = (gfx.com.MsgBillboard)msg;
            _billboard.Billboards = bbMsg.Billboards;
            if (!bbMsg.Enable && _isDisplayed)
            {
                _billboard.RemoveAllParticles();
                _isDisplayed = false;
            }
            Enable = bbMsg.Enable;
        }

        #endregion
    }
}
