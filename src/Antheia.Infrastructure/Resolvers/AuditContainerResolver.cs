using Antheia.Domain.Entities;
using Dima.ChangeAudit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Antheia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Dima.ChangeAudit.Models.Resolvers;

namespace Antheia.Infrastructure.Resolvers
{
    public sealed class AuditContainerResolver
    : IAuditContainerResolver
    {
        public async Task<AuditEntityReference?> ResolveAsync(
            int containerId,
            byte containerTypeId,
            DbContext context,
            CancellationToken cancellationToken = default)
        {
            return containerTypeId switch
            {
                (byte)SectionContainerType.Blend =>
                    await ResolveBlendAsync(
                        containerId,
                        context,
                        cancellationToken),

                //(byte)SectionContainerType.Experiment =>
                //    await ResolveExperimentAsync(
                //        containerId,
                //        context,
                //        cancellationToken),

                //(byte)SectionContainerType.Template =>
                //    await ResolveTemplateAsync(
                //        containerId,
                //        context,
                //        cancellationToken),

                _ => null
            };
        }

        private static async Task<AuditEntityReference?>
            ResolveBlendAsync(
                int blendId,
                DbContext context,
                CancellationToken cancellationToken)
        {
            var exists = await context.Set<BlendRecord>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.BlendId == blendId,
                    cancellationToken);

            if (!exists)
                return null;

            return new AuditEntityReference
            {
                EntityType = "Blend",
                EntityId = blendId.ToString(),
                Name = await context.Set<BlendRecord>()
                    .AsNoTracking()
                    .Where(x => x.BlendId == blendId)
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync(cancellationToken)
            };
        }

        //private static async Task<AuditEntityReference?>
        //    ResolveExperimentAsync(
        //        int experimentId,
        //        DbContext context,
        //        CancellationToken cancellationToken)
        //{
        //    var exists = await context.Set<ExperimentRecord>()
        //        .AsNoTracking()
        //        .AnyAsync(
        //            x => x.ExperimentId == experimentId,
        //            cancellationToken);

        //    if (!exists)
        //        return null;

        //    return new AuditEntityReference
        //    {
        //        EntityType = "Experiment",
        //        EntityId = experimentId.ToString()
        //    };
        //}

        //private static async Task<AuditEntityReference?>
        //    ResolveTemplateAsync(
        //        int templateId,
        //        DbContext context,
        //        CancellationToken cancellationToken)
        //{
        //    var exists = await context.Set<Template>()
        //        .AsNoTracking()
        //        .AnyAsync(
        //            x => x.TemplateId == templateId,
        //            cancellationToken);

        //    if (!exists)
        //        return null;

        //    return new AuditEntityReference
        //    {
        //        EntityType = "Template",
        //        EntityId = templateId.ToString()
        //    };
        //}
    }
}
