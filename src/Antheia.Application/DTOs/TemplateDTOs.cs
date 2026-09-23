using Antheia.Domain.Enums;

namespace Antheia.Application.DTOs;

public record UpdateTemplateHeaderDto(
    string Title,
    string? Objective,
    string? Description
);

public record AddSectionDto(
    SectionType SectionTypeId,
    string SectionTitle
);

public record IngredientDto(
    string Name,
    string? Type,
    decimal? Ratio,
    decimal? Quantity
);

public record PrepMethodDto(
    string? AdditionSequence,
    string? MixingSpeed,
    string? MixingTime,
    string? Temperature
);

public record EvaluationDto(
    string EvaluationParameter,
    string? Result,
    string? Specification,
    string? Status
);

public record DraftTemplateCreatedDto(
    int TemplateId,
    int IngredientsSectionId,
    int PrepMethodSectionId,
    int EvaluationSectionId
);

public record TemplateListItemDto(
    int TemplateId,
    string Title,
    string? Objective,
    DateTime UpdatedDate,
    bool? IsPublished
);
