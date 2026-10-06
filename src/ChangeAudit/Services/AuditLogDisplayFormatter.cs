using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Models.Application;
using Dima.ChangeAudit.Models.Display;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dima.ChangeAudit.Services;

public sealed class AuditLogDisplayFormatter : IAuditLogDisplayFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FormattedAuditLogPagedResult FormatPaged(AuditLogPagedResult pagedResult)
    {
        return new FormattedAuditLogPagedResult
        {
            Items = FormatMany(pagedResult.Items),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }

    public IReadOnlyList<FormattedAuditLogResult> FormatMany(IEnumerable<AuditLogResult> items)
    {
        return items.Select(Format).ToList().AsReadOnly();
    }

    public FormattedAuditLogResult Format(AuditLogResult item)
    {
        var hierarchy = ParseHierarchy(item.HierarchyJson);
        var parentPath = BuildParentPath(item,hierarchy);

        return new FormattedAuditLogResult
        {
            Id = item.Id,
            Action = item.Action,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            RootEntityType = item.RootEntityType,
            RootEntityId = item.RootEntityId,
            UserId = item.UserId,
            TimestampUtc = item.TimestampUtc,
            ParentPath = parentPath,
            Description = BuildMainDescription(item, hierarchy, parentPath),
            DetailChanges = BuildDetailChanges(item)
        };
    }

    private static string BuildMainDescription(
    AuditLogResult item,
    AuditHierarchyInfo? hierarchy,
    string? parentPath)
    {
        var actionText = item.Action.ToLowerInvariant();
        var context = !string.IsNullOrEmpty(parentPath) ? $" in {parentPath}" : string.Empty;

        string entityLabel;

        // Use Target's Name/Title if available, but always rely on item.EntityId from table
        if (hierarchy?.Target != null && !string.IsNullOrWhiteSpace(hierarchy.Target.Name))
        {
            entityLabel = $"{item.EntityType} #{item.EntityId} ({hierarchy.Target.Name})";
        }
        else
        {
            // Standard fallback using top-level columns from database
            entityLabel = $"{item.EntityType} #{item.EntityId}";
        }

        return $"{entityLabel} was {actionText}{context}";
    }

    private static string? BuildParentPath(AuditLogResult item, AuditHierarchyInfo? hierarchy)
    {
        if (hierarchy == null)
            return null;

        var parts = new List<string>();

        //// 1. Root using item.RootEntityType and item.RootEntityId from top-level DB columns
        //if (hierarchy.Root != null)
        //{
        //    var rootLabel = !string.IsNullOrWhiteSpace(hierarchy.Root.Name)
        //        ? $"{item.RootEntityType} #{item.RootEntityId} ({hierarchy.Root.Name})"
        //        : $"{item.RootEntityType} #{item.RootEntityId}";

        //    parts.Add(rootLabel);
        //}

        // 2. Intermediate parent chain
        if (hierarchy.Parents != null)
        {
            foreach (var parent in hierarchy.Parents)
            {
                var parentLabel = FormatEntityRef(parent);
                if (!parts.Contains(parentLabel))
                {
                    parts.Add(parentLabel);
                }
            }
        }

        return parts.Count > 0 ? string.Join(" > ", parts) : null;
    }

    private static string FormatEntityRef(AuditEntityRef entityRef)
    {
        // Format: "EntityType #EntityId (Name)" if Name exists, otherwise "EntityType #EntityId"
        if (!string.IsNullOrWhiteSpace(entityRef.Name))
        {
            return $"{entityRef.EntityType} #{entityRef.EntityId} ({entityRef.Name})";
        }

        // Optional metadata fallback check
        if (entityRef.Metadata != null)
        {
            if (entityRef.Metadata.TryGetValue("Title", out var title) && !string.IsNullOrWhiteSpace(title))
                return $"{entityRef.EntityType} #{entityRef.EntityId} ({title})";

            if (entityRef.Metadata.TryGetValue("Name", out var name) && !string.IsNullOrWhiteSpace(name))
                return $"{entityRef.EntityType} #{entityRef.EntityId} ({name})";
        }

        return $"{entityRef.EntityType} #{entityRef.EntityId}";
    }

    private static List<string> BuildDetailChanges(AuditLogResult item)
    {
        var changes = new List<string>();

        if (item.Details == null || item.Details.Count == 0)
            return changes;

        foreach (var detail in item.Details)
        {
            var friendlyPropName = SplitCamelCase(detail.PropertyName);

            if (item.Action.Equals("Added", StringComparison.OrdinalIgnoreCase))
            {
                changes.Add($"Set {friendlyPropName} to '{detail.NewValue}'");
            }
            else if (item.Action.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
            {
                changes.Add($"Removed {friendlyPropName} (was '{detail.OldValue}')");
            }
            else // Modified
            {
                changes.Add($"Changed {friendlyPropName} from '{detail.OldValue ?? "null"}' to '{detail.NewValue ?? "null"}'");
            }
        }

        return changes;
    }

    private static AuditHierarchyInfo? ParseHierarchy(string? hierarchyJson)
    {
        if (string.IsNullOrWhiteSpace(hierarchyJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AuditHierarchyInfo>(hierarchyJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, "(\\B[A-Z])", " $1");
    }
}