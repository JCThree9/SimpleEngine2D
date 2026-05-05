namespace Engine.Serialization;

public class SceneData
{
    public List<GameObjectData> GameObjects { get; set; } = new();
}

public class GameObjectData
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public TransformData Transform { get; set; } = new();
    public List<ComponentData> Components { get; set; } = new();
}

public class TransformData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Rotation { get; set; }
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;
    public float SizeX { get; set; } = 40f;
    public float SizeY { get; set; } = 40f;
}

public class ComponentData
{
    public string TypeName { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new();
}
