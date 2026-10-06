using Antheia.Domain.Enums;

namespace Antheia.Application.DTOs;

public record CreateIngredientDto(
    int SectionId,
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity,

    string SectionTitle,
    int BlendId,
    string? BlendCode
);

public record UpdateIngredientDto(
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity,

    // Optional metadata to avoid EF hierarchy reads during audit
    int SectionId,
    string SectionTitle,
    int BlendId,
    string? BlendCode
);

public record IngredientResponseDto(
    int SectionIngredientId,
    int SectionId,
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity
);
