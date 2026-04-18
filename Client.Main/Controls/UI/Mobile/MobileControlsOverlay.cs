using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Mobile controls overlay that manages virtual joystick and buttons
    /// </summary>
    public class MobileControlsOverlay
    {
        // Controls
        public VirtualJoystick MovementJoystick { get; private set; }
        public VirtualButton AttackButton { get; private set; }
        public VirtualButton SkillButton1 { get; private set; }
        public VirtualButton SkillButton2 { get; private set; }
        public VirtualButton SkillButton3 { get; private set; }
        public VirtualButton SkillButton4 { get; private set; }
        public VirtualButton InteractButton { get; private set; }
        public VirtualButton SkillSwapButton { get; private set; } // Button to swap skill sets
        
        // Configuration
        public bool VisibleOnDesktop { get; set; } = false;
        public bool Visible { get; set; } = true;
        
        // State
        public bool IsMobilePlatform => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        
        // Skill configuration - allows switching between different skill sets
        private int _currentSkillSetIndex = 0;
        private const int MaxSkillSets = 3; // 3 sets of 4 skills each = 12 skills total
        private readonly string[] _skillSetLabels = { "A", "B", "C" };
        
        // Currently selected skills for each button (indexes into the character's skill list)
        private readonly int[][] _skillSetAssignments = new int[MaxSkillSets][]
        {
            new int[4] { 0, 1, 2, 3 }, // Set A: first 4 skills
            new int[4] { 4, 5, 6, 7 }, // Set B: next 4 skills
            new int[4] { 8, 9, 10, 11 } // Set C: last 4 skills
        };
        
        private SpriteFont _font;
        
        public MobileControlsOverlay()
        {
        }
        
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("Arial");
            }
            catch
            {
                // Font not found, will use default rendering without text
            }
        }
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            if (!IsMobilePlatform && !VisibleOnDesktop)
            {
                Visible = false;
                return;
            }
            
            Visible = true;
            
            // Initialize movement joystick (bottom-left)
            MovementJoystick = new VirtualJoystick
            {
                Radius = 70f,
                DeadZone = 0.15f,
                BaseColor = Color.White * 0.2f,
                ActiveColor = Color.Cyan * 0.4f,
                StickColor = Color.Cyan * 0.7f
            };
            
            // Position joystick in bottom-left corner
            int joystickMargin = 60;
            int joystickX = joystickMargin;
            int joystickY = screenHeight - joystickMargin - (int)(MovementJoystick.Radius * 2);
            MovementJoystick.SetPosition(joystickX, joystickY);
            
            // Initialize attack button (bottom-right)
            AttackButton = new VirtualButton
            {
                Radius = 50f,
                Label = "⚔️",
                BaseColor = Color.Red * 0.3f,
                PressedColor = Color.Red * 0.6f
            };
            
            int buttonMargin = 60;
            int buttonSpacing = 70;
            int skillButtonSize = 45;
            int attackX = screenWidth - buttonMargin - (int)(AttackButton.Radius * 2);
            int attackY = screenHeight - buttonMargin - (int)(AttackButton.Radius * 2);
            AttackButton.SetPosition(attackX, attackY);
            
            // Initialize 4 skill buttons in a 2x2 grid above the attack button
            SkillButton1 = new VirtualButton
            {
                Radius = skillButtonSize / 2f,
                Label = "1",
                BaseColor = Color.Blue * 0.3f,
                PressedColor = Color.Blue * 0.6f,
                LongPressThreshold = 0.3f
            };
            int skill1X = attackX - buttonSpacing;
            int skill1Y = attackY - buttonSpacing / 2;
            SkillButton1.SetPosition(skill1X, skill1Y);
            
            SkillButton2 = new VirtualButton
            {
                Radius = skillButtonSize / 2f,
                Label = "2",
                BaseColor = Color.Purple * 0.3f,
                PressedColor = Color.Purple * 0.6f,
                LongPressThreshold = 0.3f
            };
            int skill2X = attackX;
            int skill2Y = attackY - buttonSpacing / 2;
            SkillButton2.SetPosition(skill2X, skill2Y);
            
            SkillButton3 = new VirtualButton
            {
                Radius = skillButtonSize / 2f,
                Label = "3",
                BaseColor = Color.Green * 0.3f,
                PressedColor = Color.Green * 0.6f,
                LongPressThreshold = 0.3f
            };
            int skill3X = attackX - buttonSpacing;
            int skill3Y = attackY - buttonSpacing / 2 - buttonSpacing;
            SkillButton3.SetPosition(skill3X, skill3Y);
            
            SkillButton4 = new VirtualButton
            {
                Radius = skillButtonSize / 2f,
                Label = "4",
                BaseColor = Color.Orange * 0.3f,
                PressedColor = Color.Orange * 0.6f,
                LongPressThreshold = 0.3f
            };
            int skill4X = attackX;
            int skill4Y = attackY - buttonSpacing / 2 - buttonSpacing;
            SkillButton4.SetPosition(skill4X, skill4Y);
            
            // Initialize skill swap button (above skill buttons, to change skill sets)
            SkillSwapButton = new VirtualButton
            {
                Radius = 30f,
                Label = _skillSetLabels[_currentSkillSetIndex],
                BaseColor = Color.Yellow * 0.3f,
                PressedColor = Color.Yellow * 0.7f,
                LongPressThreshold = 0.2f
            };
            int swapX = attackX - buttonSpacing / 2;
            int swapY = skill3Y - 50;
            SkillSwapButton.SetPosition(swapX, swapY);
            
            // Initialize interact button (near joystick)
            InteractButton = new VirtualButton
            {
                Radius = 35f,
                Label = "E",
                BaseColor = Color.Green * 0.3f,
                PressedColor = Color.Green * 0.6f,
                LongPressThreshold = 0.5f
            };
            
            int interactX = joystickMargin + (int)(MovementJoystick.Radius * 2) + 20;
            int interactY = joystickY + (int)(MovementJoystick.Radius) - (int)(InteractButton.Radius);
            InteractButton.SetPosition(interactX, interactY);
        }
        
        public void Update(GameTime gameTime, TouchCollection touchState)
        {
            if (!Visible) return;
            
            MovementJoystick?.Update(touchState);
            AttackButton?.Update(touchState, gameTime);
            SkillButton1?.Update(touchState, gameTime);
            SkillButton2?.Update(touchState, gameTime);
            SkillButton3?.Update(touchState, gameTime);
            SkillButton4?.Update(touchState, gameTime);
            InteractButton?.Update(touchState, gameTime);
            
            // Handle skill swap button
            if (SkillSwapButton != null)
            {
                SkillSwapButton.Update(touchState, gameTime);
                if (SkillSwapButton.IsPressed && !_wasSwapPressed)
                {
                    OnSkillSwapClicked();
                }
                _wasSwapPressed = SkillSwapButton.IsPressed;
            }
        }
        
        private bool _wasSwapPressed = false;
        
        private void OnSkillSwapClicked()
        {
            // Cycle through skill sets: A -> B -> C -> A
            _currentSkillSetIndex = (_currentSkillSetIndex + 1) % MaxSkillSets;
            SkillSwapButton.Label = _skillSetLabels[_currentSkillSetIndex];
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (!Visible) return;
            
            MovementJoystick?.Draw(spriteBatch, pixelTexture);
            AttackButton?.Draw(spriteBatch, pixelTexture, _font);
            SkillButton1?.Draw(spriteBatch, pixelTexture, _font);
            SkillButton2?.Draw(spriteBatch, pixelTexture, _font);
            SkillButton3?.Draw(spriteBatch, pixelTexture, _font);
            SkillButton4?.Draw(spriteBatch, pixelTexture, _font);
            InteractButton?.Draw(spriteBatch, pixelTexture, _font);
            SkillSwapButton?.Draw(spriteBatch, pixelTexture, _font);
        }
        
        /// <summary>
        /// Gets the current movement input from the joystick
        /// </summary>
        public Vector2 GetMovementInput()
        {
            if (MovementJoystick == null || !MovementJoystick.IsActive)
                return Vector2.Zero;
            
            return MovementJoystick.Direction * MovementJoystick.Magnitude;
        }
        
        /// <summary>
        /// Checks if the attack button is currently pressed
        /// </summary>
        public bool IsAttacking()
        {
            return AttackButton?.IsPressed ?? false;
        }
        
        /// <summary>
        /// Checks if a skill button is currently pressed
        /// </summary>
        public bool IsSkillPressed(int skillIndex)
        {
            return skillIndex switch
            {
                1 => SkillButton1?.IsPressed ?? false,
                2 => SkillButton2?.IsPressed ?? false,
                3 => SkillButton3?.IsPressed ?? false,
                4 => SkillButton4?.IsPressed ?? false,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets the currently active skill set index (0=A, 1=B, 2=C)
        /// </summary>
        public int GetCurrentSkillSetIndex()
        {
            return _currentSkillSetIndex;
        }
        
        /// <summary>
        /// Gets the skill index for a given skill button in the current set
        /// </summary>
        public int GetSkillIndexForButton(int buttonIndex)
        {
            if (buttonIndex < 1 || buttonIndex > 4)
                return -1;
            
            return _skillSetAssignments[_currentSkillSetIndex][buttonIndex - 1];
        }
        
        /// <summary>
        /// Sets a custom skill assignment for a specific button in a specific set
        /// </summary>
        public void SetSkillAssignment(int skillSetIndex, int buttonIndex, int skillIndex)
        {
            if (skillSetIndex < 0 || skillSetIndex >= MaxSkillSets)
                return;
            if (buttonIndex < 0 || buttonIndex >= 4)
                return;
            
            _skillSetAssignments[skillSetIndex][buttonIndex] = skillIndex;
        }
        
        /// <summary>
        /// Checks if the interact button is currently pressed
        /// </summary>
        public bool IsInteracting()
        {
            return InteractButton?.IsPressed ?? false;
        }
        
        /// <summary>
        /// Shows the mobile controls overlay
        /// </summary>
        public void Show()
        {
            if (IsMobilePlatform || VisibleOnDesktop)
            {
                Visible = true;
            }
        }
        
        /// <summary>
        /// Hides the mobile controls overlay
        /// </summary>
        public void Hide()
        {
            Visible = false;
        }
        
        /// <summary>
        /// Toggles visibility of mobile controls
        /// </summary>
        public void Toggle()
        {
            Visible = !Visible;
        }
    }
}
