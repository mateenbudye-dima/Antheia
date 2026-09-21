using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs;

public record UpdatePreparationMethodDto(
    string? AdditionSequence,
    string? MixingSpeed,
    string? MixingTime,
    string? Temperature
);

public record PreparationMethodResponseDto(
    int PreparationId,
    int SectionId,
    string? AdditionSequence,
    string? MixingSpeed,
    string? MixingTime,
    string? Temperature
);