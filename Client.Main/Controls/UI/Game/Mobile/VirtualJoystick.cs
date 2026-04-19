using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Virtual joystick for mobile movement control
    /// </summary>
    public class VirtualJoystick : UIControl
    {
        private Vector2 _centerPosition;
        private Vector2 _currentPosition;
        private float _radius;
        private int? _activeTouchId;
        private bool _isDragging;
        private Vector2 _inputVector;
        
        // Visual properties
        private Color _baseColor;
        private Color _activeColor;
        private float _baseRadius;
        private float _stickRadius;

        public Vector2 InputVector => _inputVector;
        public bool IsActive => _isDragging;
        public float Radius => _radius;

        public VirtualJoystick(Vector2 centerPosition, float radius = 75f)
        {
            _centerPosition = centerPosition;
            _radius = radius;
            _baseRadius = radius;
            _stickRadius = radius * 0.4f;
            _currentPosition = centerPosition;
            _inputVector = Vector2.Zero;
            
            _baseColor = new Color(50, 50, 50, 180);
            _activeColor = new Color(100, 150, 255, 200);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var touchState = TouchPanel.GetState();
            bool wasDragging = _isDragging;
            _isDragging = false;

            if (_activeTouchId.HasValue)
            {
                // Check if our active touch is still active
                bool found = false;
                foreach (var touch in touchState)
                {
                    if (touch.Id == _activeTouchId.Value)
                    {
                        found = true;
                        if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
                        {
                            UpdateStickPosition(touch.Position.ToVector2());
                            _isDragging = true;
                        }
                        else if (touch.State == TouchLocationState.Released)
                        {
                            ResetStick();
                        }
                        break;
                    }
                }

                if (!found)
                {
                    // Touch lost, reset
                    ResetStick();
                }
            }
            else
            {
                // Look for new touch
                foreach (var touch in touchState)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        float distance = Vector2.Distance(touch.Position.ToVector2(), _centerPosition);
                        if (distance <= _radius * 1.5f) // Slightly larger hit area
                        {
                            _activeTouchId = touch.Id;
                            UpdateStickPosition(touch.Position.ToVector2());
                            _isDragging = true;
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateStickPosition(Vector2 touchPosition)
        {
            Vector2 direction = touchPosition - _centerPosition;
            float length = direction.Length();
            
            if (length > _radius)
            {
                direction.Normalize();
                _currentPosition = _centerPosition + direction * _radius;
            }
            else
            {
                _currentPosition = touchPosition;
            }

            // Calculate input vector (-1 to 1)
            Vector2 offset = _currentPosition - _centerPosition;
            _inputVector = offset / _radius;
            _inputVector = Vector2.Clamp(_inputVector, Vector2.One * -1f, Vector2.One);
        }

        private void ResetStick()
        {
            _activeTouchId = null;
            _currentPosition = _centerPosition;
            _inputVector = Vector2.Zero;
            _isDragging = false;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw base circle
            DrawCircle(spriteBatch, _centerPosition, _baseRadius, _baseColor);
            
            // Draw stick
            Color stickColor = _isDragging ? _activeColor : _baseColor;
            DrawCircle(spriteBatch, _currentPosition, _stickRadius, stickColor);
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            // Simple circle approximation using rectangles
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(i * MathHelper.Pi * 2 / segments);
                float angle2 = (float)((i + 1) * MathHelper.Pi * 2 / segments);
                
                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
                
                // Draw line between points
                float lineLength = Vector2.Distance(point1, point2);
                float rotation = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                
                // Use a 1x1 white texture or create one
                Rectangle rect = new Rectangle((int)point1.X, (int)point1.Y, (int)lineLength, 3);
                spriteBatch.Draw(TextureLoader.WhiteTexture, rect, null, color, rotation, Vector2.Zero, SpriteEffects.None, 0);
            }
            
            // Fill center
            Rectangle centerRect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));
            spriteBatch.Draw(TextureLoader.WhiteTexture, centerRect, color);
        }

        public void SetPosition(Vector2 newPosition)
        {
            _centerPosition = newPosition;
            if (!_isDragging)
            {
                _currentPosition = newPosition;
            }
        }
    }
}
