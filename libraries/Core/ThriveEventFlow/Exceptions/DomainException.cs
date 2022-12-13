namespace ThriveEventFlow;

public class DomainException : Exception {
    public DomainException(string message) : base(message) { }
}