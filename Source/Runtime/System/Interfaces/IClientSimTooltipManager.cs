
namespace VRC.SDK3.ClientSim
{
    public interface IClientSimTooltipManager
    {
        void DisplayTooltip(IClientSimInteractable interact);
        void DisableTooltip(IClientSimInteractable interact);
    }
}