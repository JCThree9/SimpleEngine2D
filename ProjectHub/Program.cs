using Engine.Core;
using ProjectHub.Scenes;

// Create the engine for the hub
using var game = new EngineGame(1280, 720, "SimpleEngine2D - Project Hub");

// Push the hub scene
game.Scenes.Push(new HubScene());

// Run!
game.Run();
