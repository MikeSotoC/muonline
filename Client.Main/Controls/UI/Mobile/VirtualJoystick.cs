using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Virtual joystick control for mobile movement
    /// </summary>
    public class VirtualJoystick : GameControl
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
                ViewSize = new Point((int)(_radius * 2), (int)(_radius * 2));
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
        
        // Events
        public event EventHandler<Vector2> DirectionChanged;
        
        public VirtualJoystick()
        {
            Interactive = true;
            CapturePointerWhenNonInteractive = true;
            Radius = 60f;
        }
        
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
            _centerPosition = new Vector2(x + Radius, y + Radius);
            _currentPosition = _centerPosition;
        }
        
        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                _isActive = false;
                _touchId = -1;
                Direction = Vector2.Zero;
                Magnitude = 0f;
                _currentPosition = _centerPosition;
                return;
            }
            
            var touchState = MuGame.Instance.Touch;
            var prevTouchState = MuGame.Instance.PrevTouchState;
            
            bool touchFound = false;
            
            // Check for active touch on this joystick
            for (int i = 0; i < touchState.Count; i++)
            {
                var touch = touchState[i];
                
                // Find our touch or acquire new one
                if (_touchId == -1 || touch.Id == _touchId)
                {
                    var touchPos = touch.Position;
                    Rectangle touchRect = new Rectangle(
                        DisplayPosition.X, 
                        DisplayPosition.Y, 
                        DisplaySize.X, 
                        DisplaySize.Y);
                    
                    if (_touchId == -1)
                    {
                        // Try to acquire touch if it's within or near the joystick
                        Rectangle expandedRect = new Rectangle(
                            touchRect.X - (int)Radius, 
                            touchRect.Y - (int)Radius, 
                            touchRect.Width + (int)(Radius * 2), 
                            touchRect.Height + (int)(Radius * 2));
                        
                        if (expandedRect.Contains((int)touchPos.X, (int)touchPos.Y))
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
                for (int i = 0; i < touchState.Count; i++)
                {
                    if (touchState[i].Id == _touchId && touchState[i].State != TouchLocationState.Released)
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
            
            base.Update(gameTime);
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
        
        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;
            
            SpriteBatch spriteBatch = GraphicsManager.Instance.Sprite;
            
            // Draw base circle
            spriteBatch.Draw(
                GraphicsManager.Instance.Pixel,
                new Rectangle(
                    DisplayPosition.X, 
                    DisplayPosition.Y, 
                    DisplaySize.X, 
                    DisplaySize.Y),
                null,
                _isActive ? ActiveColor : BaseColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
            
            // Draw stick
            float stickRadius = Radius * 0.4f;
            spriteBatch.Draw(
                GraphicsManager.Instance.Pixel,
                new Rectangle(
                    (int)(_currentPosition.X - stickRadius),
                    (int)(_currentPosition.Y - stickRadius),
                    (int)(stickRadius * 2),
                    (int)(stickRadius * 2)),
                null,
                StickColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
            
            base.Draw(gameTime);
        }
    }
}
