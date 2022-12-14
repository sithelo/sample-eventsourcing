using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ThriveEventFlow;

[PublicAPI]
public static class Ensure {
    
    
    public static T NotNull<T>(T? value, [CallerArgumentExpression("value")] string? name = default) where T : class
        => value ?? throw new ArgumentNullException(name);

    
    public static string NotEmptyString(string? value, [CallerArgumentExpression("value")] string? name = default)
        => !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentNullException(name);

   
    public static void IsTrue(bool condition, Func<string> errorMessage) {
        if (!condition) throw new DomainException(errorMessage());
    }

   
    public static void IsTrue(bool condition, Func<Exception> getException) {
        if (!condition) throw getException();
    }
}