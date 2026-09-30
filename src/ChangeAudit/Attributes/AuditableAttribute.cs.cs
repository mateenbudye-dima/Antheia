namespace Dima.ChangeAudit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class AuditableAttribute : Attribute
{
    /// <summary>
    /// Optional custom logical entity name (e.g. [Auditable("LabExperiment")]).
    /// If null, defaults to the class name.
    /// </summary>
    public string? EntityTypeName { get; }

    public AuditableAttribute(string? entityTypeName = null)
    {
        EntityTypeName = entityTypeName;
    }
}