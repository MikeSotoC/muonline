using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Interface for mobile touch input controls
    /// </summary>
    public interface IMobileInput
    {
        /// <summary>
        /// Unique identifier for the touch input control
        /// </summary>
        int ControlId { get; }
        
        /// <summary>
        /// Indicates if the control is currently active (being touched)
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Indicates if the control is visible and can receive input
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Gets the current touch state for this control
        /// </summary>
        TouchLocationState TouchState { get; }
        
        /// <summary>
        /// Updates the control with new touch input
        /// </summary>
        void UpdateTouch(TouchCollection touchState);
        
        /// <summary>
        /// Resets the control state
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Checks if a touch position intersects with this control
        /// </summary>
        bool ContainsTouch(Vector2 touchPosition);
    }
}
