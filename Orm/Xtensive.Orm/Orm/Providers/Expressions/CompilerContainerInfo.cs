using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Compiler container info
  /// </summary>
  public sealed class CompilerContainerInfo
  {
    public ProviderInfo ProviderInfo { get; private set; }

    public static CompilerContainerInfo Current
    {
      get { return CompilerContainerInfoScope.CurrentCompilerContainerInfo; }
    }


    internal CompilerContainerInfo(ProviderInfo providerInfo)
    {
      this.ProviderInfo = providerInfo;
    }
  }
}
