namespace VRC.SDK3.ClientSim
{
    public interface IClientSimInteractable
    {
        float GetProximity();
        bool CanInteract();
        string GetInteractText();
        void Interact();
    }
}