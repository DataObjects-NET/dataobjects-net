// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;

namespace Xtensive.Orm.Internals
{
  internal sealed class CompiledQueryProcessingScope
  {
    private TranslatedQuery parameterizedQuery;

    public Parameter QueryParameter { get; }
    public ExtendedExpressionReplacer QueryParameterReplacer { get; }
    public ParameterContext ParameterContext { get; }
    public bool Execute { get; }

    [field: ThreadStatic]
    internal static CompiledQueryProcessingScope Current { get; private set; }

    public readonly ref struct ThreadBindingToken
    {
      public void Dispose() => Current = null;

      public ThreadBindingToken(CompiledQueryProcessingScope scope)
      {
        if (Current != null) {
          throw new InvalidOperationException(
            string.Format(Strings.ExXNestingIsNotSupported, nameof(CompiledQueryProcessingScope)));
        }
        Current = scope;
      }
    }

    /// <summary>
    /// Binds <see cref="CompiledQueryProcessingScope"/> instance to a current thread.
    /// </summary>
    public ThreadBindingToken Enter() => new ThreadBindingToken(this);

    /// <exception cref="NotSupportedException">Second attempt to set this property.</exception>
    public TranslatedQuery ParameterizedQuery {
      get => parameterizedQuery;
      set {
        if (parameterizedQuery != null) {
          throw Exceptions.AlreadyInitialized("ParameterizedQuery");
        }

        parameterizedQuery = value;
      }
    }

    // Constructors

    public CompiledQueryProcessingScope(Parameter queryParameter,
      ExtendedExpressionReplacer queryParameterReplacer, ParameterContext parameterContext = null, bool execute = true)
    {
      ParameterContext = parameterContext;
      QueryParameter = queryParameter;
      QueryParameterReplacer = queryParameterReplacer;
      Execute = execute;
    }
  }
}