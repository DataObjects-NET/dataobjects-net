using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="CompilerContainerInfo"/> scope.
  /// </summary>
  internal sealed class CompilerContainerInfoScope : Scope<CompilerContainerInfo>
  {
    /// <summary>
    /// Gets the current <see cref="CompilerConfiguration"/>.
    /// </summary>
    internal static CompilerContainerInfo CurrentCompilerContainerInfo
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    internal CompilerContainerInfo CompilerContainerInfo
    {
      get { return this.Context; }
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="compilerContainerInfo">The compiler container info.</param>
    internal CompilerContainerInfoScope(CompilerContainerInfo compilerContainerInfo)
      : base(compilerContainerInfo)
    {
    }
  }
}
