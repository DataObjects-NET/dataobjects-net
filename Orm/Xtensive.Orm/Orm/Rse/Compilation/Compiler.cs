using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers that implements visitor pattern.
  /// Compiles <see cref="CompilableProvider"/>s into <see cref="ExecutableProvider"/>.
  /// </summary>
  public abstract class Compiler<TResult> : ProviderVisitor<TResult>, ICompiler
    where TResult : ExecutableProvider
  {
    private readonly Stack<CompilableProvider> traversalStack = new Stack<CompilableProvider>();
    private CompilableProvider owner;
    private CompilableProvider rootProvider;

    /// <summary>
    /// Gets <see cref="CompilableProvider"/>
    /// that is just above <see cref="CompilableProvider"/> that is currently processed.
    /// For root provider returns <see langword="null"/>.
    /// </summary>
    protected CompilableProvider Owner
    {
      get
      {
        if (owner==null && traversalStack.Count >= 2)
          owner = traversalStack.ElementAt(1);
        return owner;
      }
    }

    /// <summary>
    /// Gets root of <see cref="CompilableProvider"/> tree.
    /// </summary>
    protected CompilableProvider RootProvider { get { return rootProvider; } }

    /// <inheritdoc/>
    ExecutableProvider ICompiler.Compile(CompilableProvider provider)
    {
      return Compile(provider);
    }

    /// <summary>
    /// Compiles the specified <see cref="CompilableProvider"/>.
    /// </summary>
    /// <param name="cp">The provider to compile.</param>
    public TResult Compile(CompilableProvider cp)
    {
      if (cp == null) {
        return null;
      }
      if (rootProvider == null) {
        rootProvider = cp;
        Initialize();
      }
      traversalStack.Push(cp);
      TResult result = Visit(cp);
      traversalStack.Pop();
      owner = null;
      return result;
    }

    /// <summary>
    /// Initializes this instance just before first VisitXxx() is called.
    /// </summary>
    protected virtual void Initialize()
    {
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected Compiler()
    {
    }
  }
}
