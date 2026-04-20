using Client.Main.Controls.UI.Common;
using Client.Main.Core.Client;
using Client.Main.Helpers.MuHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Client.Main.Controls.UI.Game.Hud
{
    public class MuHelperControl : UIControl
    {
        private readonly MuHelper _muHelper;
        private SpriteFont _font;
        private bool _isCollapsed;
        private Rectangle _mainRect;
        private Rectangle _toggleButtonRect;
        private Rectangle _settingsButtonRect;
        private bool _showSettings;
        
        // Settings UI rects
        private Rectangle _settingsPanelRect;
        private Rectangle[] _optionRects;
        private Rectangle[] _checkboxRects;
        
        private const int PanelWidth = 250;
        private const int PanelHeight = 300;
        private const int ButtonWidth = 80;
        private const int ButtonHeight = 25;

        public MuHelperControl(MuHelper muHelper)
        {
            _muHelper = muHelper ?? throw new ArgumentNullException(nameof(muHelper));
            _isCollapsed = false;
            _showSettings = false;
        }

        public override async Task LoadContent()
        {
            await base.LoadContent();
            _font = MuGame.Instance.Content.Load<SpriteFont>("Fonts/Default");
            CalculateLayout();
        }

        private void CalculateLayout()
        {
            var position = new Vector2(
                MuGame.Instance.Width - PanelWidth - 20,
                MuGame.Instance.Height / 2 - PanelHeight / 2
            );
            
            _mainRect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                PanelWidth,
                _isCollapsed ? 40 : PanelHeight
            );
            
            _toggleButtonRect = new Rectangle(
                _mainRect.X + 10,
                _mainRect.Y + 8,
                ButtonWidth,
                ButtonHeight
            );
            
            _settingsButtonRect = new Rectangle(
                _toggleButtonRect.Right + 10,
                _mainRect.Y + 8,
                ButtonWidth,
                ButtonHeight
            );
            
            // Settings panel layout
            if (_showSettings)
            {
                _settingsPanelRect = new Rectangle(
                    _mainRect.X,
                    _mainRect.Bottom + 5,
                    PanelWidth,
                    200
                );
                
                _optionRects = new Rectangle[6];
                _checkboxRects = new Rectangle[6];
                
                int y = _settingsPanelRect.Y + 10;
                int optionIndex = 0;
                
                // Auto Attack
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
                y += 25;
                optionIndex++;
                
                // Auto Potion HP
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
                y += 25;
                optionIndex++;
                
                // Auto Potion MP
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
                y += 25;
                optionIndex++;
                
                // Auto Buff
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
                y += 25;
                optionIndex++;
                
                // Return to Town on Low HP
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
                y += 25;
                optionIndex++;
                
                // Pick Items
                _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 30, y, 200, 20);
                _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 10, y + 2, 16, 16);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update helper state
            _muHelper?.Update(gameTime);
            
            // Handle mouse input
            var mouseState = MuGame.Instance.Mouse;
            var prevMouseState = MuGame.Instance.PrevMouseState;
            
            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                var mousePos = new Point(mouseState.X, mouseState.Y);
                
                if (_toggleButtonRect.Contains(mousePos))
                {
                    _muHelper?.Toggle();
                }
                
                if (_settingsButtonRect.Contains(mousePos))
                {
                    _showSettings = !_showSettings;
                    CalculateLayout();
                }
                
                // Handle settings clicks
                if (_showSettings && _checkboxRects != null)
                {
                    for (int i = 0; i < _checkboxRects.Length; i++)
                    {
                        if (_checkboxRects[i].Contains(mousePos))
                        {
                            ToggleOption(i);
                        }
                    }
                }
            }
        }

        private void ToggleOption(int index)
        {
            var settings = _muHelper.Settings;
            
            switch (index)
            {
                case 0: // Auto Attack
                    settings.AutoAttack = !settings.AutoAttack;
                    break;
                case 1: // Auto Potion HP
                    settings.AutoPotionHP = !settings.AutoPotionHP;
                    break;
                case 2: // Auto Potion MP
                    settings.AutoPotionMP = !settings.AutoPotionMP;
                    break;
                case 3: // Auto Buff
                    settings.AutoBuff = !settings.AutoBuff;
                    break;
                case 4: // Return to Town on Low HP
                    settings.ReturnToTownOnLowHP = !settings.ReturnToTownOnLowHP;
                    break;
                case 5: // Pick Items
                    settings.PickItems = !settings.PickItems;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw main panel background
            var panelColor = _muHelper.IsActive ? 
                new Color(0, 100, 0, 200) : 
                new Color(100, 100, 100, 200);
            
            spriteBatch.Draw(TextureHelper.WhitePixel, _mainRect, panelColor);
            
            // Draw border
            DrawBorder(spriteBatch, _mainRect, Color.White);
            
            // Draw toggle button
            var toggleColor = _muHelper.IsActive ? 
                new Color(0, 200, 0) : 
                new Color(200, 0, 0);
            spriteBatch.Draw(TextureHelper.WhitePixel, _toggleButtonRect, toggleColor);
            
            // Draw settings button
            var settingsColor = _showSettings ? 
                new Color(100, 100, 255) : 
                new Color(100, 100, 100);
            spriteBatch.Draw(TextureHelper.WhitePixel, _settingsButtonRect, settingsColor);
            
            // Draw labels
            string statusText = _muHelper.IsActive ? "ON" : "OFF";
            spriteBatch.DrawString(_font, statusText, 
                new Vector2(_toggleButtonRect.X + 30, _toggleButtonRect.Y + 4), 
                Color.White);
            
            spriteBatch.DrawString(_font, "Settings", 
                new Vector2(_settingsButtonRect.X + 10, _settingsButtonRect.Y + 4), 
                Color.White);
            
            // Draw state info
            if (!_isCollapsed)
            {
                int infoY = _mainRect.Y + 45;
                spriteBatch.DrawString(_font, $"State: {_muHelper.CurrentState}", 
                    new Vector2(_mainRect.X + 10, infoY), 
                    Color.Yellow);
                
                infoY += 20;
                if (_muHelper.CurrentTarget != null)
                {
                    spriteBatch.DrawString(_font, $"Target: {_muHelper.CurrentTarget.GetType().Name}", 
                        new Vector2(_mainRect.X + 10, infoY), 
                        Color.Orange);
                }
            }
            
            // Draw settings panel
            if (_showSettings && _settingsPanelRect != Rectangle.Empty)
            {
                spriteBatch.Draw(TextureHelper.WhitePixel, _settingsPanelRect, new Color(50, 50, 50, 230));
                DrawBorder(spriteBatch, _settingsPanelRect, Color.Gray);
                
                var settings = _muHelper.Settings;
                string[] optionLabels = new[]
                {
                    "Auto Attack",
                    "Auto Potion (HP)",
                    "Auto Potion (MP)",
                    "Auto Buff",
                    "Return on Low HP",
                    "Pick Items"
                };
                
                bool[] optionValues = new[]
                {
                    settings.AutoAttack,
                    settings.AutoPotionHP,
                    settings.AutoPotionMP,
                    settings.AutoBuff,
                    settings.ReturnToTownOnLowHP,
                    settings.PickItems
                };
                
                for (int i = 0; i < optionLabels.Length && i < _optionRects.Length; i++)
                {
                    // Draw checkbox
                    var checkboxColor = optionValues[i] ? Color.Green : Color.Red;
                    spriteBatch.Draw(TextureHelper.WhitePixel, _checkboxRects[i], checkboxColor);
                    
                    // Draw label
                    spriteBatch.DrawString(_font, optionLabels[i], 
                        new Vector2(_optionRects[i].X, _optionRects[i].Y), 
                        Color.White);
                }
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            // Top
            spriteBatch.Draw(TextureHelper.WhitePixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            // Bottom
            spriteBatch.Draw(TextureHelper.WhitePixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
            // Left
            spriteBatch.Draw(TextureHelper.WhitePixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            // Right
            spriteBatch.Draw(TextureHelper.WhitePixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
        }

        public void SetPosition(Vector2 position)
        {
            // Recalculate layout with new position
            CalculateLayout();
        }
    }
}
