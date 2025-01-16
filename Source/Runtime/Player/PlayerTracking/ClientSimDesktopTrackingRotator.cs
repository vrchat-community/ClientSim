using UnityEngine;

namespace VRC.SDK3.ClientSim.PlayerTracking
{
    public class ClientSimDesktopTrackingRotator
    {
        private const float MINIMUM_X_ANGLE = -80f;
        private const float MAXIMUM_X_ANGLE = 70f;
        private const float MINIMUM_Y_ANGLE = -90F;
        private const float MAXIMUM_Y_ANGLE = 90F;
        
        private Quaternion _targetBaseYRot;
        private Quaternion _targetHeadXRot;
        
        private readonly Transform _headXTransform;
        private readonly Transform _headYTransform;

        private IClientSimStation _currentStation;

        public ClientSimDesktopTrackingRotator(Transform headXTransform, Transform headYTransform)
        {
            _headXTransform = headXTransform;
            _headYTransform = headYTransform;

            _targetBaseYRot = _headYTransform.localRotation;
            _targetHeadXRot = _headXTransform.localRotation;
        }

        public void SetStation(IClientSimStation station)
        {
            _currentStation = station;
            _targetBaseYRot = Quaternion.identity;
        }

        public void HandleRotation(float xDelta, float yDelta)
        {
            _targetHeadXRot *= Quaternion.Euler(-yDelta, 0f, 0f);
            _targetHeadXRot = ClampRotationAroundAxis(_targetHeadXRot, 0, MINIMUM_X_ANGLE, MAXIMUM_X_ANGLE);
            
            // Player in a station, allow limited rotation on Y axis.
            if (_currentStation != null && _currentStation.IsLockedInStation())
            {
                _targetBaseYRot *= Quaternion.Euler(0f, xDelta, 0f);
                _targetBaseYRot = ClampRotationAroundAxis(_targetBaseYRot, 1, MINIMUM_Y_ANGLE, MAXIMUM_Y_ANGLE);
            }

            _headXTransform.localRotation = _targetHeadXRot;
            _headYTransform.localRotation = _targetBaseYRot;
        }

        // Used in tests to force rotate the player to look at an object.
        public void LookAtPoint(Vector3 point)
        {
            // Get the amount the player needs to rotate on Y
            Vector3 localizedYPoint = _headYTransform.InverseTransformPoint(point);
            localizedYPoint.y = 0;
            float yAngle = Vector3.SignedAngle(Vector3.forward, localizedYPoint, Vector3.up);
            
            // Get the amount the player needs to rotate on X
            Vector3 localizedXPoint = _headXTransform.InverseTransformPoint(point);
            localizedXPoint.x = 0;
            float xAngle = Vector3.SignedAngle(Vector3.forward, localizedXPoint, Vector3.left);
            
            HandleRotation(yAngle, xAngle);
        }
        
        private static Quaternion ClampRotationAroundAxis(Quaternion q, int index, float minAngle, float maxAngle)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q[index]);

            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            q[index] = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);

            return q;
        }
    }
}