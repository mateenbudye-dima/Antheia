using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs
{
    public record GetTemplateForEditDto(
        int TemplateId,
        string Title,
        string? Objective,
        string? Description,
        bool? IsPublished,
        List<SectionEditDto> Sections
    );

    public record SectionEditDto(
        int SectionId,
        byte SectionTypeId,
        string SectionTitle,
        byte? SectionOrder,
        List<IngredientEditDto> Ingredients,
        PrepMethodEditDto? PreparationMethod,
        List<EvaluationEditDto> Evaluations
    );

    public record IngredientEditDto(
        int SectionIngredientId,
        string Name,
        string? Type,
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
        int EvaluationParameterType,
        string? Result,
        string? Specification,
        string? Status
    );
}
