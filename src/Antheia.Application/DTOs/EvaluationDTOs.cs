using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs;

public record CreateEvaluationDto(
    int SectionId,
    int EvaluationParameterType,
    string? Specification,
    string? Result,
    string? Status
);

public record UpdateEvaluationDto(
    string? Specification,
    string? Result,
    string? Status
);

public record EvaluationResponseDto(
    int EvaluationId,
    int SectionId,
    int EvaluationParameterType,
    string? Specification,
    string? Result,
    string? Status
);

