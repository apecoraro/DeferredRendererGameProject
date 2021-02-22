using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace DeskWars.gui
{
    class MouseIcon : gui.TextureWidget, com.MessageHandler
    {
        public MouseIcon(Texture2D texture, 
                         core.ComponentConfig config) : base(texture, config)
        {
            this.ReceiveEvents = false;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.RegisterForMessages(typeof(com.MsgUpdateMouseIcon), this); 
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.UnRegisterForMessages(typeof(com.MsgUpdateMouseIcon), this); 
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void Update(core.GameObject gameObject, GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            DestinationRectangle.X = mouseState.X -(int)(BoundingRect.Width * 0.5f);
            DestinationRectangle.Y = mouseState.Y -(int)(BoundingRect.Height * 0.5f);
            DestinationRectangle.Width = BoundingRect.Width;
            DestinationRectangle.Height = BoundingRect.Height;
            SourceRectangle = DestinationRectangle;
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            com.MsgUpdateMouseIcon iconMsg = (com.MsgUpdateMouseIcon)msg;
            this.Texture = iconMsg.Texture;
            //core.DebugService dbg = (core.DebugService)core.Game.instance().Services.GetService(typeof(core.DebugService));
            //dbg.AddDebugText("New Mouse Icon", 2.0f);
        }

        #endregion
    }
}
