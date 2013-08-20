// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class AssemblyResolver : IAssemblyResolver
  {
    private readonly IList<string> referencedAssemblies;

    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
      throw new System.NotImplementedException();
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
      throw new System.NotImplementedException();
    }

    public AssemblyDefinition Resolve(string fullName)
    {
      throw new System.NotImplementedException();
    }

    public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
    {
      throw new System.NotImplementedException();
    }

    public AssemblyResolver(IList<string> referencedAssemblies)
    {
      if (referencedAssemblies==null)
        throw new ArgumentNullException("referencedAssemblies");
      this.referencedAssemblies = referencedAssemblies;
    }
  }
}