// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base class for any random generator.
  /// </summary>
  /// <typeparam name="T">Type of instances to generate.</typeparam>
  /// <remarks>
  /// Initializes a new instance of this type.
  /// </remarks>
  /// <param name="provider">Instance generator provider this generator is bound to.</param>
  public abstract class InstanceGeneratorBase<T>(IInstanceGeneratorProvider provider)
    : IInstanceGenerator<T>
  {
    /// <inheritdoc/>
    public IInstanceGeneratorProvider Provider
    {
      [DebuggerStepThrough]
      get;
    } = provider ?? throw new ArgumentNullException(nameof(provider));

    /// <inheritdoc/>
    public abstract T GetInstance(Random random);

    /// <inheritdoc/>
    public IEnumerable<T> GetInstances(Random random, int? count)
    {
      for (int i = 0; !count.HasValue || i < count.Value; i++) {
        yield return GetInstance(random);
      }
    }

    #region IInstanceGeneratorBase members

    /// <inheritdoc/>
    object IInstanceGeneratorBase.GetInstance(Random random) => GetInstance(random);

    /// <inheritdoc/>
    IEnumerable IInstanceGeneratorBase.GetInstances(Random random, int? count) => GetInstances(random, count);

    #endregion
  }
}