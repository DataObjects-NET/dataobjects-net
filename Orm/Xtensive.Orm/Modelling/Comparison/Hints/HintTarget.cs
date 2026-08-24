// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System;


namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Hint target reference.
  /// </summary>
  public struct HintTarget : IEquatable<HintTarget>
  {
    /// <summary>
    /// Gets the model this hint target points to.
    /// </summary>
    public ModelType Model { get; }

    /// <summary>
    /// Gets the node path this hint target points to.
    /// </summary>
    public string Path { get; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(HintTarget obj)
    {
      return obj.Model==Model && obj.Path==Path;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return obj is HintTarget hintTarget && Equals(hintTarget);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (Model.GetHashCode() * 397) ^ (Path!=null ? Path.GetHashCode() : 0);
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
      Model = model;
      Path = path;
    }
  }
}
