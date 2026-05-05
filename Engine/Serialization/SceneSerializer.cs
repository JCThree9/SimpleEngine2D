using Engine.Core;
using Engine.Editor;
using Engine.GameObjects;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Engine.Serialization;

public static class SceneSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void Save(Scene scene, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(filePath, SaveToString(scene));
    }

    public static void Load(string filePath, Scene scene)
    {
        if (!File.Exists(filePath))
            return;

        LoadFromString(File.ReadAllText(filePath), scene);
    }

    public static string SaveToString(Scene scene)
    {
        var sceneData = new SceneData();

        foreach (var gameObject in scene.GameObjects)
        {
            var objectData = new GameObjectData
            {
                Name = gameObject.Name,
                IsActive = gameObject.IsActive,
                Transform = new TransformData
                {
                    PositionX = gameObject.Transform.Position.X,
                    PositionY = gameObject.Transform.Position.Y,
                    Rotation = gameObject.Transform.Rotation,
                    ScaleX = gameObject.Transform.Scale.X,
                    ScaleY = gameObject.Transform.Scale.Y,
                    SizeX = gameObject.Transform.Size.X,
                    SizeY = gameObject.Transform.Size.Y
                }
            };

            foreach (var component in gameObject.Components)
            {
                var componentData = new ComponentData
                {
                    TypeName = component.GetType().AssemblyQualifiedName ??
                               component.GetType().FullName ??
                               component.GetType().Name
                };

                foreach (var property in GetEditorVisibleProperties(component.GetType()))
                {
                    if (!property.CanRead)
                        continue;

                    var value = property.GetValue(component);
                    if (value == null)
                        continue;

                    componentData.Properties[property.Name] = ConvertToString(value);
                }

                objectData.Components.Add(componentData);
            }

            sceneData.GameObjects.Add(objectData);
        }

        return JsonSerializer.Serialize(sceneData, JsonOptions);
    }

    public static void LoadFromString(string json, Scene scene)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        var sceneData = JsonSerializer.Deserialize<SceneData>(json);
        if (sceneData == null)
            return;

        scene.ClearAllGameObjects();

        foreach (var objectData in sceneData.GameObjects)
        {
            var gameObject = new GameObject(objectData.Name)
            {
                IsActive = objectData.IsActive
            };

            gameObject.Transform.Position = new Vector2(objectData.Transform.PositionX, objectData.Transform.PositionY);
            gameObject.Transform.Rotation = objectData.Transform.Rotation;
            gameObject.Transform.Scale = new Vector2(objectData.Transform.ScaleX, objectData.Transform.ScaleY);
            gameObject.Transform.Size = new Vector2(objectData.Transform.SizeX, objectData.Transform.SizeY);

            foreach (var componentData in objectData.Components)
            {
                var componentType = ResolveType(componentData.TypeName);
                if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
                    continue;

                if (Activator.CreateInstance(componentType) is not Component component)
                    continue;

                foreach (var property in GetEditorVisibleProperties(componentType))
                {
                    if (!property.CanWrite || !componentData.Properties.TryGetValue(property.Name, out var value))
                        continue;

                    if (TryParseValue(property.PropertyType, value, out var parsedValue))
                        property.SetValue(component, parsedValue);
                }

                gameObject.AddComponent(component);
            }

            scene.AddGameObject(gameObject);
        }
    }

    private static IEnumerable<PropertyInfo> GetEditorVisibleProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetCustomAttribute<EditorVisibleAttribute>() != null);
    }

    private static string ConvertToString(object value)
    {
        return value switch
        {
            float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            int intValue => intValue.ToString(CultureInfo.InvariantCulture),
            bool boolValue => boolValue.ToString(),
            string stringValue => stringValue,
            Vector2 vectorValue => string.Format(CultureInfo.InvariantCulture, "{0},{1}", vectorValue.X, vectorValue.Y),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static bool TryParseValue(Type type, string value, out object? parsedValue)
    {
        if (type == typeof(float) &&
            float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
        {
            parsedValue = floatValue;
            return true;
        }

        if (type == typeof(int) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            parsedValue = intValue;
            return true;
        }

        if (type == typeof(bool) && bool.TryParse(value, out var boolValue))
        {
            parsedValue = boolValue;
            return true;
        }

        if (type == typeof(string))
        {
            parsedValue = value;
            return true;
        }

        if (type == typeof(Vector2))
        {
            var parts = value.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 &&
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                parsedValue = new Vector2(x, y);
                return true;
            }
        }

        parsedValue = null;
        return false;
    }

    private static Type? ResolveType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        var type = Type.GetType(typeName);
        if (type != null)
            return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }
}
