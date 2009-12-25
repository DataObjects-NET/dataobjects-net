// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.13

using System;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Provides data for <see cref="IAssemblyInspector.InspectionStart"/>, <see cref="IAssemblyInspector.InspectionComplete"/> 
  /// events in <see cref="IAssemblyInspector"/>.
  /// </summary>
  [Serializable]
  public class AssemblyInspectorEventArgs: EventArgs
  {
    private readonly string file;

    /// <summary>
    /// Gets the file that is being inspected.
    /// </summary>
    /// <value>The file.</value>
    public string File
    {
      get { return file; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyInspectorEventArgs"/> class.
    /// </summary>
    /// <param name="file">The file for inspection.</param>
    public AssemblyInspectorEventArgs(string file)
    {
      this.file = file;
    }
  }
}