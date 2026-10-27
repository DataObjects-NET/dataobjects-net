// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class AssemblyResolver : IAssemblyResolver
  {
    private readonly MessageLogger logger;
    private readonly Dictionary<string, string> assemblyFiles = new(WeavingHelper.AssemblyNameComparer);
    private readonly Dictionary<string, ModuleDefinition> loadedAssemblies = new(WeavingHelper.AssemblyNameComparer);
    private readonly IAssemblyResolver defaultAssemblyResolver;

    public AssemblyDefinition Resolve(AssemblyNameReference name) => ResolveInternal(name);

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => ResolveInternal(name);

    public AssemblyDefinition Resolve(string fullName) => ResolveInternal(AssemblyNameReference.Parse(fullName));

    public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters) => ResolveInternal(AssemblyNameReference.Parse(fullName));

    private AssemblyDefinition ResolveInternal(AssemblyNameReference name)
    {
      var shortName = name.Name;
      if (!loadedAssemblies.TryGetValue(shortName, out var module)) {
        if (!TryGetAssemblyFile(shortName, out var file)) {
          AssemblyDefinition resolvedAssembly;
          try {
            resolvedAssembly = defaultAssemblyResolver.Resolve(name);
          }
          catch (Exception) {
            resolvedAssembly = null;
          }
          if (resolvedAssembly is null) {
            logger.Write(MessageCode.ErrorUnableToFindReferencedAssembly, shortName);
            throw new StageFailedException();
          }
          return resolvedAssembly;
        }
        module = LoadAssembly(file);
        loadedAssemblies.Add(shortName, module);
      }
      return module.Assembly;
    }

    private bool TryGetAssemblyFile(string shortName, out string file)
    {
      return assemblyFiles.TryGetValue(shortName, out file);
    }

    private ModuleDefinition LoadAssembly(string file)
    {
      var parameters = new ReaderParameters {
        ReadingMode = ReadingMode.Deferred,
        AssemblyResolver = this,
        MetadataResolver = new MetadataResolver(this),
      };
      return ModuleDefinition.ReadModule(file, parameters);
    }

    public AssemblyResolver(IEnumerable<string> referencedAssemblies, MessageLogger logger)
    {
      ArgumentNullException.ThrowIfNull(referencedAssemblies);
      ArgumentNullException.ThrowIfNull(logger);

      this.logger = logger;

      foreach (var file in referencedAssemblies) {
        var name = Path.GetFileNameWithoutExtension(file);
        if (name!=null && File.Exists(file))
          assemblyFiles.Add(name, file);
        else
          logger.Write(MessageCode.WarningReferencedAssemblyFileIsNotFound, file);
      }
      this.defaultAssemblyResolver = new DefaultAssemblyResolver();
    }

    public void Dispose()
    {
      defaultAssemblyResolver.Dispose();
    }
  }
}