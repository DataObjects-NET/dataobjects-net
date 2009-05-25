// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.18

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Parameter for <see cref="CopyHint"/>.
  /// </summary>
  [Serializable]
  public struct CopyParameter : IEquatable<CopyParameter>
  {
    /// <summary>
    /// Gets the source node path.
    /// </summary>
    public string SourcePath { get; private set; }

    /// <summary>
    /// Gets the target node path.
    /// </summary>
    public string TargetPath { get; private set; }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public bool Equals(CopyParameter other)
    {
      return other.SourcePath==SourcePath 
        && other.TargetPath==TargetPath;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (CopyParameter))
        return false;
      return Equals((CopyParameter) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (SourcePath!=null ? SourcePath.GetHashCode() : 0);
        result = (result * 397) ^ (TargetPath!=null ? TargetPath.GetHashCode() : 0);
        return result;
      }
    }

    /// <see cref="ClassDocTemplate.OperatorEq"/>
    public static bool operator ==(CopyParameter left, CopyParameter right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(CopyParameter left, CopyParameter right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}=={1}", SourcePath, TargetPath);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CopyParameter(string sourcePath, string targetPath)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourcePath, "sourcePath");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetPath, "targetPath");
      
      SourcePath = sourcePath;
      TargetPath = targetPath;
    }

  }
}