namespace VRC.SDK3.ClientSim
{
    public class ClientSimMenuStateChangedEvent : IClientSimEvent
    {
        public bool isMenuOpen;
    }
    
    public class ClientSimMenuRespawnClickedEvent : IClientSimEvent { }
}