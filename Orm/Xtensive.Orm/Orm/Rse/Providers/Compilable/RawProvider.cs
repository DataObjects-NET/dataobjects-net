// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.07.09

using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class RawProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;
    private Func<ParameterContext, IEnumerable<Tuple>> compiledSource;

    /// <summary>
    /// Raw data source - an array of tuples.
    /// </summary>
    public Expression<Func<ParameterContext, IEnumerable<Tuple>>> Source { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Source"/>.
    /// </summary>
    public Func<ParameterContext, IEnumerable<Tuple>> CompiledSource {
      get {
        if (compiledSource==null)
          compiledSource = Source.CachingCompile();
        return compiledSource;
      }
    }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Source.ToString(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public RawProvider(RecordSetHeader header, Expression<Func<ParameterContext, IEnumerable<Tuple>>> source)
      : base(ProviderType.Raw)
    {
      Source = source;
      this.header = header;
      Initialize();
    }
  }
}