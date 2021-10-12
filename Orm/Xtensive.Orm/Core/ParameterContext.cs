// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Core
{
  /// <summary>
  /// Provides storing context-specific <see cref="Parameter{TValue}"/>'s values.
  /// </summary>
  public sealed class ParameterContext
  {
    private readonly ParameterContext outerContext;

    private readonly Dictionary<Parameter, object> values = new Dictionary<Parameter, object>();

    [DebuggerStepThrough]
    internal bool TryGetValue(Parameter parameter, out object value) =>
      values.TryGetValue(parameter, out value) || outerContext?.TryGetValue(parameter, out value) == true;

    [DebuggerStepThrough]
    public TValue GetValue<TValue>(Parameter<TValue> parameter)
    {
      if (TryGetValue(parameter, out var result)) {
        return (TValue) result;
      }

      throw new InvalidOperationException(string.Format(Strings.ExValueForParameterXIsNotSet, parameter));
    }

    [DebuggerStepThrough]
    internal void SetValue(Parameter parameter, object value) => values[parameter] = value;

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ParameterContext(ParameterContext outerContext = null, IReadOnlyDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings = null)
    {
      this.outerContext = outerContext;

      if (tupleParameterBindings != null) {
        foreach (var (parameter, tuple) in tupleParameterBindings) {
          SetValue(parameter, tuple);
        }
      }
    }
  }
}
