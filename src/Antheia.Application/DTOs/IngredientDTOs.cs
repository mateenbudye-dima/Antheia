using Antheia.Domain.Enums;

namespace Antheia.Application.DTOs;

public record CreateIngredientDto(
    int SectionId,
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity
);

public record UpdateIngredientDto(
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity
);

public record IngredientResponseDto(
    int SectionIngredientId,
    int SectionId,
    string Name,
    IngredientType Type,
    decimal? Ratio,
    decimal? Quantity
);
