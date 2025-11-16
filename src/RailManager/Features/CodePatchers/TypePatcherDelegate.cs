using System;
using Mono.Cecil;
using RailManager.Wrappers.Mono.Cecil;

namespace RailManager.Features.CodePatchers;

public sealed record TypePatcherInfo(Type MarkerType, Func<TypePatcherDelegate> Factory);

public delegate bool TypePatcherDelegate(IAssemblyDefinition assemblyDefinition, TypeDefinition typeDefinition);