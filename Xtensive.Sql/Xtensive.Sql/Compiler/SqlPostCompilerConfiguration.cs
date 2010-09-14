// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.07

using System.Collections.Generic;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// <see cref="PostCompiler"/> configuration.
  /// </summary>
  public sealed class SqlPostCompilerConfiguration
  {
    public HashSet<object> AlternativeBranches { get; private set; }

    public Dictionary<object, string> PlaceholderValues { get; private set; }

    public Dictionary<object, List<string[]>> DynamicFilterValues { get; private set; }


    // Constructors

    public SqlPostCompilerConfiguration()
    {
      AlternativeBranches = new HashSet<object>();
      PlaceholderValues = new Dictionary<object, string>();
      DynamicFilterValues = new Dictionary<object, List<string[]>>();
    }
  }
}