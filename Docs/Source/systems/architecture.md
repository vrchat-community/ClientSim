# Architecture

The architecture of ClientSim has a focus on small components with an event based observer pattern, mixed with manual dependency injection where each class is initialized only with the dependencies it needs. The included player controller is based on generic dependency providers, which allows for the eventual extension to using VR without rewriting the core systems.

## Observer Pattern

ClientSim uses the observer pattern to send events within the system that anything can listen to without knowing what handles them. Events help decouple the different systems, improving testability as one system does not need to directly depend on another just to send messages when something happens. See [EventDispatcher](runtime/event-dispatcher.md) for specific details.

## Dependency Injection

ClientSim’s architecture uses a manually-handled dependency injection. On creation of a system, all dependencies are passed to it, either through its constructor or through an initialization method. Dependencies are structured as providers, and must extend an interface that declares what methods it provides. When a class needs a specific item, it depends on the provider interface instead of the class that implements it. This allows for different implementations of the provider without the dependent code needing to change. The provider pattern allows for dependencies to easily be mocked in tests.