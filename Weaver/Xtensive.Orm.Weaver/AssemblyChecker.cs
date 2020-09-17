// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.09.02

using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  public sealed class AssemblyChecker
  {
    private readonly Dictionary<string, byte[]> frameworkAssemblies = new Dictionary<string, byte[]>(WeavingHelper.AssemblyNameComparer);
    private readonly Dictionary<string, byte[]> netCoreAssemblies = new Dictionary<string, byte[]>(WeavingHelper.AssemblyNameComparer);


    public void RegisterFrameworkAssembly(string name, string publicKeyToken)
    {
      frameworkAssemblies.Add(name, WeavingHelper.ParsePublicKeyToken(publicKeyToken));
    }

    public void RegisterNetCoreAssembly(string name, string publicKeyToken)
    {
      netCoreAssemblies.Add(name, WeavingHelper.ParsePublicKeyToken(publicKeyToken));
    }

    public bool IsFrameworkAssembly(AssemblyNameReference reference)
    {
      byte[] expectedToken;
      return frameworkAssemblies.TryGetValue(reference.Name, out expectedToken) && reference.HasPublicKeyToken(expectedToken);
    }

    public bool IsNetCoreAssembly(AssemblyNameReference reference)
    {
      byte[] expectedToken;
      return netCoreAssemblies.TryGetValue(reference.Name, out expectedToken) && reference.HasPublicKeyToken(expectedToken);
    }
  }
}