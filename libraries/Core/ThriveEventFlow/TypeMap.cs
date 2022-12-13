using System.Reflection;
using JetBrains.Annotations;
using static ThriveEventFlow.Diagnostics.ThriveEventFlowEventSource;

// ReSharper disable InvertIf

namespace ThriveEventFlow;


public static class TypeMap {
    public static readonly TypeMapper Instance = new();

    public static string GetTypeName(object o, bool fail = true) => Instance.GetTypeName(o, fail);

   
    public static void RegisterKnownEventTypes(params Assembly[] assemblies)
        => Instance.RegisterKnownEventTypes(assemblies);
}

public class TypeMapper {
    readonly Dictionary<string, Type> _reverseMap = new();
    readonly Dictionary<Type, string> _map        = new();

    public IReadOnlyDictionary<Type, string> Map        => _map;
    public IReadOnlyDictionary<string, Type> ReverseMap => _reverseMap;

    [PublicAPI]
    public string GetTypeName<T>() {
        if (!_map.TryGetValue(typeof(T), out var name)) {
            Log.TypeNotMappedToName(typeof(T));
            throw new UnregisteredTypeException(typeof(T));
        }

        return name;
    }

    public string GetTypeName(object o, bool fail = true) {
        if (_map.TryGetValue(o.GetType(), out var name)) return name;

        if (!fail) return "unknown";

        Log.TypeNotMappedToName(o.GetType());
        throw new UnregisteredTypeException(o.GetType());
    }

    [PublicAPI]
    public string GetTypeNameByType(Type type) {
        if (!_map.TryGetValue(type, out var name)) {
            Log.TypeNotMappedToName(type);
            throw new UnregisteredTypeException(type);
        }

        return name;
    }

    public Type GetType(string typeName) {
        if (!_reverseMap.TryGetValue(typeName, out var type)) {
            Log.TypeNameNotMappedToType(typeName);
            throw new UnregisteredTypeException(typeName);
        }

        return type;
    }

    public bool TryGetType(string typeName, out Type? type) => _reverseMap.TryGetValue(typeName, out type);

    public void AddType<T>(string name) => AddType(typeof(T), name);

    internal void AddType(Type type, string name) {
        _reverseMap[name] = type;
        _map[type]        = name;
        Log.TypeMapRegistered(type.Name, name);
    }

    public void RemoveType<T>() {
        var name = GetTypeName<T>();
        _reverseMap.Remove(name);
        _map.Remove(typeof(T));
    }

    public bool IsTypeRegistered<T>() => _map.ContainsKey(typeof(T));

    public void RegisterKnownEventTypes(params Assembly[] assembliesWithEvents) {
        var assembliesToScan = assembliesWithEvents.Length == 0
            ? GetDefaultAssemblies() : assembliesWithEvents;

        foreach (var assembly in assembliesToScan) {
            RegisterAssemblyEventTypes(assembly);
        }

        Assembly[] GetDefaultAssemblies() {
            var firstLevel = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && NamePredicate(x.GetName()))
                .ToArray();

            return firstLevel
                .SelectMany(Get)
                .Concat(firstLevel)
                .Distinct()
                .ToArray();

            IEnumerable<Assembly> Get(Assembly assembly) {
                var referenced = assembly.GetReferencedAssemblies().Where(name => NamePredicate(name));
                var assemblies = referenced.Select(Assembly.Load).ToList();
                return assemblies.Concat(assemblies.SelectMany(Get)).Distinct();
            }
        }

        bool NamePredicate(AssemblyName name)
            => name.Name != null                   &&
               !name.Name.StartsWith("System.")    &&
               !name.Name.StartsWith("Microsoft.") &&
               !name.Name.StartsWith("netstandard");
    }

    static readonly Type AttributeType = typeof(EventTypeAttribute);

    void RegisterAssemblyEventTypes(Assembly assembly) {
        var decoratedTypes = assembly.DefinedTypes.Where(
            x => x.IsClass && x.CustomAttributes.Any(a => a.AttributeType == AttributeType)
        );

        foreach (var type in decoratedTypes) {
            var attr = type.GetAttribute<EventTypeAttribute>()!;
            AddType(type, attr.EventType);
        }
    }

    public void EnsureTypesRegistered(IEnumerable<Type> types) {
        foreach (var type in types) {
            GetTypeNameByType(type);
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class EventTypeAttribute : Attribute {
    public string EventType { get; }

    public EventTypeAttribute(string eventType) => EventType = eventType;
}

public class UnregisteredTypeException : Exception {
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public UnregisteredTypeException(Type type) : base($"Type {type.Name} is not registered in the type map") { }

    public UnregisteredTypeException(string type) : base($"Type name {type} is not registered in the type map") { }
}
