using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Virtual action button for mobile (attack, interact, skill, etc.)
    /// </summary>
    public class VirtualButton : GameControl
    {
        // Fields
        private int _touchId = -1;
        private bool _isPressed;
        private float _pressTime;
        
        // Configuration
        public float Radius { get; set; } = 40f;
        public Color BaseColor { get; set; } = Color.White * 0.3f;
        public Color PressedColor { get; set; } = Color.White * 0.7f;
        public Color IconColor { get; set; } = Color.White;
        public string Label { get; set; } = "A";
        
        // State
        public bool IsPressed => _isPressed;
        public float PressDuration => _pressTime;
        
        // Events
        public event EventHandler Pressed;
        public event EventHandler Released;
        public event EventHandler<LongPressEventArgs> LongPress;
        
        public float LongPressThreshold { get; set; } = 0.5f; // seconds
        
        public VirtualButton()
        {
            Interactive = true;
            CapturePointerWhenNonInteractive = true;
        }
        
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
            ViewSize = new Point((int)(Radius * 2), (int)(Radius * 2));
        }
        
        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                if (_isPressed)
                {
                    _isPressed = false;
                    _touchId = -1;
                    _pressTime = 0f;
                    Released?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
            
            var touchState = MuGame.Instance.Touch;
            bool stillPressed = false;
            
            // Check for active touch on this button
            for (int i = 0; i < touchState.Count; i++)
            {
                var touch = touchState[i];
                
                // Find our touch or acquire new one
                if (_touchId == -1 || touch.Id == _touchId)
                {
                    var touchPos = touch.Position;
                    
                    if (_touchId == -1)
                    {
                        // Try to acquire touch if it's within the button
                        Rectangle buttonRect = new Rectangle(
                            DisplayPosition.X,
                            DisplayPosition.Y,
                            DisplaySize.X,
                            DisplaySize.Y);
                        
                        // Expand hit area slightly for better usability
                        Rectangle expandedRect = new Rectangle(
                            buttonRect.X - 10,
                            buttonRect.Y - 10,
                            buttonRect.Width + 20,
                            buttonRect.Height + 20);
                        
                        if (expandedRect.Contains((int)touchPos.X, (int)touchPos.Y))
                        {
                            _touchId = touch.Id;
                            _isPressed = true;
                            _pressTime = 0f;
                            Pressed?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else if (_touchId == touch.Id)
                    {
                        if (touch.State == TouchLocationState.Released)
                        {
                            _isPressed = false;
                            _touchId = -1;
                            _pressTime = 0f;
                            Released?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            stillPressed = true;
                            _pressTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            
                            // Check for long press
                            if (_pressTime >= LongPressThreshold && _pressTime - (float)gameTime.ElapsedGameTime.TotalSeconds < LongPressThreshold)
                            {
                                LongPress?.Invoke(this, new LongPressEventArgs { Duration = _pressTime });
                            }
                        }
                    }
                }
            }
            
            // If we had a touch but it's gone, reset
            if (_touchId != -1 && !stillPressed)
            {
                bool found = false;
                for (int i = 0; i < touchState.Count; i++)
                {
                    if (touchState[i].Id == _touchId && touchState[i].State != TouchLocationState.Released)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    _isPressed = false;
                    _touchId = -1;
                    _pressTime = 0f;
                    Released?.Invoke(this, EventArgs.Empty);
                }
            }
            
            base.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;
            
            SpriteBatch spriteBatch = GraphicsManager.Instance.Sprite;
            
            // Draw button circle
            Color buttonColor = _isPressed ? PressedColor : BaseColor;
            
            // Draw outer circle
            spriteBatch.Draw(
                GraphicsManager.Instance.Pixel,
                new Rectangle(
                    DisplayPosition.X,
                    DisplayPosition.Y,
                    DisplaySize.X,
                    DisplaySize.Y),
                null,
                buttonColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
            
            // Draw inner circle for visual feedback
            float innerRadius = Radius * 0.7f;
            int innerSize = (int)(innerRadius * 2);
            int innerX = DisplayPosition.X + (DisplaySize.X - innerSize) / 2;
            int innerY = DisplayPosition.Y + (DisplaySize.Y - innerSize) / 2;
            
            spriteBatch.Draw(
                GraphicsManager.Instance.Pixel,
                new Rectangle(innerX, innerY, innerSize, innerSize),
                null,
                buttonColor * 1.5f,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
            
            // Draw label text if present
            if (!string.IsNullOrEmpty(Label))
            {
                Vector2 textSize = GraphicsManager.Instance.Font.MeasureString(Label);
                Vector2 textPos = new Vector2(
                    DisplayPosition.X + (DisplaySize.X - textSize.X) / 2,
                    DisplayPosition.Y + (DisplaySize.Y - textSize.Y) / 2);
                
                spriteBatch.DrawString(
                    GraphicsManager.Instance.Font,
                    Label,
                    textPos,
                    IconColor);
            }
            
            base.Draw(gameTime);
        }
    }
    
    public class LongPressEventArgs : EventArgs
    {
        public float Duration { get; set; }
    }
}
