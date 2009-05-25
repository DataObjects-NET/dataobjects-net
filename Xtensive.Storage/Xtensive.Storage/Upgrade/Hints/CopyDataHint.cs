// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.15

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Copy data from one node to another.
  /// </summary>
  [Serializable]
  public sealed class CopyDataHint : UpgradeHint,
    IEquatable<CopyDataHint>
  {
    /// <summary>
    /// Gets the target node name.
    /// </summary>
    public string TargetColumnName { get; private set; }

    /// <summary>
    /// Gets the source node name.
    /// </summary>
    public string SourceColumnName { get; private set; }

    /// <summary>
    /// Gets the identity conditions.
    /// </summary>
    public List<CopyParameter> IdentityConditions { get; private set; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(CopyDataHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return
        other.TargetColumnName==TargetColumnName
          && other.SourceColumnName==TargetColumnName
            && IdentityConditions.Except(other.IdentityConditions)
              .Union(other.IdentityConditions.Except(IdentityConditions)).Any();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (CopyDataHint))
        return false;
      return Equals((CopyDataHint) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int hashCode;
      unchecked {
        hashCode = 
          TargetColumnName.GetHashCode() * 397 ^ 
            SourceColumnName.GetHashCode();
        foreach (var condition in IdentityConditions)
          hashCode ^= condition.GetHashCode();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorEq" copy="true"/>
    /// </summary>
    public static bool operator ==(CopyDataHint left, CopyDataHint right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true"/>
    /// </summary>
    public static bool operator !=(CopyDataHint left, CopyDataHint right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override void Translate(HintSet target)
    {
      target.Add(new CopyHint(SourceColumnName, TargetColumnName, IdentityConditions));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}: {1} -> {2} where {3}",
        "Copy node", SourceColumnName, TargetColumnName,
        string.Join(", ", IdentityConditions.Select(
          copyCondition => copyCondition.ToString()).ToArray()));
    }


     // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceColumnName">The current node name.</param>
    /// <param name="targetColumnName">The old node name.</param>
    public CopyDataHint(string sourceColumnName, string targetColumnName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceColumnName, "sourceColumnName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetColumnName, "targetColumnName");

      TargetColumnName = targetColumnName;
      SourceColumnName = sourceColumnName;
      IdentityConditions = new List<CopyParameter>();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceColumnName">The current node name.</param>
    /// <param name="targetColumnName">The old node name.</param>
    /// <param name="targetIdentityColumnName">Name of the identity column.</param>
    /// <param name="sourceIdentityColumnName">Name of the identity column in source model.</param>
    public CopyDataHint(string sourceColumnName, string targetColumnName, 
      string sourceIdentityColumnName, string targetIdentityColumnName)
      : this(sourceColumnName, targetColumnName)
    {
      IdentityConditions.Add(new CopyParameter(sourceIdentityColumnName, targetIdentityColumnName));
    }
  }
}