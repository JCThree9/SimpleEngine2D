using Engine.Core;
using SampleGame.Scenes;

// Create the engine with a 1280x720 window
using var game = new EngineGame(1280, 720, "Sample Game");

// Push the initial scene
game.Scenes.Push(new GameScene());

// Run!
game.Run();
