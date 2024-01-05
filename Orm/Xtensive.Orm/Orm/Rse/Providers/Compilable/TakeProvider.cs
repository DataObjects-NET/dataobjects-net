// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that takes first N result records from <see cref="UnaryProvider.Source"/>. Amount of records is specified using <see cref="Count"/> property.
  /// </summary>
  [Serializable]
  public sealed class TakeProvider : UnaryProvider
  {
    /// <summary>
    /// Take amount function.
    /// </summary>
    public Func<ParameterContext, int> Count { get; private set; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Count.ToString();
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The <see cref="Count"/> property value.</param>
    public TakeProvider(CompilableProvider provider, Func<ParameterContext, int> count)
      : base(ProviderType.Take, provider)
    {
      Count = count;
      Initialize();
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The value for <see cref="Count"/> function property.</param>
    public TakeProvider(CompilableProvider provider, int count)
      : base(ProviderType.Take, provider)
    {
      Count = context => count;
      Initialize();
    }
  }
}