namespace Dima.ChangeAudit.Models.Resolvers
{
    public sealed class AuditHierarchy
    {
        public AuditEntityReference? Root { get; set; }

        /// <summary>
        /// Describes the entity being directly audited/changed (e.g., Section name, Title).
        /// </summary>
        public AuditEntityReference? Target { get; set; }

        /// <summary>
        /// Intermediate parents between Root and Target. Excludes Target and Root.
        /// </summary>
        public List<AuditEntityReference> Parents { get; set; } = [];
    }
}
