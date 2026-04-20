using Client.Main.Objects.Player;
using Client.Main.Objects.Monsters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Enhanced mobile control overlay with gesture support and improved UI
    /// </summary>
    public class MobileControlsOverlay : UIControl
    {
        private VirtualJoystick _movementJoystick;
        private MobileActionButton _attackButton;
        private MobileActionButton _skill1Button;
        private MobileActionButton _skill2Button;
        private MobileActionButton _skill3Button;
        private MobileActionButton _skill4Button;
        private MobileActionButton _menuButton;
        private MobileActionButton _mapButton;
        
        private TouchGestureHandler _gestureHandler;
        
        private PlayerObject _player;
        private Vector2 _joystickPosition;
        private bool _isVisible;
        private bool _isInitialized;
        
        // Camera touch control
        private bool _cameraDragging;
        private Vector2 _lastCameraTouchPos;
        
        public bool IsVisible 
        { 
            get => _isVisible;
            set 
            {
                _isVisible = value;
                Visible = value;
            }
        }
        
        public VirtualJoystick MovementJoystick => _movementJoystick;
        public MobileActionButton AttackButton => _attackButton;
        public TouchGestureHandler GestureHandler => _gestureHandler;

        public MobileControlsOverlay()
        {
            _movementJoystick = new VirtualJoystick(Vector2.Zero, 75f);
            _attackButton = new MobileActionButton(Vector2.Zero, 60f, "⚔️");
            _skill1Button = new MobileActionButton(Vector2.Zero, 50f, "S1");
            _skill2Button = new MobileActionButton(Vector2.Zero, 50f, "S2");
            _skill3Button = new MobileActionButton(Vector2.Zero, 50f, "S3");
            _skill4Button = new MobileActionButton(Vector2.Zero, 50f, "S4");
            _menuButton = new MobileActionButton(Vector2.Zero, 45f, "☰");
            _mapButton = new MobileActionButton(Vector2.Zero, 45f, "🗺️");
            
            _gestureHandler = new TouchGestureHandler();
            
            SetupButtonEvents();
            
            Visible = false;
            _isVisible = false;
            _isInitialized = false;
        }

        private void SetupButtonEvents()
        {
            _attackButton.OnPress += OnAttackPressed;
            _attackButton.OnRelease += OnAttackReleased;
            
            _skill1Button.OnClick += () => OnSkillClicked(0);
            _skill2Button.OnClick += () => OnSkillClicked(1);
            _skill3Button.OnClick += () => OnSkillClicked(2);
            _skill4Button.OnClick += () => OnSkillClicked(3);
            
            _menuButton.OnClick += OnMenuClicked;
            _mapButton.OnClick += OnMapClicked;
            
            // Gesture handlers
            _gestureHandler.OnTap += OnScreenTap;
            _gestureHandler.OnDoubleTap += OnScreenDoubleTap;
            _gestureHandler.OnLongPress += OnScreenLongPress;
        }

        public void Initialize(int screenWidth, int screenHeight)
        {
            if (_isInitialized)
                return;
                
            InitializePositions(screenWidth, screenHeight);
            _isInitialized = true;
        }
        
        public void InitializePositions(int screenWidth, int screenHeight)
        {
            // Joystick on bottom-left with safe area margin
            float joystickMargin = Math.Max(80f, screenHeight * 0.12f);
            _joystickPosition = new Vector2(joystickMargin, screenHeight - joystickMargin);
            _movementJoystick.SetPosition(_joystickPosition);
            
            // Action buttons cluster on bottom-right
            float buttonMargin = Math.Max(80f, screenWidth * 0.1f);
            float attackButtonSize = 65f;
            float skillButtonSize = 50f;
            float buttonSpacing = 70f;
            
            // Main attack button (larger, centered in cluster)
            _attackButton.SetPosition(new Vector2(
                screenWidth - buttonMargin - attackButtonSize/2, 
                screenHeight - buttonMargin - attackButtonSize/2));
            
            // Skill buttons around attack button
            _skill1Button.SetPosition(new Vector2(
                screenWidth - buttonMargin - buttonSpacing - skillButtonSize/2, 
                screenHeight - buttonMargin + 30f));
            
            _skill2Button.SetPosition(new Vector2(
                screenWidth - buttonMargin + buttonSpacing - skillButtonSize/2, 
                screenHeight - buttonMargin + 30f));
            
            _skill3Button.SetPosition(new Vector2(
                screenWidth - buttonMargin - buttonSpacing/2 - skillButtonSize/2, 
                screenHeight - buttonMargin - buttonSpacing + 10f));
            
            _skill4Button.SetPosition(new Vector2(
                screenWidth - buttonMargin + buttonSpacing/2 - skillButtonSize/2, 
                screenHeight - buttonMargin - buttonSpacing + 10f));
            
            // Utility buttons (top-right corner)
            float cornerMargin = 20f;
            float utilityButtonSize = 45f;
            
            _menuButton.SetPosition(new Vector2(
                screenWidth - cornerMargin - utilityButtonSize, 
                cornerMargin));
            
            _mapButton.SetPosition(new Vector2(
                screenWidth - cornerMargin - utilityButtonSize * 2 - 10f, 
                cornerMargin));
        }

        public void SetPlayer(PlayerObject player)
        {
            _player = player;
        }

        private void OnAttackPressed()
        {
            if (_player != null && _player.IsAlive())
            {
                // Attack logic handled by GameScene or WalkableWorldControl
            }
        }

        private void OnAttackReleased()
        {
            // Stop attack if needed
        }

        private void OnSkillClicked(int skillSlot)
        {
            if (_player != null && _player.IsAlive())
            {
                // TODO: Use skill from slot
                // This will be integrated with the skill system
            }
        }
        
        private void OnMenuClicked()
        {
            // TODO: Open pause menu
        }
        
        private void OnMapClicked()
        {
            // TODO: Toggle map visibility
        }

        private void OnScreenTap(Vector2 position)
        {
            // Tap on screen to move to location (if not touching joystick)
            if (!_movementJoystick.IsActive && _player != null && _player.IsAlive())
            {
                MoveToPosition(position);
            }
        }
        
        private void OnScreenDoubleTap(Vector2 position)
        {
            // Double-tap could trigger dash or special action
        }
        
        private void OnScreenLongPress(Vector2 position)
        {
            // Long-press could show context menu or target info
        }

        private void MoveToPosition(Vector2 screenPosition)
        {
            // Convert screen position to world position
            // This requires camera transformation which should be provided
            // For now, this is a placeholder
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible || !Enabled)
                return;

            base.Update(gameTime);
            
            var touchState = TouchPanel.GetState();
            
            // Update gesture handler first (for non-UI touches)
            _gestureHandler.Update(gameTime, touchState);
            
            // Update joystick
            _movementJoystick.Update(gameTime);
            
            // Update buttons
            _attackButton.Update(gameTime);
            _skill1Button.Update(gameTime);
            _skill2Button.Update(gameTime);
            _skill3Button.Update(gameTime);
            _skill4Button.Update(gameTime);
            _menuButton.Update(gameTime);
            _mapButton.Update(gameTime);
            
            // Apply movement input to player
            if (_movementJoystick.IsActive && _player != null && _player.IsAlive())
            {
                Vector2 input = _movementJoystick.InputVector;
                if (input.LengthSquared() > 0.1f) // Dead zone
                {
                    MovePlayer(input, gameTime.ElapsedGameTime);
                }
            }
        }

        private void MovePlayer(Vector2 input, TimeSpan elapsed)
        {
            if (_player?.Walker == null)
                return;
                
            // Convert joystick input to world movement direction
            float speed = _player.MoveSpeed * (float)elapsed.TotalSeconds;
            
            Vector3 currentPos = _player.Position;
            Vector3 newPos = currentPos + new Vector3(input.X, input.Y, 0) * speed;
            
            // Use the existing walker movement system
            var targetTile = new Vector2(
                (int)(newPos.X / Constants.TERRAIN_SCALE),
                (int)(newPos.Y / Constants.TERRAIN_SCALE)
            );
            
            _player.Walker.MoveTo(targetTile);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
                
            base.Draw(gameTime, spriteBatch);
            
            _movementJoystick.Draw(gameTime, spriteBatch);
            _attackButton.Draw(gameTime, spriteBatch);
            _skill1Button.Draw(gameTime, spriteBatch);
            _skill2Button.Draw(gameTime, spriteBatch);
            _skill3Button.Draw(gameTime, spriteBatch);
            _skill4Button.Draw(gameTime, spriteBatch);
            _menuButton.Draw(gameTime, spriteBatch);
            _mapButton.Draw(gameTime, spriteBatch);
        }
        
        public void HandleScreenResize(int screenWidth, int screenHeight)
        {
            InitializePositions(screenWidth, screenHeight);
        }
    }
}
