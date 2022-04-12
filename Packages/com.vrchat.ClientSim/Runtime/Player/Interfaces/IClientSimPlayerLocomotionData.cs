namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerLocomotionData
    {
        float GetJump();
        void SetJump(float jump);
        float GetRunSpeed();
        void SetRunSpeed(float runSpeed);
        float GetWalkSpeed();
        void SetWalkSpeed(float walkSpeed);
        float GetStrafeSpeed();
        void SetStrafeSpeed(float strafeSpeed);
        float GetGravityStrength();
        void SetGravityStrength(float gravity);
        bool GetImmobilized();
        void SetImmobilized(bool immobilize);
        void SetUseLegacyLocomotion(bool useLegacyLocomotion);
        bool GetUseLegacyLocomotion();
    }
}