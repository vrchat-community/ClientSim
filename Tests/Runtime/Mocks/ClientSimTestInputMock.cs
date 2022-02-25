
namespace VRC.SDK3.ClientSim.Tests
{
    public class ClientSimTestInputMock : ClientSimInputBase
    {
        private float _movementHorizontal = 0;
        private float _movementVertical = 0;
        private float _lookHorizontal = 0;
        private float _lookVertical = 0;
        private float _pickupRotateUpDown = 0;
        private float _pickupRotateLeftRight = 0;
        private float _pickupRotateCwCcw = 0;
        private float _pickupManipulateDistance = 0; 
        
        public void SetMovementHorizontal(float value)
        {
            _movementHorizontal = value;
        }
        
        public void SetMovementVertical(float value)
        {
            _movementVertical = value;
        }

        public void SetLookHorizontal(float value)
        {
            _lookHorizontal = value;
        }

        public void SetLookVertical(float value)
        {
            _lookVertical = value;
        }

        public void SetPickupRotateUpDown(float value)
        {
            _pickupRotateUpDown = value;
        }

        public void SetPickupRotateLeftRight(float value)
        {
            _pickupRotateLeftRight = value;
        }

        public void SetPickupRotateCwCcw(float value)
        {
            _pickupRotateCwCcw = value;
        }

        public void SetPickupManipulateDistance(float value)
        {
            _pickupManipulateDistance = value;
        }
        
        
        public override float GetMovementHorizontal()
        {
            return _movementHorizontal;
        }

        public override float GetMovementVertical()
        {
            return _movementVertical;
        }

        public override float GetLookHorizontal()
        {
            return _lookHorizontal;
        }

        public override float GetLookVertical()
        {
            return _lookVertical;
        }

        public override float GetPickupRotateUpDown()
        {
            return _pickupRotateUpDown;
        }

        public override float GetPickupRotateLeftRight()
        {
            return _pickupRotateLeftRight;
        }

        public override float GetPickupRotateCwCcw()
        {
            return _pickupRotateCwCcw;
        }

        public override float GetPickupManipulateDistance()
        {
            return _pickupManipulateDistance;
        }
    }
}