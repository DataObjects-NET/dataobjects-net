// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL parameter name provider.
  /// </summary>
  public class SqlParameterNameProvider
  {
    private const string DefaultPrefix = "p";
    private readonly string prefix;
    private int nextParameter;

    internal Dictionary<object, string> NameTable { get; }

    /// <summary>
    /// Gets the name for the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>Name for the specified parameter.</returns>
    public string GetName(object parameter)
    {
      if (!NameTable.TryGetValue(parameter, out var result)) {
        result = prefix + nextParameter++;
        NameTable.Add(parameter, result);
      }
      return result;
    }


    // Constructor

    /// <inheritdoc/>
    public SqlParameterNameProvider(SqlCompilerConfiguration configuration)
    {
      NameTable = new Dictionary<object, string>();
      prefix = string.IsNullOrEmpty(configuration.ParameterNamePrefix)
        ? DefaultPrefix
        : configuration.ParameterNamePrefix;
    }
  }
}