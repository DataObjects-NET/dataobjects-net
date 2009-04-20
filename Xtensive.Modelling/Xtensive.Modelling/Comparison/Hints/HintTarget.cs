// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  [Serializable]
  public struct HintTarget : IEquatable<HintTarget>
  {
    private ModelType model;
    private string path;

    public ModelType Model {
      get { return model; }
    }

    public string Path {
      get { return path; }
    }

    #region Equality members

    public bool Equals(HintTarget obj)
    {
      return obj.model==model && obj.path==path;
    }

    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (HintTarget))
        return false;
      return Equals((HintTarget) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return (model.GetHashCode() * 397) ^ (path!=null ? path.GetHashCode() : 0);
      }
    }

    public static bool operator ==(HintTarget left, HintTarget right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(HintTarget left, HintTarget right)
    {
      return !left.Equals(right);
    }

    #endregion


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
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