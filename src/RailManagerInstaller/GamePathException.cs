using System;
using System.Diagnostics.CodeAnalysis;

namespace RailManagerInstaller;

[ExcludeFromCodeCoverage]
public class GamePathException(string message) : Exception(message);