// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base class for any wrapping <see cref="IInstanceGenerator{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to generate random instances for.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  /// <remarks>
  /// <see cref="ClassDocTemplate.Ctor" copy="true" />
  /// </remarks>
  /// <param name="provider">Instance generator provider this generator is bound to.</param>
  public abstract class WrappingInstanceGenerator<T, TBase1, TBase2>(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<T>(provider)
  {
    /// <summary>
    /// Generator for the first base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase1> BaseGenerator1 = provider.GetInstanceGenerator<TBase1>();

    /// <summary>
    /// Generator for the first base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase2> BaseGenerator2 = provider.GetInstanceGenerator<TBase2>();
  }
}