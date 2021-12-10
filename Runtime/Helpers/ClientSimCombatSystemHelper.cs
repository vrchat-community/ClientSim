using System;
using System.Collections;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim { 

    [AddComponentMenu("")]
    public class ClientSimCombatSystemHelper : ClientSimBehaviour, IVRC_Destructible
    {
        private const string VISUAL_DAMAGE_PREFAB_PATH = "Assets/VRChat Examples/Prefabs/VRCPlayerVisualDamage.prefab";
        
        private VRCPlayerApi player_;
        private ClientSimPlayerController playerController_;
        
        private bool respawnOnDeath_;
        private float respawnTime_ = 5f;
        private Transform respawnPoint_;
        private float maxPlayerHealth_ = 100;
        private float currentHealth_ = 100;
        private bool resetHealthOnRespawn_ = true;
        private GameObject visualDamagePrefab_;
        
        private GameObject visualDamageObj_;
        private VRC_VisualDamage visualDamage_;

        private bool dead_ = false;

        private static ClientSimCombatSystemHelper GetCombatHelper(VRCPlayerApi player)
        {
            GameObject playerObj = player.gameObject;
            if (playerObj == null)
            {
                return null;
            }

            return playerObj.GetComponent<ClientSimCombatSystemHelper>();
        }
        
        public static void CombatSetup(VRCPlayerApi player)
        {
            GameObject playerObj = player.gameObject;
            if (playerObj == null)
            {
                return;
            }

            ClientSimCombatSystemHelper combatHelper = playerObj.GetComponent<ClientSimCombatSystemHelper>();
            if (combatHelper != null)
            {
                return;
            }
            
            combatHelper = player.gameObject.AddComponent<ClientSimCombatSystemHelper>();
            combatHelper.Initialize(player);
        }

        public static void CombatSetMaxHitpoints(VRCPlayerApi player, float maxHealth)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }
            combatHelper.maxPlayerHealth_ = maxHealth;
        }

        public static float CombatGetCurrentHitpoints(VRCPlayerApi player)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                // If a player doesn't have combat setup, their hitpoints are -1.
                return -1;
            }
            return combatHelper.currentHealth_;
        }

        public static void CombatSetRespawn(VRCPlayerApi player, bool respawnOnDeath, float respawnTime, Transform spawnPoint)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }
            
            combatHelper.respawnOnDeath_ = respawnOnDeath;
            combatHelper.respawnTime_ = respawnTime;
            combatHelper.respawnPoint_ = spawnPoint;
        }

        public static void CombatSetDamageGraphic(VRCPlayerApi player, GameObject visualDamage)
        {
            ClientSimCombatSystemHelper combatHelper = GetCombatHelper(player);
            if (combatHelper == null)
            {
                return;
            }

            combatHelper.visualDamagePrefab_ = visualDamage;
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
                float delta = health - combatHelper.currentHealth_;
                if (delta <= 0)
                {
                    combatHelper.ApplyDamage(-delta);
                }
                else
                {
                    combatHelper.ApplyHealing(delta);
                }
            }
            
            combatHelper.currentHealth_ = health;
        }

        private void Initialize(VRCPlayerApi player)
        {
            player_ = player;
            playerController_ = player_.GetPlayerController();
        }
        
        private void Start()
        {
            currentHealth_ = GetMaxHealth();
            CreateVisualDamage();
        }

        private void CreateVisualDamage()
        {
            if (!player_.isLocal)
            {
                return;
            }
            
            if (visualDamageObj_ != null)
            {
                Destroy(visualDamageObj_);
            }

            GameObject damage = visualDamagePrefab_;
#if UNITY_EDITOR
            // If damage prefab is null, try loading it from the sample prefabs
            if (damage == null)
            {
                damage = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(VISUAL_DAMAGE_PREFAB_PATH);
            }
#endif
            if (damage != null) 
            {
                visualDamageObj_ = Instantiate(damage, playerController_.GetCameraProxyTransform());
                visualDamageObj_.transform.localScale = new Vector3(40, 40, 40);
                visualDamageObj_.transform.localPosition = new Vector3(0, 0, 0.5f);
                
				visualDamage_ = visualDamageObj_.GetComponent<VRC_VisualDamage>();
				if (visualDamage_ != null) 
                {
                    // VRChatBug: Visual Damage script is blacklisted in SDK3 and will be destroyed after spawning.
                    DestroyImmediate(visualDamage_);
                }
            }
        }

        public float GetMaxHealth()
        {
            return maxPlayerHealth_;
        }

        public float GetCurrentHealth()
        {
            return currentHealth_;
        }

        private void ApplyVisualDamage()
        {
            if (visualDamage_ != null)
            {
                try
                {
                    visualDamage_.SetDamagePercent(1 - (currentHealth_ / maxPlayerHealth_));
                }
                catch (Exception e)
                {
                    this.LogWarning("Error applying damage: "+ e);
                }
            }
        }

        public void ApplyDamage(float damage)
        {
            if (!player_.isLocal)
            {
                return;
            }
            
            if (currentHealth_ <= 0)
            {
                return;
            }
            this.Log("ApplyDamage: " + damage + " currentHealth: " + currentHealth_);

            currentHealth_ = Mathf.Clamp(currentHealth_ - damage, 0, maxPlayerHealth_);
            ApplyVisualDamage();

            if (currentHealth_ <= 0 && !dead_)
            {
                dead_ = true;
                this.Log("Player Died");
                
                player_.EnablePickups(false);
                if (playerController_)
                {
                    playerController_.PlayerDied();
                }
                
                StartCoroutine(PlayerDied());
            }
        }

        private IEnumerator PlayerDied()
        {
            yield return new WaitForSeconds(respawnTime_);

            RevivePlayer();
        }

        private void RevivePlayer()
        {
            dead_ = false;
            this.Log("Player Revived");
            
            player_.EnablePickups(true);
            
            if (respawnPoint_ != null && respawnOnDeath_)
            {
                if (playerController_)
                {
                    playerController_.Teleport(respawnPoint_, false);
                }
            }
            
            if (resetHealthOnRespawn_)
            {
                ApplyHealing(maxPlayerHealth_);
            }
            
            if (playerController_)
            {
                playerController_.PlayerRevived();
            }
        }

        public void ApplyHealing(float healing)
        {
            if (!player_.isLocal)
            {
                return;
            }
            
            currentHealth_ = Mathf.Clamp(currentHealth_ + healing, 0, maxPlayerHealth_);
            ApplyVisualDamage();

            this.Log("ApplyHealing: " + healing + " currentHealth: "+currentHealth_);
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