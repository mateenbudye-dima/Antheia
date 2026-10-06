using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Antheia.Application.DTOs;

public record UpdatePreparationMethodDto(
    string? AdditionSequence,
    string? MixingSpeed,
    string? MixingTime,
    string? Temperature,

    int SectionId,
    string SectionTitle,
    int BlendId,
    string BlendCode
);

public record PreparationMethodResponseDto(
    int PreparationId,
    int SectionId,
    string? AdditionSequence,
    string? MixingSpeed,
    string? MixingTime,
    string? Temperature
);