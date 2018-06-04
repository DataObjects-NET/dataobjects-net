// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.02

using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyChecker
  {
    private readonly Dictionary<string, byte[]> frameworkAssemblies = new Dictionary<string, byte[]>(WeavingHelper.AssemblyNameComparer);
    private readonly Dictionary<string, byte[]> netStandardAssemblies = new Dictionary<string, byte[]>(WeavingHelper.AssemblyNameComparer);


    public void RegisterFrameworkAssembly(string name, string publicKeyToken)
    {
      frameworkAssemblies.Add(name, WeavingHelper.ParsePublicKeyToken(publicKeyToken));
    }

    public void RegisterNetStandardAssembly(string name, string publicKeyToken)
    {
      netStandardAssemblies.Add(name, WeavingHelper.ParsePublicKeyToken(publicKeyToken));
    }

    public bool IsFrameworkAssembly(AssemblyNameReference reference)
    {
      byte[] expectedToken;
      return frameworkAssemblies.TryGetValue(reference.Name, out expectedToken) && reference.HasPublicKeyToken(expectedToken);
    }

    public bool IsNetStandardAssembly(AssemblyNameReference reference)
    {
      byte[] expectedToken;
      return netStandardAssemblies.TryGetValue(reference.Name, out expectedToken) && reference.HasPublicKeyToken(expectedToken);
    }
  }
}