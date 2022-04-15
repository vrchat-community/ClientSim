using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests
{
    public class ClientSimTestInputListener
    {
        public bool jump = false;
        public HandType jumpHandType;
        public bool use = false;
        public HandType useHandType;
        public bool grab = false;
        public HandType grabHandType;
        public bool drop = false;
        public HandType dropHandType;
        public bool toggleMenu = false;
        public HandType toggleMenuHandType;

        public bool run = false;
        public bool toggleCrouch = false;
        public bool toggleProne = false;
        public bool releaseMouse = false;

        // Easy way to know the number of buttons enabled.
        public int CountTrue()
        {
            return 
                (jump ? 1 : 0)
                + (use ? 1 : 0)
                + (grab ? 1 : 0)
                + (drop ? 1 : 0)
                + (toggleMenu ? 1 : 0)
                + (run ? 1 : 0)
                + (toggleCrouch ? 1 : 0)
                + (toggleProne ? 1 : 0)
                + (releaseMouse ? 1 : 0);
        }

        public bool AllOff()
        {
            return CountTrue() == 0;
        }
        
        public void Subscribe(IClientSimInput input)
        {
            input.SubscribeJump(JumpInput);
            input.SubscribeUse(UseInput);
            input.SubscribeGrab(GrabInput);
            input.SubscribeDrop(DropInput);
            input.SubscribeToggleMenu(ToggleMenuInput);
            
            input.SubscribeRun(RunInput);
            input.SubscribeToggleCrouch(ToggleCrouchInput);
            input.SubscribeToggleProne(ToggleProneInput);
            input.SubscribeReleaseMouse(ReleaseMouseInput);
        }

        public void Unsubscribe(IClientSimInput input)
        {
            input.UnsubscribeJump(JumpInput);
            input.UnsubscribeUse(UseInput);
            input.UnsubscribeGrab(GrabInput);
            input.UnsubscribeDrop(DropInput);
            input.UnsubscribeToggleMenu(ToggleMenuInput);
            
            input.UnsubscribeRun(RunInput);
            input.UnsubscribeToggleCrouch(ToggleCrouchInput);
            input.UnsubscribeToggleProne(ToggleProneInput);
            input.UnsubscribeReleaseMouse(ReleaseMouseInput);
        }

        private void JumpInput(bool value, HandType hand)
        {
            jump = value;
            jumpHandType = hand;
        }
        
        private void UseInput(bool value, HandType hand)
        {
            use = value;
            useHandType = hand;
        }
        
        private void GrabInput(bool value, HandType hand)
        {
            grab = value;
            grabHandType = hand;
        }
        
        private void DropInput(bool value, HandType hand)
        {
            drop = value;
            dropHandType = hand;
        }
        
        private void ToggleMenuInput(bool value, HandType hand)
        {
            toggleMenu = value;
            toggleMenuHandType = hand;
        }

        private void RunInput(bool value)
        {
            run = value;
        }

        private void ToggleCrouchInput(bool value)
        {
            toggleCrouch = value;
        }

        private void ToggleProneInput(bool value)
        {
            toggleProne = value;
        }

        private void ReleaseMouseInput(bool value)
        {
            releaseMouse = value;
        }
    }
}