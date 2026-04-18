using Client.Main.Controls;
using Microsoft.Xna.Framework;
using System;

namespace Client.Main.Controls.UI.Mobile
{
    /// <summary>
    /// Mobile controls overlay that manages virtual joystick and buttons
    /// </summary>
    public class MobileControlsOverlay : GameControl
    {
        // Controls
        public VirtualJoystick MovementJoystick { get; private set; }
        public VirtualButton AttackButton { get; private set; }
        public VirtualButton SkillButton1 { get; private set; }
        public VirtualButton SkillButton2 { get; private set; }
        public VirtualButton InteractButton { get; private set; }
        
        // Configuration
        public bool VisibleOnDesktop { get; set; } = false;
        public float Opacity { get; set; } = 1f;
        
        // State
        public bool IsMobilePlatform => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        
        public MobileControlsOverlay()
        {
            Interactive = true;
            CapturePointerWhenNonInteractive = true;
            Align = ControlAlign.None;
        }
        
        public override async Task Load()
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
            int joystickY = MuGame.Instance.Height - joystickMargin - (int)(MovementJoystick.Radius * 2);
            MovementJoystick.SetPosition(joystickX, joystickY);
            Controls.Add(MovementJoystick);
            
            // Initialize attack button (bottom-right)
            AttackButton = new VirtualButton
            {
                Radius = 50f,
                Label = "⚔️",
                BaseColor = Color.Red * 0.3f,
                PressedColor = Color.Red * 0.6f
            };
            
            int buttonMargin = 60;
            int buttonSpacing = 80;
            int attackX = MuGame.Instance.Width - buttonMargin - (int)(AttackButton.Radius * 2);
            int attackY = MuGame.Instance.Height - buttonMargin - (int)(AttackButton.Radius * 2);
            AttackButton.SetPosition(attackX, attackY);
            Controls.Add(AttackButton);
            
            // Initialize skill button 1 (above attack)
            SkillButton1 = new VirtualButton
            {
                Radius = 40f,
                Label = "1",
                BaseColor = Color.Blue * 0.3f,
                PressedColor = Color.Blue * 0.6f,
                LongPressThreshold = 0.3f
            };
            
            int skill1X = attackX - buttonSpacing;
            int skill1Y = attackY - buttonSpacing / 2;
            SkillButton1.SetPosition(skill1X, skill1Y);
            Controls.Add(SkillButton1);
            
            // Initialize skill button 2 (above skill 1)
            SkillButton2 = new VirtualButton
            {
                Radius = 40f,
                Label = "2",
                BaseColor = Color.Purple * 0.3f,
                PressedColor = Color.Purple * 0.6f,
                LongPressThreshold = 0.3f
            };
            
            int skill2X = skill1X;
            int skill2Y = skill1Y - buttonSpacing;
            SkillButton2.SetPosition(skill2X, skill2Y);
            Controls.Add(SkillButton2);
            
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
            Controls.Add(InteractButton);
            
            await base.Load();
        }
        
        public override void Update(GameTime gameTime)
        {
            // Only update on mobile platforms or if explicitly enabled for desktop
            if (!IsMobilePlatform && !VisibleOnDesktop)
            {
                Visible = false;
                return;
            }
            
            // Apply opacity to all child controls
            foreach (var control in Controls)
            {
                if (control is VirtualJoystick joystick)
                {
                    // Joystick colors already have alpha, but we could modulate here if needed
                }
                else if (control is VirtualButton button)
                {
                    // Button colors already have alpha
                }
            }
            
            base.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;
            
            // Set overall opacity
            float oldAlpha = Alpha;
            Alpha = Opacity;
            
            base.Draw(gameTime);
            
            Alpha = oldAlpha;
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
                _ => false
            };
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
