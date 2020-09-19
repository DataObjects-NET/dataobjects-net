// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Xtensive.Orm.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ExecutableRawProvider : ExecutableProvider<Providers.RawProvider>
  {
    #region Cached properties

    private const string CachedSourceName = "CachedSource";

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var parameterContext = ((Xtensive.Orm.Providers.EnumerationContext) context).ParameterContext;
      SetValue(context, CachedSourceName, Origin.CompiledSource.Invoke(parameterContext));
    }

    /// <inheritdoc/>
    protected internal override DataReader OnEnumerate(EnumerationContext context)
    {
      return new DataReader(GetValue<IEnumerable<Tuple>>(context, CachedSourceName));
    }


    // Constructors

    public ExecutableRawProvider(Providers.RawProvider origin)
      : base(origin)
    {
    }
  }
}