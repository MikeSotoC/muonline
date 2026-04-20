using System;
using System.Collections.Generic;
using System.Linq;
using Client.Main.Core.Client;
using Client.Main.Networking;
using Client.Main.Objects;
using Microsoft.Xna.Framework;

namespace Client.Main.Helpers.MuHelper
{
    public enum HelperState
    {
        Idle,
        Moving,
        Attacking,
        UsingPotion,
        UsingSkill,
        ReturningToTown
    }

    public enum TargetPriority
    {
        Closest,
        Weakest,
        Strongest,
        PlayerSelected
    }

    public class MuHelperSettings
    {
        public bool Enabled { get; set; } = false;
        public bool AutoAttack { get; set; } = true;
        public bool AutoMove { get; set; } = false;
        public bool AutoPotionHP { get; set; } = true;
        public bool AutoPotionMP { get; set; } = true;
        public bool AutoBuff { get; set; } = false;
        public bool PickItems { get; set; } = false;
        
        public int HPPotionThreshold { get; set; } = 70; // Use potion when HP < 70%
        public int MPPotionThreshold { get; set; } = 30; // Use potion when MP < 30%
        public int AttackRange { get; set; } = 5;
        public int FollowDistance { get; set; } = 3;
        
        public TargetPriority TargetPriority { get; set; } = TargetPriority.Closest;
        public List<int> TargetMonsterIds { get; set; } = new List<int>(); // Empty = all monsters
        public List<int> BuffSkillIds { get; set; } = new List<int>();
        
        public Vector2? RallyPoint { get; set; } = null; // Point to return to
        public bool ReturnToRallyPoint { get; set; } = false;
        public bool ReturnToTownOnLowHP { get; set; } = false;
        public int ReturnToTownHPThreshold { get; set; } = 30;
    }

    public class MuHelper
    {
        private readonly CharacterState _characterState;
        private readonly NetworkManager _networkManager;
        private MuHelperSettings _settings;
        private HelperState _currentState;
        private DateTime _lastActionTime;
        private DateTime _lastScanTime;
        private GameObject _currentTarget;
        private Vector2 _targetPosition;
        private DateTime _stateStartTime;
        private const int ScanIntervalMs = 500;
        private const int ActionCooldownMs = 1000;
        private const int PotionCooldownMs = 5000;
        private DateTime _lastHPPotionTime;
        private DateTime _lastMPPotionTime;

        public MuHelperSettings Settings 
        { 
            get => _settings; 
            set => _settings = value; 
        }
        
        public HelperState CurrentState => _currentState;
        public GameObject CurrentTarget => _currentTarget;
        public bool IsActive => _settings.Enabled;

        public MuHelper(CharacterState characterState, NetworkManager networkManager)
        {
            _characterState = characterState ?? throw new ArgumentNullException(nameof(characterState));
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _settings = new MuHelperSettings();
            _currentState = HelperState.Idle;
            _lastActionTime = DateTime.MinValue;
            _lastScanTime = DateTime.MinValue;
            _lastHPPotionTime = DateTime.MinValue;
            _lastMPPotionTime = DateTime.MinValue;
        }

        public void Update(GameTime gameTime)
        {
            if (!_settings.Enabled)
            {
                _currentState = HelperState.Idle;
                _currentTarget = null;
                return;
            }

            var now = DateTime.Now;
            
            // Check for emergency situations
            if (CheckEmergencyConditions(now))
                return;

            // Scan for targets periodically
            if ((now - _lastScanTime).TotalMilliseconds >= ScanIntervalMs)
            {
                _lastScanTime = now;
                ScanForTargets();
            }

            // Execute state machine
            ExecuteStateMachine(now);
        }

        private bool CheckEmergencyConditions(DateTime now)
        {
            var currentHPPercent = (_characterState.CurrentLife / _characterState.MaxLife) * 100;
            
            // Return to town on low HP
            if (_settings.ReturnToTownOnLowHP && currentHPPercent < _settings.ReturnToTownHPThreshold)
            {
                if (_currentState != HelperState.ReturningToTown)
                {
                    _currentState = HelperState.ReturningToTown;
                    _stateStartTime = now;
                    StartReturnToTown();
                }
                return true;
            }
            
            return false;
        }

