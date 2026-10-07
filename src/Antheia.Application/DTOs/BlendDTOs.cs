using Antheia.Domain.Enums;

namespace Antheia.Application.DTOs;

public record UpdateBlendHeaderDto(
    string Code,
    string? Objective,
    string? Description,
    string? TrialNumber
);

public record AddSectionDto(
    SectionType SectionTypeId,
    string SectionTitle,
    string BlendCode
);

public record IngredientDto(
    string Name,
    IngredientType Type,
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

public record DraftBlendCreatedDto(
    int BlendId,
    int IngredientsSectionId,
    int PrepMethodSectionId,
    int EvaluationSectionId
);

public record BlendListItemDto(
    int BlendId,
    string Code,
    string? TrialNumber,
    string? Objective,
    DateTime UpdatedDate,
    bool? IsPublished,
    BlendStatus Status,
    Guid CreatedBy
);
