using System;

namespace MediaOrganizerApp.Application.Exceptions;

/// <summary>Transient OpenRouter failure (network / 429 / 503) that may succeed on retry.</summary>
public sealed class CloudProviderTransientException : Exception
{
    public CloudProviderTransientException(string message) : base(message) { }
    public CloudProviderTransientException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>The selected model does not support reasoning / thinking; retry without it.</summary>
public sealed class CloudModelReasoningUnsupportedException : Exception
{
    public CloudModelReasoningUnsupportedException(string message) : base(message) { }
}