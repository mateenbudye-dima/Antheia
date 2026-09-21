using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs;

public record CreateIngredientDto(
    int SectionId,
    string Name,
    string? Type,
    decimal? Ratio,
    decimal? Quantity
);

public record UpdateIngredientDto(
    string Name,
    string? Type,
    decimal? Ratio,
    decimal? Quantity
);

public record IngredientResponseDto(
    int SectionIngredientId,
    int SectionId,
    string Name,
    string? Type,
    decimal? Ratio,
    decimal? Quantity
);
