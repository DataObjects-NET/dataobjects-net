using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Weaver
{
  internal static class PartnerAssemblies
  {
    internal static string[] GetAssemblyNameTemplates()
    {
      return new string[] {
        @"^MEScontrol\.[A-Za-z\.]* 67d5889111bf42c8",
      };
    }

    internal static string[] GetAssemblies()
    {
      return new string[] {
        "Xtensive.Project109.Host.Data 41612694afcd3c2c"
      };
    }

  }
}
