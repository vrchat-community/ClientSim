using System;

namespace VRC.SDK3.ClientSim
{
    [Serializable]
    public class ClientSimPlayerLocomotionData : IClientSimPlayerLocomotionData
    {
        private const float DEFAULT_RUN_SPEED = 4;
        private const float DEFAULT_WALK_SPEED = 2;

        public float walkSpeed = DEFAULT_WALK_SPEED;
        public float strafeSpeed = DEFAULT_WALK_SPEED;
        public float runSpeed = DEFAULT_RUN_SPEED;
        public float jumpSpeed;
        public float gravityStrength = 1f;
        public bool immobilized = false;
        public bool useLegacyLocomotion = false;
        
        public float GetJump()
        {
            return jumpSpeed;
        }

        public void SetJump(float value)
        {
            jumpSpeed = value;
        }

        public float GetRunSpeed()
        {
            return runSpeed;
        }

        public void SetRunSpeed(float value)
        {
            runSpeed = value;
        }

        public float GetWalkSpeed()
        {
            return walkSpeed;
        }

        public void SetWalkSpeed(float value)
        {
            walkSpeed = value;
        }

        public float GetStrafeSpeed()
        {
            return strafeSpeed;
        }

        public void SetStrafeSpeed(float value)
        {
            strafeSpeed = value;
        }
        
        public float GetGravityStrength()
        {
            return gravityStrength;
        }

        public void SetGravityStrength(float value)
        {
            gravityStrength = value;
        }

        public bool GetImmobilized()
        {
            return immobilized;
        }
        
        public void SetImmobilized(bool value)
        {
            immobilized = value;
        }

        public void SetUseLegacyLocomotion(bool value)
        {
            useLegacyLocomotion = value;
        }

        public bool GetUseLegacyLocomotion()
        {
            return useLegacyLocomotion;
        }
    }
}