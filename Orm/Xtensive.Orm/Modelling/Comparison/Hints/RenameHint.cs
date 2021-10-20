// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Rename hint.
  /// </summary>
  [Serializable]
  public class RenameHint : Hint
  {
    /// <summary>
    /// Gets the source node path.
    /// </summary>
    public string SourcePath { get; private set; }

    /// <summary>
    /// Gets the target node path.
    /// </summary>
    public string TargetPath { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      yield return new HintTarget(ModelType.Source, SourcePath);
      yield return new HintTarget(ModelType.Target, TargetPath);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"Rename '{SourcePath}' -> '{TargetPath}'";
    }

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    public RenameHint(string sourcePath, string targetPath)
    {
      SourcePath = sourcePath;
      TargetPath = targetPath;
    }
  }
}