        private void ScanForTargets()
        {
            if (!_settings.AutoAttack)
                return;

            var playerPos = _characterState.Position;
            var objects = ScopeManager.Instance?.GetObjectsInRange(playerPos, _settings.AttackRange);
            
            if (objects == null || objects.Count == 0)
            {
                _currentTarget = null;
                return;
            }

            // Filter valid targets (monsters)
            var validTargets = objects.Where(o => 
                o is MonsterObject monster &&
                monster.IsAlive &&
                (_settings.TargetMonsterIds.Count == 0 || _settings.TargetMonsterIds.Contains(monster.Model?.Id ?? 0))
            ).ToList();

            if (validTargets.Count == 0)
            {
                _currentTarget = null;
                return;
            }

            // Select target based on priority
            _currentTarget = _settings.TargetPriority switch
            {
                TargetPriority.Closest => validTargets.OrderBy(o => Vector2.DistanceSquared(playerPos, o.Position)).FirstOrDefault(),
                TargetPriority.Weakest => validTargets.OrderBy(o => (o as MonsterObject)?.CurrentLife ?? int.MaxValue).FirstOrDefault(),
                TargetPriority.Strongest => validTargets.OrderByDescending(o => (o as MonsterObject)?.CurrentLife ?? 0).FirstOrDefault(),
                TargetPriority.PlayerSelected => _currentTarget, // Keep current target
                _ => validTargets.FirstOrDefault()
            };
        }

        private void ExecuteStateMachine(DateTime now)
        {
            // Check action cooldown
            var canAct = (now - _lastActionTime).TotalMilliseconds >= ActionCooldownMs;

            switch (_currentState)
            {
                case HelperState.Idle:
                    HandleIdleState(now, canAct);
                    break;
                case HelperState.Moving:
                    HandleMovingState(now, canAct);
                    break;
                case HelperState.Attacking:
                    HandleAttackingState(now, canAct);
                    break;
                case HelperState.UsingPotion:
                    HandleUsingPotionState(now, canAct);
                    break;
                case HelperState.UsingSkill:
                    HandleUsingSkillState(now, canAct);
                    break;
                case HelperState.ReturningToTown:
                    HandleReturningToTownState(now, canAct);
                    break;
            }
        }

        private void HandleIdleState(DateTime now, bool canAct)
        {
            // Check if we need to use potions
            if (CheckAndUsePotions(now))
                return;

            // Check if we need to buff
            if (_settings.AutoBuff && CanUseBuff())
            {
                UseNextBuff();
                return;
            }

            // If we have a target, move towards it
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                var distance = Vector2.Distance(_characterState.Position, _currentTarget.Position);
                
                if (distance <= _settings.AttackRange)
                {
                    _currentState = HelperState.Attacking;
                    _stateStartTime = now;
                }
                else
                {
                    _targetPosition = _currentTarget.Position;
                    _currentState = HelperState.Moving;
                    _stateStartTime = now;
                    StartMovement(_targetPosition);
                }
            }
            else if (_settings.AutoMove && _settings.RallyPoint.HasValue)
            {
                // Move to rally point if no target
                var distance = Vector2.Distance(_characterState.Position, _settings.RallyPoint.Value);
                if (distance > _settings.FollowDistance)
                {
                    _targetPosition = _settings.RallyPoint.Value;
                    _currentState = HelperState.Moving;
                    _stateStartTime = now;
                    StartMovement(_targetPosition);
                }
            }
        }

        private void HandleMovingState(DateTime now, bool canAct)
        {
            // Check if we reached the target position
            var distance = Vector2.Distance(_characterState.Position, _targetPosition);
            
            if (distance < 1.0f)
            {
                // Reached destination
                if (_currentTarget != null && _currentTarget.IsAlive)
                {
                    _currentState = HelperState.Attacking;
                    _stateStartTime = now;
                }
                else
                {
                    _currentState = HelperState.Idle;
                }
                return;
            }

            // Check if target is still valid while moving
            if (_currentTarget != null && !_currentTarget.IsAlive)
            {
                _currentTarget = null;
                _currentState = HelperState.Idle;
                StopMovement();
                return;
            }

            // Continue movement if needed
            if (canAct && ShouldContinueMovement())
            {
                ContinueMovement(_targetPosition);
            }
        }

