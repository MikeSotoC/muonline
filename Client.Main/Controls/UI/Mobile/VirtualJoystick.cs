using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Virtual joystick control for mobile movement
    /// </summary>
    public class VirtualJoystick
    {
        // Fields
        private Vector2 _centerPosition;
        private Vector2 _currentPosition;
        private Vector2 _delta;
        private float _radius;
        private int _touchId = -1;
        private bool _isActive;
        
        // Configuration
        public float Radius 
        { 
            get => _radius; 
            set 
            { 
                _radius = value; 
            } 
        }
        
        public float DeadZone { get; set; } = 0.1f;
        public Color BaseColor { get; set; } = Color.White * 0.3f;
        public Color ActiveColor { get; set; } = Color.White * 0.6f;
        public Color StickColor { get; set; } = Color.White * 0.8f;
        
        // Output values
        public Vector2 Direction { get; private set; } = Vector2.Zero;
        public float Magnitude { get; private set; } = 0f;
        public bool IsActive => _isActive;
        public Vector2 Position { get; set; }
        
        // Events
        public event EventHandler<Vector2> DirectionChanged;
        
        public VirtualJoystick()
        {
            Radius = 60f;
        }
        
        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
            _centerPosition = new Vector2(x + Radius, y + Radius);
            _currentPosition = _centerPosition;
        }
        
        public void Update(TouchCollection touchState)
        {
            bool touchFound = false;
            
            // Check for active touch on this joystick
            foreach (var touch in touchState)
            {
                // Find our touch or acquire new one
                if (_touchId == -1 || touch.Id == _touchId)
                {
                    var touchPos = touch.Position;
                    
                    if (_touchId == -1)
                    {
                        // Try to acquire touch if it's within or near the joystick
                        float distance = Vector2.Distance(touchPos, _centerPosition);
                        if (distance <= Radius * 2)
                        {
                            _touchId = touch.Id;
                            _isActive = true;
                        }
                    }
                    
                    if (_touchId == touch.Id && touch.State != TouchLocationState.Released)
                    {
                        touchFound = true;
                        UpdateJoystick(touchPos);
                    }
                    else if (touch.State == TouchLocationState.Released && _touchId == touch.Id)
                    {
                        ResetJoystick();
                    }
                }
            }
            
            // If no touch found and we had one, reset
            if (!touchFound && _isActive)
            {
                // Check if touch was released
                bool stillActive = false;
                foreach (var touch in touchState)
                {
                    if (touch.Id == _touchId && touch.State != TouchLocationState.Released)
                    {
                        stillActive = true;
                        break;
                    }
                }
                
                if (!stillActive)
                {
                    ResetJoystick();
                }
            }
        }
        
        private void UpdateJoystick(Vector2 touchPos)
        {
            Vector2 direction = touchPos - _centerPosition;
            float length = direction.Length();
            
            if (length > Radius)
            {
                direction.Normalize();
                direction *= Radius;
            }
            
            _currentPosition = _centerPosition + direction;
            
            // Calculate normalized direction and magnitude
            if (length > DeadZone * Radius)
            {
                Direction = direction / Radius;
                Magnitude = Math.Min(length / Radius, 1f);
            }
            else
            {
                Direction = Vector2.Zero;
                Magnitude = 0f;
            }
            
            DirectionChanged?.Invoke(this, Direction * Magnitude);
        }
        
        private void ResetJoystick()
        {
            _isActive = false;
            _touchId = -1;
            _currentPosition = _centerPosition;
            Direction = Vector2.Zero;
            Magnitude = 0f;
            DirectionChanged?.Invoke(this, Vector2.Zero);
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            // Draw base circle
            DrawCircle(spriteBatch, _centerPosition, Radius, _isActive ? ActiveColor : BaseColor, 32, pixelTexture);
            
            // Draw stick
            float stickRadius = Radius * 0.4f;
            DrawCircle(spriteBatch, _currentPosition, stickRadius, StickColor, 16, pixelTexture);
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
}
