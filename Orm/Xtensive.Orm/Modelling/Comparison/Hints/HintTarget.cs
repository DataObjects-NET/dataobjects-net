// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System;


namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint target reference.
  /// </summary>
  [Serializable]
  public struct HintTarget : IEquatable<HintTarget>
  {
    private ModelType model;
    private string path;

    /// <summary>
    /// Gets the model this hint target points to.
    /// </summary>
    public ModelType Model {
      get { return model; }
    }

    /// <summary>
    /// Gets the node path this hint target points to.
    /// </summary>
    public string Path {
      get { return path; }
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(HintTarget obj)
    {
      return obj.model==model && obj.path==path;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (HintTarget))
        return false;
      return Equals((HintTarget) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (model.GetHashCode() * 397) ^ (path!=null ? path.GetHashCode() : 0);
      }
    }

    /// <summary>
    /// Checks specified objects for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(HintTarget left, HintTarget right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Checks specified objects for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(HintTarget left, HintTarget right)
    {
      return !left.Equals(right);
    }

    #endregion


    // Constructors

    /// <summary>
    ///   Initializes new instance of this type.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="path">The path.</param>
    public HintTarget(ModelType model, string path)
    {
      this.model = model;
      this.path = path;
    }
  }
}