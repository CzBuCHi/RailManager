using System;
using System.Collections.Generic;

namespace RailManager.Exceptions;

public sealed class ValidationException(string message, IReadOnlyCollection<string> errors) : Exception(message)
{
    public IReadOnlyCollection<string> Errors { get; } = errors;
}
