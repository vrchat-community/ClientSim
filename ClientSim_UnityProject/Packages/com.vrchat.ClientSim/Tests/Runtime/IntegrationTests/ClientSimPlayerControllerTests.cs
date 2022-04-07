using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimPlayerControllerTests : ClientSimTestBase
    {
        // TODO
        // Doesn't move through colliders
        // PlayerController handling moving platforms (Currently bugged in VRChat where only rotation is applied)s 
        
        // Current Tests:
        // Move (forward, back, left, right)
        // Don't move while menu is open
        // Immobilize prevents movement (but not jump)
        // Jump
        // crouch/prone
        // Look
        // Don't look while mouse released
        // Don't look while menu is open
        // Movement speed (walk, strafe, run)

        private VRCPlayerApi _localPlayer;
        
        private IEnumerator StartClientSim()
        {
            yield return LoadBasicScene();
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                initializationDelay = 0f
            };

            yield return StartClientSim(settings);

            _localPlayer = Networking.LocalPlayer;
            
            Helper.CloseMenu();
        }
        
        [UnityTest]
        public IEnumerator TestPlayerMove()
        {
            yield return StartClientSim();

            _localPlayer.Immobilize(false);
            
            // Ensure position and rotation match global coordinates.
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            // Test moving forward
            TestInput.SetInputMoveForward(true);

            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    Vector3 pos = _localPlayer.GetPosition();
                    return Mathf.Abs(pos.sqrMagnitude - pos.z * pos.z) < 1e-2f && pos.z > 0.5f;
                }, 
                "Player did not move forward when forward key is pressed!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated without rotation input");
            
            TestInput.SetInputMoveForward(false);
            yield return null;
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            // Test moving backward
            TestInput.SetInputMoveBackward(true);

            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    Vector3 pos = _localPlayer.GetPosition();
                    return Mathf.Abs(pos.sqrMagnitude - pos.z * pos.z) < 1e-2f && pos.z < -0.5f;
                },
                "Player did not move backward when back key is pressed!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated without rotation input");
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            
            // Test moving right
            TestInput.SetInputMoveRight(true);

            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    Vector3 pos = _localPlayer.GetPosition();
                    return Mathf.Abs(pos.sqrMagnitude - pos.x * pos.x) < 1e-2f && pos.x > 0.5f;
                }, 
                "Player did not move right when right key is pressed!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated without rotation input");
            
            TestInput.SetInputMoveRight(false);
            yield return null;
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);

            // Test moving left
            TestInput.SetInputMoveLeft(true);
            
            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    Vector3 pos = _localPlayer.GetPosition();
                    return Mathf.Abs(pos.sqrMagnitude - pos.x * pos.x) < 1e-2f && pos.x < -0.5f;
                },
                "Player did not move left when left key is pressed!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated without rotation input");
            
            TestInput.SetInputMoveLeft(false);
        }
        
        
        [UnityTest]
        public IEnumerator TestPlayerMoveWithMenuOpen()
        {
            yield return StartClientSim();

            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            Helper.OpenMenu();
            yield return null;
            
            
            // Test moving forward
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved forward while the menu was open!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated while the menu was open");
            
            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved backward while the menu was open!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated while the menu was open");
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            
            // Test moving right
            TestInput.SetInputMoveRight(true);
            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved right while the menu was open!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated while the menu was open");
            
            TestInput.SetInputMoveRight(false);
            yield return null;

            // Test moving left
            TestInput.SetInputMoveLeft(true);
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved left while the menu was open!");
            Assert.IsTrue(Quaternion.Angle(_localPlayer.GetRotation(), Quaternion.identity) < 0.1f, "Player rotated while the menu was open");
            
            TestInput.SetInputMoveLeft(false);
        }
        
        
        [UnityTest]
        public IEnumerator TestPlayerMoveWhileImmobilized()
        {
            yield return StartClientSim();
            
            _localPlayer.Immobilize(true);
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);


            // Test moving forward
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved forward while the menu was open!");

            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved backward while the menu was open!");
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            
            // Test moving right
            TestInput.SetInputMoveRight(true);
            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved right while the menu was open!");
            
            TestInput.SetInputMoveRight(false);
            yield return null;

            // Test moving left
            TestInput.SetInputMoveLeft(true);
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(
                Mathf.Approximately(_localPlayer.GetPosition().sqrMagnitude, 0), 
                "Player moved left while the menu was open!");
            
            TestInput.SetInputMoveLeft(false);
        }

        [UnityTest]
        public IEnumerator TestPlayerJump()
        {
            yield return StartClientSim();

            _localPlayer.Immobilize(true);
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);

            Assert.IsTrue(_localPlayer.IsPlayerGrounded(), "Player is not grounded at start.");
            Assert.IsTrue(_localPlayer.GetVelocity().sqrMagnitude < 0.01f, "Player has non zero velocity at start.");

            // Player cannot jump
            _localPlayer.SetJumpImpulse(0);
            TestInput.SetInputJump(true);

            yield return null;
            
            Assert.IsTrue(_localPlayer.IsPlayerGrounded(), "Player is not grounded when jumping with zero jump value.");
            Assert.IsTrue(_localPlayer.GetVelocity().sqrMagnitude < 0.01f, "Player has velocity when jumping with zero jump value.");
            
            yield return null;
            
            Assert.IsTrue(_localPlayer.IsPlayerGrounded(), "Player is not grounded when jumping with zero jump value.");
            Assert.IsTrue(_localPlayer.GetVelocity().sqrMagnitude < 0.01f, "Player has velocity when jumping with zero jump value.");
            
            TestInput.SetInputJump(false);
            
            yield return null;
            
            
            // Player can jump
            _localPlayer.SetJumpImpulse(10);
            
            TestInput.SetInputJump(true);

            yield return ClientSimTestUtils.WaitUntil(
                () => !_localPlayer.IsPlayerGrounded(),
                "Player stayed grounded after jump input!");
            
            Assert.IsTrue(_localPlayer.GetVelocity().sqrMagnitude > 0.01f, "Player velocity is still zero after jumping");
            
            TestInput.SetInputJump(false);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPlayerCrouchProne()
        {
            yield return StartClientSim();

            Vector3 headPosStanding = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            
            // Toggle crouch to go into crouch position.
            TestInput.SetInputToggleCrouch(true);

            yield return null;
            
            Vector3 headPosCrouch = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            
            Assert.IsTrue(headPosStanding.y > headPosCrouch.y, "Player camera did not lower after pressing crouch button.");
            TestInput.SetInputToggleCrouch(false);
            
            yield return null;
            
            // Toggle crouch again to go back to normal height.
            TestInput.SetInputToggleCrouch(true);
            
            yield return null;
            
            Vector3 headPosAfterCrouch = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            
            Assert.IsTrue(headPosAfterCrouch.y > headPosCrouch.y, "Player camera did not raise after pressing crouch button again.");
            Assert.IsTrue(Vector3.Distance(headPosStanding, headPosAfterCrouch) < 0.01f, "Player camera did not go back to the same height after toggling crouch off.");
            TestInput.SetInputToggleCrouch(false);
            
            yield return null;
            
            
            // Toggle Prone to go into crouch position.
            TestInput.SetInputToggleProne(true);

            yield return null;
            
            Vector3 headPosProne = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            
            Assert.IsTrue(headPosStanding.y > headPosProne.y, "Player camera did not lower after pressing crouch button.");
            Assert.IsTrue(headPosCrouch.y > headPosProne.y, "Player prone height is not lower that player crouch height.");
            TestInput.SetInputToggleProne(false);
            
            yield return null;
            
            // Toggle prone again to go back to normal height.
            TestInput.SetInputToggleProne(true);
            
            yield return null;
            
            Vector3 headPosAfterProne = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            
            Assert.IsTrue(headPosAfterProne.y > headPosProne.y, "Player camera did not raise after pressing prone button again.");
            Assert.IsTrue(Vector3.Distance(headPosStanding, headPosAfterProne) < 0.01f, "Player camera did not go back to the same height after toggling prone off.");
            TestInput.SetInputToggleProne(false);
            
            
            yield return null;
            
            // Verify that going from crouch to prone works
            TestInput.SetInputToggleCrouch(true);
            
            yield return null;
            
            TestInput.SetInputToggleCrouch(false);
            Vector3 headPosCrouch2 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Assert.IsTrue(Vector3.Distance(headPosCrouch, headPosCrouch2) < 0.01f, "Player camera did not go back to the same crouch height.");

            // Go prone from crouch position.
            TestInput.SetInputToggleProne(true);
            
            yield return null;
            
            Vector3 headPosProne2 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Assert.IsTrue(Vector3.Distance(headPosProne, headPosProne2) < 0.01f, "Player camera did not go back to the same prone height.");
            
            TestInput.SetInputToggleProne(false);
            
            yield return null;
            
            // Toggle prone again to go back to standing height.
            TestInput.SetInputToggleProne(true);
            
            yield return null;
            
            TestInput.SetInputToggleProne(false);
            
            Vector3 headPosStanding2 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Assert.IsTrue(Vector3.Distance(headPosStanding, headPosStanding2) < 0.01f, "Player camera did not go back to the same height after toggling prone off.");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPlayerLook()
        {
            yield return StartClientSim();
            
            // Ensure position and rotation match global coordinates.
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            Quaternion playerRotation = _localPlayer.GetRotation();
            Assert.IsTrue(Quaternion.Angle(playerRotation, Quaternion.identity) < 0.01f, "Player initial rotation is not identity.");

            Quaternion playerHeadRotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation, Quaternion.identity) < 0.01f, "Player initial head rotation is not identity.");
            
            TestInput.SetInputLookDelta(new Vector2(100, 0));
            
            yield return new WaitForSeconds(0.2f);
            
            Quaternion playerRotation2 = _localPlayer.GetRotation();
            Quaternion playerHeadRotation2 = Quaternion.Inverse(playerRotation2) * _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            
            Assert.IsTrue(Quaternion.Angle(playerRotation, playerRotation2) > 0.2f, "Player did not rotate after updating look input.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation2, Quaternion.identity) < 0.01f, "Player head rotation is not identity after only rotating on y axis.");
            
            TestInput.SetInputLookDelta(new Vector2(-100, 0));
            
            yield return new WaitForSeconds(0.2f);
            
            Quaternion playerRotation3 = _localPlayer.GetRotation();
            Quaternion playerHeadRotation3 = Quaternion.Inverse(playerRotation3) * _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            
            Assert.IsTrue(Quaternion.Angle(playerRotation2, playerRotation3) > 0.2f, "Player did not rotate after updating look input.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation3, Quaternion.identity) < 0.01f, "Player head rotation is not identity after only rotating on y axis.");

            
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            playerHeadRotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;

            TestInput.SetInputLookDelta(new Vector2(0, 100));
            yield return new WaitForSeconds(0.2f);
            
            playerRotation = _localPlayer.GetRotation();
            Quaternion playerHeadRotation4 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Assert.IsTrue(Quaternion.Angle(playerRotation, Quaternion.identity) < 0.01f, "Player controller rotation is not identity after only rotating on x axis.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation, playerHeadRotation4) > 0.2f, "Player head did not rotate after updating look input.");
            
            TestInput.SetInputLookDelta(new Vector2(0, -100));
            yield return new WaitForSeconds(0.2f);
            
            playerRotation = _localPlayer.GetRotation();
            Quaternion playerHeadRotation5 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Assert.IsTrue(Quaternion.Angle(playerRotation, Quaternion.identity) < 0.01f, "Player controller rotation is not identity after only rotating on x axis.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation, playerHeadRotation5) > 0.2f, "Player head did not rotate after updating look input.");
        }
        
        [UnityTest]
        public IEnumerator TestPlayerLookMouseReleased()
        {
            yield return StartClientSim();
            
            // Ensure position and rotation match global coordinates.
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            Quaternion playerRotation = _localPlayer.GetRotation();
            Assert.IsTrue(Quaternion.Angle(playerRotation, Quaternion.identity) < 0.01f, "Player initial rotation is not identity.");

            Quaternion playerHeadRotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation, Quaternion.identity) < 0.01f, "Player initial head rotation is not identity.");
            
            TestInput.SetInputReleaseMouse(true);
            yield return null;
            
            TestInput.SetInputLookDelta(new Vector2(100, 100));
            
            yield return new WaitForSeconds(0.2f);
            
            Quaternion playerRotation2 = _localPlayer.GetRotation();
            Quaternion playerHeadRotation2 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            
            Assert.IsTrue(Quaternion.Angle(playerRotation2, Quaternion.identity) < 0.01f, "Player rotated while mouse is released.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation2, Quaternion.identity) < 0.01f, "Player head rotated while mouse is released.");
        }
        
        [UnityTest]
        public IEnumerator TestPlayerLookMenuOpen()
        {
            yield return StartClientSim();
            
            // Ensure position and rotation match global coordinates.
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            Quaternion playerRotation = _localPlayer.GetRotation();
            Assert.IsTrue(Quaternion.Angle(playerRotation, Quaternion.identity) < 0.01f, "Player initial rotation is not identity.");

            Quaternion playerHeadRotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation, Quaternion.identity) < 0.01f, "Player initial head rotation is not identity.");
            
            Helper.OpenMenu();
            
            yield return null;
            
            TestInput.SetInputLookDelta(new Vector2(100, 100));
            
            yield return new WaitForSeconds(0.2f);
            
            Quaternion playerRotation2 = _localPlayer.GetRotation();
            Quaternion playerHeadRotation2 = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            
            Assert.IsTrue(Quaternion.Angle(playerRotation2, Quaternion.identity) < 0.01f, "Player rotated while mouse is released.");
            Assert.IsTrue(Quaternion.Angle(playerHeadRotation2, Quaternion.identity) < 0.01f, "Player head rotated while mouse is released.");
        }
        
        
        [UnityTest]
        public IEnumerator TestPlayerMovementSpeed()
        {
            yield return StartClientSim();

            bool IsRatioWithinRange(float ratio, float expected, float tolerance = 0.05f)
            {
                return expected - tolerance <= ratio && ratio <= expected + tolerance;
            }

            float waitDuration = 0.3f;
            
            // Set player to default speeds
            _localPlayer.SetWalkSpeed(2);
            _localPlayer.SetStrafeSpeed(2);
            _localPlayer.SetRunSpeed(4);

            yield return null;
            
            // Move in all directions and save final velocity to be compared after increasing move speeds.
            
            // Test moving forward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerForwardVel = _localPlayer.GetVelocity();
            
            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerBackwardVel = _localPlayer.GetVelocity();
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            
            // Test moving right
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveRight(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerRightVel = _localPlayer.GetVelocity();

            TestInput.SetInputMoveRight(false);
            yield return null;

            // Test moving left
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveLeft(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerLeftVel = _localPlayer.GetVelocity();

            TestInput.SetInputMoveLeft(false);
            
            yield return null;
            
            TestInput.SetInputRun(true);
            
            yield return null;
            
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;

            // Test moving forward
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerForwardVelRun = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerForwardVel, playerForwardVelRun) < 1f, "Player forward and run are different directions.");
            Assert.IsTrue(playerForwardVel.z > 0 && playerForwardVel.z < playerForwardVelRun.z, "Player walk velocity is not closer to zero than player run velocity.");
            Assert.IsTrue(IsRatioWithinRange(playerForwardVelRun.z / playerForwardVel.z, 2), "Player run velocity is not expected ratio compared to player walk velocity");
            
            
            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerBackwardVelRun = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerBackwardVel, playerBackwardVelRun) < 1f, "Player forward and run are different directions.");
            Assert.IsTrue(playerBackwardVel.z < 0 && playerBackwardVel.z > playerBackwardVelRun.z, "Player walk velocity is not closer to zero than player run velocity.");
            Assert.IsTrue(IsRatioWithinRange(playerBackwardVelRun.z / playerBackwardVel.z, 2), "Player run velocity is not expected ratio compared to player walk velocity");
            
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            TestInput.SetInputRun(false);
            
            yield return null;


            float multiplier = 4;
            _localPlayer.SetWalkSpeed(_localPlayer.GetWalkSpeed() * multiplier);
            _localPlayer.SetStrafeSpeed(_localPlayer.GetStrafeSpeed() * multiplier);
            _localPlayer.SetRunSpeed(_localPlayer.GetRunSpeed() * multiplier);
            
            
            
            
            // Test moving forward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerForwardVel2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerForwardVel, playerForwardVel2) < 1f, "Increasing player walk speed changed velocity direction");
            Assert.IsTrue(playerForwardVel.z > 0 && playerForwardVel.z < playerForwardVel2.z, "Increasing player walk speed decreased player walk velocity");
            Assert.IsTrue(IsRatioWithinRange(playerForwardVel2.z / playerForwardVel.z, multiplier), "Increasing player walk speed did not change by expected ratio");

            
            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerBackwardVel2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerBackwardVel, playerBackwardVel2) < 1f, "Increasing player walk speed changed velocity direction");
            Assert.IsTrue(playerBackwardVel.z < 0 && playerBackwardVel.z > playerBackwardVel2.z, "Increasing player walk speed decreased player walk velocity");
            Assert.IsTrue(IsRatioWithinRange(playerBackwardVel2.z / playerBackwardVel.z, multiplier), "Increasing player walk speed did not change by expected ratio");

            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            
            // Test moving right
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveRight(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerRightVel2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerRightVel, playerRightVel2) < 1f, "Increasing player strafe speed changed velocity direction");
            Assert.IsTrue(playerRightVel.x > 0 && playerRightVel.x < playerRightVel2.x, "Increasing player strafe speed decreased player strafe velocity");
            Assert.IsTrue(IsRatioWithinRange(playerRightVel2.x / playerRightVel.x, multiplier), "Increasing player strafe speed did not change by expected ratio");

            
            TestInput.SetInputMoveRight(false);
            yield return null;

            // Test moving left
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveLeft(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerLeftVel2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerLeftVel, playerLeftVel2) < 1f, "Increasing player strafe speed changed velocity direction");
            Assert.IsTrue(playerLeftVel.x < 0 && playerLeftVel.x > playerLeftVel2.x, "Increasing player strafe speed decreased player strafe velocity");
            Assert.IsTrue(IsRatioWithinRange(playerLeftVel2.x / playerLeftVel.x, multiplier), "Increasing player strafe speed did not change by expected ratio");


            TestInput.SetInputMoveLeft(false);
            
            yield return null;
            
            TestInput.SetInputRun(true);
            
            yield return null;
            
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;

            // Test moving forward
            TestInput.SetInputMoveForward(true);
            yield return new WaitForSeconds(waitDuration);

            Vector3 playerForwardVelRun2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerForwardVelRun, playerForwardVelRun2) < 1f, "Increasing player run speed changed velocity direction");
            Assert.IsTrue(playerForwardVelRun.z > 0 && playerForwardVelRun.z < playerForwardVelRun2.z, "Increasing player run speed decreased player run velocity");
            Assert.IsTrue(IsRatioWithinRange(playerForwardVelRun2.z / playerForwardVelRun.z, multiplier), "Increasing player run speed did not change by expected ratio");

            
            TestInput.SetInputMoveForward(false);
            yield return null;
            
            // Test moving backward
            _localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            _localPlayer.SetVelocity(Vector3.zero);
            yield return null;
            TestInput.SetInputMoveBackward(true);
            yield return new WaitForSeconds(waitDuration);
            
            Vector3 playerBackwardVelRun2 = _localPlayer.GetVelocity();
            Assert.IsTrue(Vector3.Angle(playerBackwardVelRun, playerBackwardVelRun2) < 1f, "Increasing player run speed changed velocity direction");
            Assert.IsTrue(playerBackwardVelRun.z < 0 && playerBackwardVelRun.z > playerBackwardVelRun2.z, "Increasing player run speed decreased player run velocity");
            Assert.IsTrue(IsRatioWithinRange(playerBackwardVelRun2.z / playerBackwardVelRun.z, multiplier), "Increasing player run speed did not change by expected ratio");
            
            
            TestInput.SetInputMoveBackward(false);
            yield return null;
            
            TestInput.SetInputRun(false);
            
            yield return null;
        }
    }
}