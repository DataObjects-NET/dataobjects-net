using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal class PartnerAssemblyChecker
  {
    private static PartnerAssemblyChecker Checker;
    private static object CheckerGuard = new object();

    private readonly Dictionary<string, byte[]> partnerAssemblies = new Dictionary<string, byte[]>(WeavingHelper.AssemblyNameComparer);
    private readonly Dictionary<string, byte[]> partnerAssemblyTemplate = new Dictionary<string, byte[]>();

    public bool IsPartnerAssembly(AssemblyNameReference reference)
    {
      byte[] expectedToken;
      if (partnerAssemblies.TryGetValue(reference.Name, out expectedToken) && reference.HasPublicKeyToken(expectedToken))
        return true;

      return partnerAssemblyTemplate.Select(assemblyTemplate => new Regex(assemblyTemplate.Key)).Any(regex => regex.IsMatch(reference.Name));
    }

    internal static PartnerAssemblyChecker Get()
    {
      lock (CheckerGuard) {
        if (Checker!=null)
          return Checker;
        Checker = new PartnerAssemblyChecker();
        return Checker;
      }
    }

    private PartnerAssemblyChecker()
    {
      foreach (var assemblyName in PartnerAssemblies.GetAssemblies()) {
        var items = assemblyName.Split();
        partnerAssemblies.Add(items[0], WeavingHelper.ParsePublicKeyToken(items[1]));
      }

      foreach (var assemblyNameTemplate in PartnerAssemblies.GetAssemblyNameTemplates()) {
        var items = assemblyNameTemplate.Split();
        partnerAssemblyTemplate.Add(items[0], WeavingHelper.ParsePublicKeyToken(items[1]));
      }
    }
  }
}
