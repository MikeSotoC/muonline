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
        
        private const int PanelWidth = 260;
        private const int PanelHeight = 320;
        private const int ButtonWidth = 90;
        private const int ButtonHeight = 32;
        private const int CornerRadius = 8;

        // Modern color scheme
        private static readonly Color PanelBgColor = new Color(20, 24, 32, 220);
        private static readonly Color PanelBorderColor = new Color(60, 70, 90, 200);
        private static readonly Color AccentColor = new Color(0, 198, 255);
        private static readonly Color AccentHoverColor = new Color(0, 220, 255);
        private static readonly Color SuccessColor = new Color(0, 200, 83);
        private static readonly Color DangerColor = new Color(255, 82, 82);
        private static readonly Color TextPrimaryColor = Color.White;
        private static readonly Color TextSecondaryColor = new Color(180, 180, 180);
        private static readonly Color CheckboxUncheckedColor = new Color(50, 55, 65);
        private static readonly Color CheckboxCheckedColor = AccentColor;

        private bool _isToggleButtonHovered;
        private bool _isSettingsButtonHovered;
        private int _hoveredOptionIndex = -1;

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
            
            // Initialize modern renderer
            ModernUiRenderer.Initialize(MuGame.Instance.GraphicsDevice);
            
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
                _isCollapsed ? 50 : PanelHeight
            );
            
            _toggleButtonRect = new Rectangle(
                _mainRect.X + 10,
                _mainRect.Y + 10,
                ButtonWidth,
                ButtonHeight
            );
            
            _settingsButtonRect = new Rectangle(
                _toggleButtonRect.Right + 10,
                _mainRect.Y + 10,
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
                    220
                );
                
                _optionRects = new Rectangle[6];
                _checkboxRects = new Rectangle[6];
                
                int y = _settingsPanelRect.Y + 15;
                int optionIndex = 0;
                
                string[] optionLabels = new[]
                {
                    "Auto Attack",
                    "Auto Potion (HP)",
                    "Auto Potion (MP)",
                    "Auto Buff",
                    "Return on Low HP",
                    "Pick Items"
                };
                
                foreach (var label in optionLabels)
                {
                    _optionRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 35, y, 210, 28);
                    _checkboxRects[optionIndex] = new Rectangle(_settingsPanelRect.X + 12, y + 4, 20, 20);
                    y += 35;
                    optionIndex++;
                }
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
            var mousePos = new Point(mouseState.X, mouseState.Y);
            
            // Update hover states
            _isToggleButtonHovered = _toggleButtonRect.Contains(mousePos);
            _isSettingsButtonHovered = _settingsButtonRect.Contains(mousePos);
            _hoveredOptionIndex = -1;
            
            if (_showSettings && _checkboxRects != null)
            {
                for (int i = 0; i < _checkboxRects.Length; i++)
                {
                    if (_optionRects[i].Contains(mousePos))
                    {
                        _hoveredOptionIndex = i;
                        break;
                    }
                }
            }
            
            // Handle clicks
            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                if (_toggleButtonRect.Contains(mousePos))
                {
                    _muHelper?.Toggle();
                    SoundController.Instance.PlayBuffer("Sound/iButtonClick.wav");
                }
                
                if (_settingsButtonRect.Contains(mousePos))
                {
                    _showSettings = !_showSettings;
                    CalculateLayout();
                    SoundController.Instance.PlayBuffer("Sound/iButtonClick.wav");
                }
                
                // Handle settings clicks
                if (_showSettings && _checkboxRects != null)
                {
                    for (int i = 0; i < _checkboxRects.Length; i++)
                    {
                        if (_checkboxRects[i].Contains(mousePos))
                        {
                            ToggleOption(i);
                            SoundController.Instance.PlayBuffer("Sound/iButtonClick.wav");
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
            // Draw main panel with modern glass effect
            ModernUiRenderer.DrawGlassPanel(
                spriteBatch,
                _mainRect,
                CornerRadius,
                PanelBgColor,
                borderColor: PanelBorderColor,
                withShadow: true);

            // Draw title bar accent
            var titleBarRect = new Rectangle(_mainRect.X + CornerRadius, _mainRect.Y + CornerRadius, 
                _mainRect.Width - 2 * CornerRadius, 3);
            spriteBatch.Draw(ModernUiRenderer.WhitePixel, titleBarRect, AccentColor);

            // Draw toggle button with modern style
            Color toggleNormalColor = _muHelper.IsActive ? SuccessColor : DangerColor;
            Color toggleHoverColor = _muHelper.IsActive ? 
                new Color(0, 230, 100) : new Color(255, 100, 100);
            Color togglePressedColor = _muHelper.IsActive ? 
                new Color(0, 180, 70) : new Color(200, 60, 60);
            
            ModernUiRenderer.DrawModernButton(
                spriteBatch,
                _toggleButtonRect,
                CornerRadius / 2,
                toggleNormalColor,
                toggleHoverColor,
                togglePressedColor,
                _isToggleButtonHovered,
                false,
                _muHelper.IsActive ? "ON" : "OFF",
                _font,
                TextPrimaryColor);

            // Draw settings button
            Color settingsNormalColor = PanelBorderColor;
            Color settingsHoverColor = AccentColor;
            
            ModernUiRenderer.DrawModernButton(
                spriteBatch,
                _settingsButtonRect,
                CornerRadius / 2,
                settingsNormalColor,
                settingsHoverColor,
                AccentColor,
                _isSettingsButtonHovered,
                false,
                "⚙️ Settings",
                _font,
                TextPrimaryColor);

            // Draw state info (only when expanded)
            if (!_isCollapsed)
            {
                int infoY = _mainRect.Y + 55;
                
                // State label
                var stateText = $"State: {_muHelper.CurrentState}";
                spriteBatch.DrawString(_font, stateText,
                    new Vector2(_mainRect.X + 15, infoY),
                    TextSecondaryColor);
                
                infoY += 25;
                
                // Target info
                if (_muHelper.CurrentTarget != null)
                {
                    var targetText = $"Target: {_muHelper.CurrentTarget.GetType().Name}";
                    spriteBatch.DrawString(_font, targetText,
                        new Vector2(_mainRect.X + 15, infoY),
                        AccentColor);
                }
                
                // Status indicator dot
                int dotX = _mainRect.Right - 25;
                int dotY = _mainRect.Y + 15;
                int dotSize = 8;
                Color dotColor = _muHelper.IsActive ? SuccessColor : Color.Gray;
                spriteBatch.Draw(ModernUiRenderer.WhitePixel,
                    new Rectangle(dotX, dotY, dotSize, dotSize),
                    dotColor);
            }

            // Draw settings panel with glass effect
            if (_showSettings && _settingsPanelRect != Rectangle.Empty)
            {
                ModernUiRenderer.DrawGlassPanel(
                    spriteBatch,
                    _settingsPanelRect,
                    CornerRadius,
                    PanelBgColor,
                    borderColor: AccentColor * 0.5f,
                    withShadow: true);

                // Draw header for settings panel
                var headerText = "⚙️ Helper Settings";
                var headerPos = new Vector2(_settingsPanelRect.X + 15, _settingsPanelRect.Y + 10);
                spriteBatch.DrawString(_font, headerText, headerPos, AccentColor);

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
                    // Highlight hovered option
                    if (i == _hoveredOptionIndex)
                    {
                        var hoverRect = _optionRects[i];
                        hoverRect.Inflate(4, 2);
                        spriteBatch.Draw(ModernUiRenderer.WhitePixel, hoverRect, 
                            new Color(AccentColor.R, AccentColor.G, AccentColor.B, 30));
                    }

                    // Draw checkbox with modern style
                    ModernUiRenderer.DrawCheckbox(
                        spriteBatch,
                        _checkboxRects[i],
                        4,
                        optionValues[i],
                        CheckboxUncheckedColor,
                        CheckboxCheckedColor,
                        Color.Black);

                    // Draw label
                    Color labelColor = (i == _hoveredOptionIndex) ? 
                        TextPrimaryColor : TextSecondaryColor;
                    
                    spriteBatch.DrawString(_font, optionLabels[i],
                        new Vector2(_optionRects[i].X, _optionRects[i].Y + 4),
                        labelColor);
                }
            }
        }

        public void SetPosition(Vector2 position)
        {
            // Recalculate layout with new position
            CalculateLayout();
        }
    }
}