        private void HandleAttackingState(DateTime now, bool canAct)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                _currentTarget = null;
                _currentState = HelperState.Idle;
                return;
            }

            var distance = Vector2.Distance(_characterState.Position, _currentTarget.Position);
            
            // Check if we need to reposition
            if (distance > _settings.AttackRange)
            {
                _targetPosition = _currentTarget.Position;
                _currentState = HelperState.Moving;
                _stateStartTime = now;
                StartMovement(_targetPosition);
                return;
            }

            // Attack if cooldown allows
            if (canAct)
            {
                PerformAttack(_currentTarget);
                _lastActionTime = now;
            }
        }

        private bool CheckAndUsePotions(DateTime now)
        {
            var currentHPPercent = (_characterState.CurrentLife / _characterState.MaxLife) * 100;
            var currentMPPercent = (_characterState.CurrentMana / _characterState.MaxMana) * 100;

            // Use HP potion
            if (_settings.AutoPotionHP && currentHPPercent < _settings.HPPotionThreshold)
            {
                if ((now - _lastHPPotionTime).TotalMilliseconds >= PotionCooldownMs)
                {
                    UseHPPotion();
                    _lastHPPotionTime = now;
                    _currentState = HelperState.UsingPotion;
                    _stateStartTime = now;
                    return true;
                }
            }

            // Use MP potion
            if (_settings.AutoPotionMP && currentMPPercent < _settings.MPPotionThreshold)
            {
                if ((now - _lastMPPotionTime).TotalMilliseconds >= PotionCooldownMs)
                {
                    UseMPPotion();
                    _lastMPPotionTime = now;
                    _currentState = HelperState.UsingPotion;
                    _stateStartTime = now;
                    return true;
                }
            }

            return false;
        }

        private void HandleUsingPotionState(DateTime now, bool canAct)
        {
            // Potion usage is instant, return to idle
            _currentState = HelperState.Idle;
        }

        private void HandleUsingSkillState(DateTime now, bool canAct)
        {
            // Skill usage handled elsewhere
            _currentState = HelperState.Idle;
        }

        private void HandleReturningToTownState(DateTime now, bool canAct)
        {
            // Implementation depends on game mechanics
            // For now, just stay in this state until manual intervention
        }

        #region Action Methods

        private void StartMovement(Vector2 targetPosition)
        {
            // Send movement packet to server
            _networkManager?.SendMoveRequest(_characterState, targetPosition);
        }

        private void ContinueMovement(Vector2 targetPosition)
        {
            // Continue moving towards target
            _networkManager?.SendMoveRequest(_characterState, targetPosition);
        }

        private void StopMovement()
        {
            // Stop movement
            _networkManager?.SendStopMoveRequest(_characterState);
        }

        private bool ShouldContinueMovement()
        {
            // Check if we should continue moving
            return true;
        }

        private void PerformAttack(GameObject target)
        {
            // Send attack packet to server
            _networkManager?.SendAttackRequest(_characterState, target);
        }

        private void UseHPPotion()
        {
            // Find and use HP potion from inventory
            var hpPotion = _characterState.Inventory?.Items
                .FirstOrDefault(item => item?.ItemInfo?.Group == 14 && item?.ItemInfo?.Index == 0); // Apple
                
            if (hpPotion != null)
            {
                _networkManager?.SendUseItemRequest(_characterState, hpPotion);
            }
        }

        private void UseMPPotion()
        {
            // Find and use MP potion from inventory
            var mpPotion = _characterState.Inventory?.Items
                .FirstOrDefault(item => item?.ItemInfo?.Group == 14 && item?.ItemInfo?.Index == 1); // Small Healing Potion
                
            if (mpPotion != null)
            {
                _networkManager?.SendUseItemRequest(_characterState, mpPotion);
            }
        }

        private bool CanUseBuff()
        {
            // Check if buffs need to be applied
            return _settings.BuffSkillIds.Count > 0;
        }

        private void UseNextBuff()
        {
            // Use next buff skill
            if (_settings.BuffSkillIds.Count > 0)
            {
                var buffSkillId = _settings.BuffSkillIds[0];
                _networkManager?.SendSkillCastRequest(_characterState, buffSkillId, _characterState.Position);
            }
        }

        private void StartReturnToTown()
        {
            // Implement return to town logic
            // This could use a teleport skill or command
        }

        #endregion

        #region Public Methods

        public void Toggle()
        {
            _settings.Enabled = !_settings.Enabled;
            if (!_settings.Enabled)
            {
                _currentState = HelperState.Idle;
                _currentTarget = null;
                StopMovement();
            }
        }

        public void SetEnabled(bool enabled)
        {
            _settings.Enabled = enabled;
            if (!enabled)
            {
                _currentState = HelperState.Idle;
                _currentTarget = null;
                StopMovement();
            }
        }

        public void SetTargetPriority(TargetPriority priority)
        {
            _settings.TargetPriority = priority;
        }

        public void SetRallyPoint(Vector2 position)
        {
            _settings.RallyPoint = position;
        }

        public void ClearTarget()
        {
            _currentTarget = null;
        }

        #endregion
    }
}
