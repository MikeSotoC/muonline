using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Action button for mobile controls (attack, skills, etc.)
    /// </summary>
    public class MobileActionButton : UIControl, IMobileInput
    {
        private Vector2 _position;
        private float _radius;
        private int? _activeTouchId;
        private bool _isPressed;
        private string _label;
        private Color _baseColor;
        private Color _pressedColor;
        private Color _textColor;
        private TouchLocationState _touchState;
        private static int _nextControlId = 0;
        private readonly int _controlId;
        
        public event Action OnClick;
        public event Action OnPress;
        public event Action OnRelease;
        
        public int ControlId => _controlId;
        public bool IsPressed => _isPressed;
        public bool IsActive => _isPressed;
        public bool IsEnabled => Visible && Enabled;
        public TouchLocationState TouchState => _touchState;
        public string Label => _label;

        public MobileActionButton(Vector2 position, float radius = 50f, string label = "A")
        {
            _controlId = _nextControlId++;
            _position = position;
            _radius = radius;
            _label = label;
            _touchState = TouchLocationState.Invalid;
            
            _baseColor = new Color(80, 80, 80, 200);
            _pressedColor = new Color(150, 80, 80, 220);
            _textColor = Color.White;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var touchState = TouchPanel.GetState();
            bool wasPressed = _isPressed;
            _isPressed = false;
            _touchState = TouchLocationState.Invalid;

            if (_activeTouchId.HasValue)
            {
                // Check if our active touch is still active
                bool found = false;
                foreach (var touch in touchState)
                {
                    if (touch.Id == _activeTouchId.Value)
                    {
                        found = true;
                        _touchState = touch.State;
                        if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
                        {
                            // Check if still within button area
                            float distance = Vector2.Distance(touch.Position.ToVector2(), _position);
                            if (distance <= _radius * 1.2f)
                            {
                                _isPressed = true;
                                if (!wasPressed && OnPress != null)
                                {
                                    OnPress();
                                }
                            }
                            else if (wasPressed && OnRelease != null)
                            {
                                OnRelease();
                            }
                        }
                        else if (touch.State == TouchLocationState.Released)
                        {
                            if (wasPressed && OnClick != null)
                            {
                                OnClick();
                            }
                            if (wasPressed && OnRelease != null)
                            {
                                OnRelease();
                            }
                            _activeTouchId = null;
                        }
                        break;
                    }
                }

                if (!found)
                {
                    // Touch lost
                    if (wasPressed && OnRelease != null)
                    {
                        OnRelease();
                    }
                    _activeTouchId = null;
                }
            }
            else
            {
                // Look for new touch
                foreach (var touch in touchState)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        float distance = Vector2.Distance(touch.Position.ToVector2(), _position);
                        if (distance <= _radius)
                        {
                            _activeTouchId = touch.Id;
                            _touchState = touch.State;
                            _isPressed = true;
                            if (OnPress != null)
                            {
                                OnPress();
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateTouch(TouchCollection touchState)
        {
            // This method is called by MobileControlsOverlay for centralized touch management
            // The main Update method already handles touch processing
            // This is kept for interface compatibility
        }

        public void Reset()
        {
            _activeTouchId = null;
            _isPressed = false;
            _touchState = TouchLocationState.Invalid;
        }

        public bool ContainsTouch(Vector2 touchPosition)
        {
            float distance = Vector2.Distance(touchPosition, _position);
            return distance <= _radius;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw button circle
            Color buttonColor = _isPressed ? _pressedColor : _baseColor;
            DrawCircle(spriteBatch, _position, _radius, buttonColor);
            
            // Draw label
            if (!string.IsNullOrEmpty(_label))
            {
                Vector2 textSize = TextureLoader.DefaultFont.MeasureString(_label);
                Vector2 textPosition = _position - textSize / 2;
                spriteBatch.DrawString(TextureLoader.DefaultFont, _label, textPosition, _textColor);
            }
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            // Draw filled circle using rectangles
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(i * MathHelper.Pi * 2 / segments);
                float angle2 = (float)((i + 1) * MathHelper.Pi * 2 / segments);
                
                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
                
                float lineLength = Vector2.Distance(point1, point2);
                float rotation = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                
                Rectangle rect = new Rectangle((int)point1.X, (int)point1.Y, (int)lineLength, 3);
                spriteBatch.Draw(TextureLoader.WhiteTexture, rect, null, color, rotation, Vector2.Zero, SpriteEffects.None, 0);
            }
            
            // Fill center
            Rectangle centerRect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));
            spriteBatch.Draw(TextureLoader.WhiteTexture, centerRect, color);
        }

        public void SetPosition(Vector2 newPosition)
        {
            _position = newPosition;
        }
    }
}
