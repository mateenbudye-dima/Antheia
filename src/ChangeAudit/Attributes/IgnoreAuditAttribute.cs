
namespace Dima.ChangeAudit.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class IgnoreAuditAttribute : Attribute { }