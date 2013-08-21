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

    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
      return ResolveInternal(name.Name);
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
      return ResolveInternal(name.Name);
    }

    public AssemblyDefinition Resolve(string fullName)
    {
      return ResolveInternal(new AssemblyName(fullName).Name);
    }

    public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
    {
      return ResolveInternal(new AssemblyName(fullName).Name);
    }

    private AssemblyDefinition ResolveInternal(string shortName)
    {
      ModuleDefinition module;
      if (!loadedAssemblies.TryGetValue(shortName, out module)) {
        var file = GetAssemblyFile(shortName);
        module = LoadAssembly(file);
        loadedAssemblies.Add(shortName, module);
      }
      return module.Assembly;
    }

    private string GetAssemblyFile(string shortName)
    {
      string file;
      if (assemblyFiles.TryGetValue(shortName, out file))
        return file;
      logger.Write(MessageCode.ErrorUnableToFindReferencedAssembly, shortName);
      throw new StageFailedException();
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
    }
  }
}