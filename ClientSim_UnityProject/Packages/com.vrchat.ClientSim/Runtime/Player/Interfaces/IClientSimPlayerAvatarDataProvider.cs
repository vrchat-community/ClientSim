using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerAvatarDataProvider
    {
        Transform GetBoneTransform(HumanBodyBones bone);
        Quaternion GetBoneRotation(HumanBodyBones bone);
        Vector3 GetBonePosition(HumanBodyBones bone);
    }
}