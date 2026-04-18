using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Virtual action button for mobile (attack, interact, skill, etc.)
    /// </summary>
    public class VirtualButton
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
        public Vector2 Position { get; set; }
        
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
        }
        
        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }
        
        public void Update(TouchCollection touchState, GameTime gameTime)
        {
            bool stillPressed = false;
            
            // Check for active touch on this button
            foreach (var touch in touchState)
            {
                // Find our touch or acquire new one
                if (_touchId == -1 || touch.Id == _touchId)
                {
                    var touchPos = touch.Position;
                    
                    if (_touchId == -1)
                    {
                        // Try to acquire touch if it's within the button
                        float distance = Vector2.Distance(touchPos, Position);
                        
                        // Expand hit area slightly for better usability
                        if (distance <= Radius + 10)
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
                foreach (var touch in touchState)
                {
                    if (touch.Id == _touchId && touch.State != TouchLocationState.Released)
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
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font)
        {
            // Draw button circle
            Color buttonColor = _isPressed ? PressedColor : BaseColor;
            
            // Draw outer circle
            DrawCircle(spriteBatch, Position, Radius, buttonColor, 32, pixelTexture);
            
            // Draw inner circle for visual feedback
            float innerRadius = Radius * 0.7f;
            DrawCircle(spriteBatch, Position, innerRadius, buttonColor * 1.5f, 16, pixelTexture);
            
            // Draw label text if present
            if (!string.IsNullOrEmpty(Label) && font != null)
            {
                Vector2 textSize = font.MeasureString(Label);
                Vector2 textPos = Position - textSize / 2;
                
                spriteBatch.DrawString(font, Label, textPos, IconColor);
            }
        }
        
        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments, Texture2D pixelTexture)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(i * 2 * Math.PI / segments);
                float angle2 = (float)((i + 1) * 2 * Math.PI / segments);
                
                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
                
                float length = Vector2.Distance(point1, point2);
                float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                
                spriteBatch.Draw(
                    pixelTexture,
                    point1,
                    null,
                    color,
                    angle,
                    new Vector2(0, 0.5f),
                    new Vector2(length, 1f),
                    SpriteEffects.None,
                    0f);
            }
        }
    }
    
    public class LongPressEventArgs : EventArgs
    {
        public float Duration { get; set; }
    }
}
