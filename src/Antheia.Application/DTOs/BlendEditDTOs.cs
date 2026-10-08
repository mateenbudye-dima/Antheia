using Antheia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs
{
    public record GetBlendForEditDto(
        int BlendId,
        string Code,
        string? TrialNumber,
        string? Objective,
        string? Description,
        bool? IsPublished,
        BlendStatus Status,
        Guid CreatedBy,
        DateTime UpdatedDate,
        List<SectionEditDto> Sections
    );

    public record SectionEditDto(
        int SectionId,
        SectionType SectionTypeId,
        string SectionTitle,
        byte? SectionOrder,
        List<IngredientEditDto> Ingredients,
        PrepMethodEditDto? PreparationMethod,
        List<EvaluationEditDto> Evaluations
    );

    public record IngredientEditDto(
        int SectionIngredientId,
        string Name,
        IngredientType Type,
        decimal? Ratio,
        decimal? Quantity
    );

    public record PrepMethodEditDto(
        int PreparationId,
        string? AdditionSequence,
        string? MixingSpeed,
        string? MixingTime,
        string? Temperature
    );

    public record EvaluationEditDto(
        int EvaluationId,
        string EvaluationParameter,
        string? Result,
        string? Specification,
        string? Status
    );
}
