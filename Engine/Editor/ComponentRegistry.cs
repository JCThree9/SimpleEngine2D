using Engine.GameObjects;
using System.Reflection;

namespace Engine.Editor;

public static class ComponentRegistry
{
    private static readonly List<Type> _availableComponents = new();
    private static bool _initialized;

    public static IReadOnlyList<Type> AvailableComponents => _availableComponents;

    public static void Initialize()
    {
        if (_initialized)
            return;

        var componentType = typeof(Component);
        var types = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in GetTypesSafely(assembly))
            {
                if (!componentType.IsAssignableFrom(type) ||
                    type.IsAbstract ||
                    type.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                types.Add(type);
            }
        }

        _availableComponents.Clear();
        _availableComponents.AddRange(types
            .Distinct()
            .OrderBy(type => type.Name));
        _initialized = true;
    }

    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }
}
