using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace Client.Main.Controls.UI.Game.Mobile
{
    /// <summary>
    /// Handles gesture-based touch input for mobile devices
    /// </summary>
    public class TouchGestureHandler
    {
        private Dictionary<int, TouchLocation> _activeTouches = new();
        private Dictionary<int, Vector2> _previousPositions = new();
        private Dictionary<int, float> _touchTimers = new();
        
        // Gesture thresholds
        private const float TapTimeout = 0.3f;
        private const float DoubleTapTimeout = 0.4f;
        private const float SwipeThreshold = 50f;
        private const float LongPressTimeout = 0.8f;
        
        // Recent tap tracking for double-tap
        private Vector2 _lastTapPosition;
        private float _lastTapTime;
        
        public event Action<Vector2> OnTap;
        public event Action<Vector2> OnDoubleTap;
        public event Action<Vector2> OnLongPress;
        public event Action<Vector2, Vector2> OnSwipe;
        public event Action<Vector2> OnPanStart;
        public event Action<Vector2> OnPanEnd;
        
        public void Update(GameTime gameTime, TouchCollection touchState)
        {
            var currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
            
            // Process each touch
            foreach (var touch in touchState)
            {
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        HandleTouchPressed(touch, currentTime);
                        break;
                        
                    case TouchLocationState.Moved:
                        HandleTouchMoved(touch, currentTime);
                        break;
                        
                    case TouchLocationState.Released:
                        HandleTouchReleased(touch, currentTime);
                        break;
                }
                
                // Update position for next frame
                if (_activeTouches.ContainsKey(touch.Id))
                {
                    _activeTouches[touch.Id] = touch;
                    _previousPositions[touch.Id] = touch.Position.ToVector2();
                }
            }
            
            // Remove released touches
            var releasedIds = new List<int>();
            foreach (var kvp in _activeTouches)
            {
                bool found = false;
                foreach (var touch in touchState)
                {
                    if (touch.Id == kvp.Key)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    releasedIds.Add(kvp.Key);
                }
            }
            
            foreach (var id in releasedIds)
            {
                _activeTouches.Remove(id);
                _previousPositions.Remove(id);
                _touchTimers.Remove(id);
            }
            
            // Check for long press timeouts
            CheckLongPressTimeouts(currentTime);
        }
        
        private void HandleTouchPressed(TouchLocation touch, float currentTime)
        {
            _activeTouches[touch.Id] = touch;
            _previousPositions[touch.Id] = touch.Position.ToVector2();
            _touchTimers[touch.Id] = currentTime;
        }
        
        private void HandleTouchMoved(TouchLocation touch, float currentTime)
        {
            if (!_previousPositions.TryGetValue(touch.Id, out var prevPos))
                return;
                
            var currentPos = touch.Position.ToVector2();
            var delta = currentPos - prevPos;
            
            // Check for swipe
            if (delta.Length() > SwipeThreshold)
            {
                OnSwipe?.Invoke(prevPos, currentPos);
                _touchTimers[touch.Id] = 0; // Reset timer to prevent long press
            }
        }
        
        private void HandleTouchReleased(TouchLocation touch, float currentTime)
        {
            if (!_touchTimers.TryGetValue(touch.Id, out var pressTime))
                return;
                
            var pressDuration = currentTime - pressTime;
            var position = touch.Position.ToVector2();
            
            // Check for tap vs long press
            if (pressDuration < TapTimeout)
            {
                // Check for double-tap
                float timeSinceLastTap = currentTime - _lastTapTime;
                float distanceFromLastTap = Vector2.Distance(position, _lastTapPosition);
                
                if (timeSinceLastTap < DoubleTapTimeout && distanceFromLastTap < 30f)
                {
                    OnDoubleTap?.Invoke(position);
                    _lastTapTime = 0; // Reset to prevent triple-tap
                }
                else
                {
                    OnTap?.Invoke(position);
                    _lastTapPosition = position;
                    _lastTapTime = currentTime;
                }
            }
            else if (pressDuration >= LongPressTimeout)
            {
                OnLongPress?.Invoke(position);
            }
            
            OnPanEnd?.Invoke(position);
        }
        
        private void CheckLongPressTimeouts(float currentTime)
        {
            foreach (var kvp in _touchTimers)
            {
                if (currentTime - kvp.Value >= LongPressTimeout)
                {
                    if (_activeTouches.ContainsKey(kvp.Key))
                    {
                        var touch = _activeTouches[kvp.Key];
                        OnLongPress?.Invoke(touch.Position.ToVector2());
                        _touchTimers[kvp.Key] = 0; // Prevent repeated triggers
                    }
                }
            }
        }
        
        public void Reset()
        {
            _activeTouches.Clear();
            _previousPositions.Clear();
            _touchTimers.Clear();
            _lastTapTime = 0;
        }
    }
}
