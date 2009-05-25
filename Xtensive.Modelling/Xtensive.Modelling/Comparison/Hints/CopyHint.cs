// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.15

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Copy hint.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{SourcePath}, {TargetPath}")]
  public class CopyHint : Hint
  {
    /// <summary>
    /// Gets the source node path.
    /// </summary>
    public string SourcePath { get; private set; }

    /// <summary>
    /// Gets the target node path.
    /// </summary>
    public string TargetPath { get; private set; }

    /// <summary>
    /// Gets the copy parameters.
    /// </summary>
    public CollectionBaseSlim<CopyParameter> CopyParameters { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      yield return new HintTarget(ModelType.Source, SourcePath);
      yield return new HintTarget(ModelType.Target, TargetPath);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="parameters">The parameters.</param>
    public CopyHint(string sourcePath, string targetPath, 
      IEnumerable<CopyParameter> parameters)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourcePath, "sourcePath");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetPath, "targetPath");
      ArgumentValidator.EnsureArgumentNotNull(parameters, "parameters");

      SourcePath = sourcePath;
      TargetPath = targetPath;
      CopyParameters = new CollectionBaseSlim<CopyParameter>(parameters);
    }
  }
}