using Client.Main.Objects.Player;
using Client.Main.Objects.Monsters;
using Microsoft.Xna.Framework;
using System;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Mobile control overlay that manages joystick and action buttons
    /// </summary>
    public class MobileControlsOverlay : UIControl
    {
        private VirtualJoystick _movementJoystick;
        private MobileActionButton _attackButton;
        private MobileActionButton _skill1Button;
        private MobileActionButton _skill2Button;
        
        private PlayerObject _player;
        private Vector2 _joystickPosition;
        private bool _isVisible;
        
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

        public MobileControlsOverlay()
        {
            // Positions will be set based on screen size in InitializePositions
            _movementJoystick = new VirtualJoystick(Vector2.Zero, 75f);
            _attackButton = new MobileActionButton(Vector2.Zero, 60f, "ATK");
            _skill1Button = new MobileActionButton(Vector2.Zero, 50f, "S1");
            _skill2Button = new MobileActionButton(Vector2.Zero, 50f, "S2");
            
            SetupButtonEvents();
            
            Visible = false;
            _isVisible = false;
        }

        private void SetupButtonEvents()
        {
            _attackButton.OnPress += OnAttackPressed;
            _attackButton.OnRelease += OnAttackReleased;
            
            _skill1Button.OnClick += OnSkill1Clicked;
            _skill2Button.OnClick += OnSkill2Clicked;
        }

        public void InitializePositions(int screenWidth, int screenHeight)
        {
            // Joystick on bottom-left
            float joystickMargin = 100f;
            _joystickPosition = new Vector2(joystickMargin, screenHeight - joystickMargin);
            _movementJoystick.SetPosition(_joystickPosition);
            
            // Buttons on bottom-right
            float buttonMargin = 100f;
            float buttonSpacing = 80f;
            
            _attackButton.SetPosition(new Vector2(screenWidth - buttonMargin, screenHeight - buttonMargin));
            _skill1Button.SetPosition(new Vector2(screenWidth - buttonMargin - buttonSpacing, screenHeight - buttonMargin + 20f));
            _skill2Button.SetPosition(new Vector2(screenWidth - buttonMargin + buttonSpacing, screenHeight - buttonMargin + 20f));
        }

        public void SetPlayer(PlayerObject player)
        {
            _player = player;
        }

        private void OnAttackPressed()
        {
            if (_player != null && _player.IsAlive())
            {
                // Attack will be handled by WalkableWorldControl based on mouse/touch target
                // This button just signals attack intent
            }
        }

        private void OnAttackReleased()
        {
            // Stop attack if needed
        }

        private void OnSkill1Clicked()
        {
            if (_player != null && _player.IsAlive())
            {
                // TODO: Use skill from slot 0
                // For now, we'll trigger a basic attack as placeholder
            }
        }

        private void OnSkill2Clicked()
        {
            if (_player != null && _player.IsAlive())
            {
                // TODO: Use skill from slot 1
                // For now, we'll trigger a basic attack as placeholder
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible || !Enabled)
                return;

            base.Update(gameTime);
            
            // Update joystick
            _movementJoystick.Update(gameTime);
            
            // Update buttons
            _attackButton.Update(gameTime);
            _skill1Button.Update(gameTime);
            _skill2Button.Update(gameTime);
            
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
            // Note: This is simplified - actual implementation needs camera angle consideration
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
        }
    }
}
