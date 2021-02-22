using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    class CommanderHealthBar : gui.Widget, com.MessageHandler
    {
        Texture2D _healthTexture;
        float _commanderHealth = 1.0f;
        Rectangle _rect = new Rectangle();
        
        public CommanderHealthBar(Texture2D healthTexture, core.ComponentConfig config) : base(config)
        {
            _healthTexture = healthTexture;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float upperLeftX = this.BoundingRect.X;
            float upperLeftY = this.BoundingRect.Y;
            float width = this.BoundingRect.Width;
            float height = this.BoundingRect.Height;

            
            _rect.X = (int)upperLeftX;
            _rect.Width = (int)width;
            _rect.Height = (int)(_commanderHealth * height);

            float maxY = upperLeftY + height;
            _rect.Y = (int)maxY - _rect.Height;

            spriteBatch.Draw(_healthTexture, 
                             _rect, 
                             null, 
                             Color.White, 
                             0.0f, 
                             Vector2.Zero,
                             SpriteEffects.None, 
                             this.Depth);
        }

        protected void UpdateCommanderHealth(int health)
        {
           _commanderHealth = (health * 0.01f);
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.RegisterForMessages(typeof(com.MsgCommanderAttacked), this);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.UnRegisterForMessages(typeof(com.MsgCommanderAttacked), this);
            base.OnRemoveFromGameObject(gameObject);
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            com.MsgCommanderAttacked attackMsg = msg as com.MsgCommanderAttacked;
            this.UpdateCommanderHealth(attackMsg.CommanderUnit.UnitStats.health);
        }

        #endregion
    }
}
