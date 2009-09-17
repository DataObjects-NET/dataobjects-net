// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.17

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Custom error information for user interface.
  /// </summary>
  [Serializable]
  public class DataErrorInfo : IDataErrorInfo
  {
    private readonly string error;
    private readonly Dictionary<string, string> columnErrors = 
      new Dictionary<string, string>();

    public string Error
    {
      get { return error; }
    }

    public string this[string columnName]
    {
      get { return columnErrors[columnName]; }
    }

    public DataErrorInfo() : this(null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="error">The error message.</param>
    public DataErrorInfo(string error)
    {
      this.error = error;
    }
  }
}