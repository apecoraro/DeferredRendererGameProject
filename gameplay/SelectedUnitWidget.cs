using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    class SelectedUnitWidget : gui.TextureWidget, com.MessageHandler
    {
        gui.TextWidget _unitNameText = null;
        gui.TextWidget _unitHealthText = null;
        gui.TextWidget _unitStaminaText = null;
        gui.TextWidget _unitAttackText = null;
        gui.TextWidget _unitDefenseText = null;
        gui.TextWidget _unitSpeedText = null;

        UnitComponent _selectedUnit = null;
        Dictionary<string, Texture2D> _iconTextures = null;
        gui.TextureWidget _iconWidget = null;

        public SelectedUnitWidget(SpriteFont font, 
                                  Color textColor,
                                  Texture2D texture,
                                  Dictionary<string, Texture2D> iconTextures,
                                  core.ComponentConfig config) :
            base(texture, config)
        {
            _iconTextures = iconTextures;
            _iconWidget = new DeskWars.gui.TextureWidget("IconTexture", 0.16f, 0.16f, -1.0f, texture, Color.White);
            _iconWidget.SetIsActive(false, false);
            _iconWidget.SetOffsetTranslation(0.02f, 0.02f);
            this.AddChild(_iconWidget);

            float xoffset = 0.2f;
            float yoffset = 0.02f;
            float yincr = 0.06f;
            float scale = 0.85f;

            _unitNameText = new gui.TextWidget("UnitText", font, "Unit      : None", Color.Red, scale);
            _unitNameText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitNameText);

            yoffset += yincr;

            _unitHealthText = new gui.TextWidget("HealthText", font, "Health   : ", textColor, scale);
            _unitHealthText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitHealthText);

            yoffset += yincr;

            _unitStaminaText = new gui.TextWidget("StaminaText", font, "Stamina : ", textColor, scale);
            _unitStaminaText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitStaminaText);

            xoffset += 0.3f;
            yoffset = 0.02f;

            _unitAttackText = new gui.TextWidget("AttackText", font, "Attack    : ", textColor, scale);
            _unitAttackText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitAttackText);

            yoffset += yincr;

            _unitDefenseText = new gui.TextWidget("DefenseText", font, "Defense : ", textColor, scale);
            _unitDefenseText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitDefenseText);

            yoffset += yincr;

            _unitSpeedText = new gui.TextWidget("SpeedText", font, "Speed    : ", textColor, scale);
            _unitSpeedText.SetOffsetTranslation(xoffset, yoffset);
            this.AddChild(_unitSpeedText);
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.RegisterForMessages(typeof(com.MsgUnitSelected), this);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().PostOffice.UnRegisterForMessages(typeof(com.MsgUnitSelected), this);
            base.OnRemoveFromGameObject(gameObject);
        }
        
        void SetUnitNameText(string name)
        {
            _unitNameText.Text = "Unit      : " + name;
            
        }

        string CreateStarsText(int num)
        {
            if (num > 10)
                num = 10;

            string text = "";
            for(int i = 0; i < num; ++i)
            {
                text += "*";
            }

            return text;
        }

        void SetHealthText(int health)
        {
            //max is 100
            string starsTxt = "";
            if(health != 0)
                starsTxt = CreateStarsText((health / 10) + 1);

            _unitHealthText.Text = "Health   : " + starsTxt;
        }

        void SetStaminaText(int stamina)
        {
            //max 100
            string stars = "";
            if (stamina != 0)
                stars = CreateStarsText((stamina / 10) + 1);

            _unitStaminaText.Text = "Stamina : " + stars;
        }

        void SetAttackRangeText(float attackRange)
        {
            int attackStars = (int)((attackRange / 500.0f) * 10.0f);
            if(attackStars == 0)
                attackStars = 0;
            else if(attackStars > 10)
                attackStars = 10;

            string stars = CreateStarsText(attackStars);

            _unitAttackText.Text = "Attack    : " + stars;
        }
        
        void SetDefensePowerText(int defPwr)
        {
            string stars = "";
            if (defPwr != 0)
                stars = CreateStarsText((defPwr / 10) + 1);

            _unitDefenseText.Text = "Defense : " + stars;
        }

        void SetSpeedText(float speed)
        {
            int numStars = (int)((speed / 0.1f) * 10.0f);
            if (numStars == 0)
                numStars = 0;
            else if (numStars > 10)
                numStars = 10;

            string stars = CreateStarsText(numStars);

            _unitSpeedText.Text = "Speed    : " + stars;
        }

        void SetUnitIcon(string className)
        {
            Texture2D iconTexture;
            if (_iconTextures.TryGetValue(className, out iconTexture))
            {
                _iconWidget.Texture = iconTexture;
                _iconWidget.SetIsActive(true, false);
            }
        }

        public override void Update(DeskWars.core.GameObject gameObject, 
                                    Microsoft.Xna.Framework.GameTime gameTime)
        {
 	        base.Update(gameObject, gameTime);
            if(_selectedUnit != null && 
               _selectedUnit.UnitStats.state != UnitComponent.State.State_Dead)
            {
                SetUnitIcon(_selectedUnit.UnitStats.Class);
                SetUnitNameText(_selectedUnit.UnitRatings.Name);
                SetHealthText(_selectedUnit.UnitStats.health);//100
                SetStaminaText(_selectedUnit.UnitStats.stamina);//100
                SetAttackRangeText(_selectedUnit.UnitRatings.AttackRange);//100
                SetDefensePowerText(_selectedUnit.UnitRatings.DefensePower);//100
                SetSpeedText(_selectedUnit.UnitRatings.TopSpeed);//100
            }
            else
            {
                SetUnitNameText("");
                SetHealthText(0);
                SetStaminaText(0);
                SetAttackRangeText(0.0f);
                SetDefensePowerText(0);
                SetSpeedText(0.0f);
            }
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            com.MsgUnitSelected selMsg = msg as com.MsgUnitSelected;
            _selectedUnit = selMsg.Unit;
        }

        #endregion
    }
}
