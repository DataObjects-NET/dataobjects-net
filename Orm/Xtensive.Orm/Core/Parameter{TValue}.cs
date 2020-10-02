// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Diagnostics;

namespace Xtensive.Core
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of parameter value.</typeparam>
  public class Parameter<TValue> : Parameter
  {
    /// <summary>
    /// Always fails.
    /// This property serves as a stub to build expression trees.
    /// Use <see cref="ParameterContext.GetValue{TValue}"/> method instead.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public TValue Value {
      [DebuggerStepThrough]
      get => throw new NotSupportedException();
    }

    // Constructors

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Parameter(string name)
      : base(name)
    { }
  }
}