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
    private readonly Dictionary<string, string> assemblyFiles = new Dictionary<string, string>(WeavingHelper.AssemblyNameComparer);
    private readonly Dictionary<string, ModuleDefinition> loadedAssemblies = new Dictionary<string, ModuleDefinition>(WeavingHelper.AssemblyNameComparer);
    private readonly IAssemblyResolver defaultAssemblyResolver;

    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
      return ResolveInternal(name);
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
      return ResolveInternal(name);
    }

    public AssemblyDefinition Resolve(string fullName)
    {
      return ResolveInternal(AssemblyNameReference.Parse(fullName));
    }

    public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
    {
      return ResolveInternal(AssemblyNameReference.Parse(fullName));
    }

    private AssemblyDefinition ResolveInternal(AssemblyNameReference name)
    {
      var shortName = name.Name;
      ModuleDefinition module;
      if (!loadedAssemblies.TryGetValue(shortName, out module)) {
        string file;
        if (!TryGetAssemblyFile(shortName, out file)) {
          AssemblyDefinition resolvedAssembly;
          try {
            resolvedAssembly = defaultAssemblyResolver.Resolve(name);
          }
          catch (Exception) {
            resolvedAssembly = null;
          }
          if (resolvedAssembly==null) {
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
      if (assemblyFiles.TryGetValue(shortName, out file))
        return true;
      return false;
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
      if (referencedAssemblies==null)
        throw new ArgumentNullException("referencedAssemblies");
      if (logger==null)
        throw new ArgumentNullException("logger");

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
    }
  }
}