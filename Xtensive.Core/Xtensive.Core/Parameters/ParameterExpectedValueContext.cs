// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.21

using System;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Substitutes real values of <see cref="Parameter{TValue}"/>s with expected values.
  /// </summary>
  public sealed class ParameterExpectedValueContext : ParameterContext
  {
    internal override bool TryGetValue(Parameter parameter, out object value)
    {
      value = parameter.ExpectedValue;
      return true;
    }

    internal override void SetValue(Parameter parameter, object value)
    {
      throw new InvalidOperationException(Resources.Strings
        .ExCantSetExpectedValueOfParameterViaParameterContext);
    }
  }
}