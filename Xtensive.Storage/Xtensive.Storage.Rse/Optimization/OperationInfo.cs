// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Information regarding to an operation was performed on a field of tuple.
  /// </summary>
  internal sealed class OperationInfo
  {
    public static class WellKnownNames
    {
      public static readonly string Add = "+";
      public static readonly string Multiply = "*";
      public static readonly string Substract = "-";
      public static readonly string Divide = "/";
      public static readonly string CompareTo = "CompareTo";
      public static readonly string Not = "!";
      public static readonly string Unknown = "<unknown>";
    }

    private readonly List<Expression> arguments;
    public readonly string Name;
    public readonly Expression Source;

    public ReadOnlyList<Expression> GetArguments()
    {
      return new ReadOnlyList<Expression>(arguments, false);
    }

    public OperationInfo(string name, IEnumerable<Expression> arguments, Expression source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      Name = name;
      Source = source;
      if (arguments != null)
        this.arguments = new List<Expression>(arguments);
      else
        this.arguments = new List<Expression>();
    }
  }
}
