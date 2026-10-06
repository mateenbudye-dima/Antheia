using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.DTOs;

public record CreateEvaluationDto(
    int SectionId,
    string EvaluationParameter,
    string? Specification,
    string? Result,
    string? Status,

    string SectionTitle,
    int BlendId,
    string BlendCode
);

public record UpdateEvaluationDto(
    string EvaluationParameter,
    string? Specification,
    string? Result,
    string? Status,

    int SectionId,
    string SectionTitle,
    int BlendId,
    string BlendCode
);

public record EvaluationResponseDto(
    int EvaluationId,
    int SectionId,
    string EvaluationParameter,
    string? Specification,
    string? Result,
    string? Status
);

