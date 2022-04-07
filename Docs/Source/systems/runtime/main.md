# Client Sim Main

ClientSimMain is the central point of ClientSim that handles initialization and destruction of ClientSim. It is contained in the ClientSimSystem prefab. On creation, all core systems will be initialized with their dependencies. This system also maintains all the implementations of the VRCSDK hooks to link VRC components to the ClientSim Helpers. ClientSimMain is a singleton to ensure only one instance is running at a time and to help easily pass information from Editor Windows and Tests. None of the runtime systems within ClientSim depend on ClientSimMain being a singleton.

![ClientSimSystem Hierarchy](/images/client-sim-main-hierarchy.png)