using System;
using System.Diagnostics.CodeAnalysis;

namespace RailManagerInstaller;

[ExcludeFromCodeCoverage]
public class InstallerException : Exception
{
    public InstallerException(string message)
        : base(message) {
    }

    public InstallerException(string message, Exception innerException)
        : base(message, innerException) {
    }
}