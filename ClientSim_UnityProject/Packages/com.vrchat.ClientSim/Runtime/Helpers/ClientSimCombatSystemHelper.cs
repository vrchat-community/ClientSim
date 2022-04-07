using System;
using System.Collections;
using System.IO;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim 
{
    // Sends Events:
    // - ClientSimPlayerDeathStatusChangedEvent
    [AddComponentMenu("")]
    public class ClientSimCombatSystemHelper : ClientSimBehaviour, IVRC_Destructible
    {
        private static readonly string _visualDamagePrefabPath = 
            Path.Combine("Assets", "VRChat Examples", "Prefabs", "VRCPlayerVisualDamage.prefab");
        
        private VRCPlayerApi _player;
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimProxyObjectProvider _proxyProvider;
        private ClientSimPlayerController _playerController;
        
        // Make values public so users can see and modify these values at runtime.
        public bool respawnOnDeath;
        public bool resetHealthOnRespawn = true;
        public float respawnTime = 5f;
        public float maxPlayerHealth = 100;
        public float currentHealth = 100;
        public Transform respawnPoint;
        private GameObject _visualDamagePrefab;
        
        private GameObject _visualDamageObj;
        private VRC_VisualDamage _visualDamage;

        private bool _dead = false;

        private static ClientSimCombatSystemHelper GetCombatHelper(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().GetCombatHelper();
        }
        
        public static void CombatSetup(VRCPlayerApi player)
        {
            player.GetClientSimPlayer().InitializeCombat();
        }

        public static void CombatSetMaxHitpoints(VRCPlayerApi player, float maxHealth)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }
            combatHelper.maxPlayerHealth = maxHealth;
        }

        public static float CombatGetCurrentHitpoints(VRCPlayerApi player)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                // If a player doesn't have combat setup, their hitpoints are -1.
                return -1;
            }
            return combatHelper.currentHealth;
        }

        public static void CombatSetRespawn(VRCPlayerApi player, bool respawnOnDeath, float respawnTime, Transform spawnPoint)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }
            
            combatHelper.respawnOnDeath = respawnOnDeath;
            combatHelper.respawnTime = respawnTime;
            combatHelper.respawnPoint = spawnPoint;
        }

        public static void CombatSetDamageGraphic(VRCPlayerApi player, GameObject visualDamage)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }

            combatHelper._visualDamagePrefab = visualDamage;
        }

        public static IVRC_Destructible CombatGetDestructible(VRCPlayerApi player)
        {
            return GetCombatHelper(player);
        }

        public static void CombatSetCurrentHitpoints(VRCPlayerApi player, float health)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }

            if (player.isLocal)
            {
                float delta = health - combatHelper.currentHealth;
                if (delta <= 0)
                {
                    combatHelper.ApplyDamage(-delta);
                }
                else
                {
                    combatHelper.ApplyHealing(delta);
                }
            }
            
            combatHelper.currentHealth = health;
        }

        public void Initialize(
            VRCPlayerApi player, 
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimProxyObjectProvider proxyProvider, 
            ClientSimPlayerController playerController)
        {
            _player = player;
            
            // Values below will be null for remote players.
            _eventDispatcher = eventDispatcher;
            _playerController = playerController;
            _proxyProvider = proxyProvider;
            
            // TODO add ragdoll to player avatar
        }
        
        private void Start()
        {
            currentHealth = GetMaxHealth();
            CreateVisualDamage();
        }

        private void CreateVisualDamage()
        {
            if (!_player.isLocal)
            {
                return;
            }
            
            if (_visualDamageObj != null)
            {
                Destroy(_visualDamageObj);
            }

            GameObject damage = _visualDamagePrefab;
#if UNITY_EDITOR
            // If damage prefab is null, try loading it from the sample prefabs
            if (damage == null)
            {
                damage = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(_visualDamagePrefabPath);
            }
#endif
            if (damage != null) 
            {
                _visualDamageObj = Instantiate(damage, _proxyProvider.CameraProxy().transform);
                _visualDamageObj.transform.localScale = new Vector3(40, 40, 40);
                _visualDamageObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                
				_visualDamage = _visualDamageObj.GetComponent<VRC_VisualDamage>();
				if (_visualDamage != null) 
                {
                    // VRChatBug: Visual Damage script is blacklisted in SDK3 and will be destroyed after spawning.
                    DestroyImmediate(_visualDamage);
                }
            }
        }

        public float GetMaxHealth()
        {
            return maxPlayerHealth;
        }

        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        private void ApplyVisualDamage()
        {
            if (_visualDamage != null)
            {
                try
                {
                    _visualDamage.SetDamagePercent(1 - (currentHealth / maxPlayerHealth));
                }
                catch (Exception e)
                {
                    this.LogWarning($"Error applying damage: {e}");
                }
            }
        }

        public void ApplyDamage(float damage)
        {
            if (!_player.isLocal)
            {
                return;
            }
            
            if (currentHealth <= 0)
            {
                return;
            }
            this.Log($"ApplyDamage: {damage} currentHealth: {currentHealth}");

            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxPlayerHealth);
            ApplyVisualDamage();

            if (currentHealth <= 0 && !_dead)
            {
                _dead = true;
                this.Log("Player Died");
                
                _player.EnablePickups(false);
                _eventDispatcher?.SendEvent(new ClientSimPlayerDeathStatusChangedEvent
                {
                    player = _player,
                    isDead = true,
                });

                StartCoroutine(PlayerDied());
            }
        }

        private IEnumerator PlayerDied()
        {
            yield return new WaitForSeconds(respawnTime);

            RevivePlayer();
        }

        private void RevivePlayer()
        {
            if (!_player.isLocal)
            {
                return;
            }
            
            _dead = false;
            this.Log("Player Revived");
            
            _player.EnablePickups(true);
            
            if (respawnPoint != null && respawnOnDeath)
            {
                _playerController.Teleport(respawnPoint, false);
            }
            
            if (resetHealthOnRespawn)
            {
                ApplyHealing(maxPlayerHealth);
            }
            
            _eventDispatcher?.SendEvent(new ClientSimPlayerDeathStatusChangedEvent
            {
                player = _player,
                isDead = false,
            });
        }

        public void ApplyHealing(float healing)
        {
            if (!_player.isLocal)
            {
                return;
            }
            
            currentHealth = Mathf.Clamp(currentHealth + healing, 0, maxPlayerHealth);
            ApplyVisualDamage();

            this.Log($"ApplyHealing: {healing} currentHealth: {currentHealth}");
        }

        public object[] GetState()
        {
            throw new System.NotImplementedException();
        }

        public void SetState(object[] state)
        {
            throw new System.NotImplementedException();
        }
    }
}