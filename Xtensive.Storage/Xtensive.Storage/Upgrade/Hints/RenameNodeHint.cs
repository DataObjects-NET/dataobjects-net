// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Rename storage model node hint.
  /// </summary>
  [Serializable]
  public sealed class RenameNodeHint :
    IEquatable<RenameNodeHint>
  {
    /// <summary>
    /// Gets the current node name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the old node name.
    /// </summary>
    public string OldName { get; private set; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(RenameNodeHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return 
        other.Name==Name 
          && other.OldName==OldName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (RenameNodeHint))
        return false;
      return Equals((RenameNodeHint) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return 
          ((Name!=null ? Name.GetHashCode() : 0) * 397) ^ 
            (OldName!=null ? OldName.GetHashCode() : 0);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorEq" copy="true"/>
    /// </summary>
    public static bool operator ==(RenameNodeHint left, RenameNodeHint right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true"/>
    /// </summary>
    public static bool operator !=(RenameNodeHint left, RenameNodeHint right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}: {1} -> {2}", 
        "Rename node", OldName, Name);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The current node name.</param>
    /// <param name="oldName">The old node name.</param>
    public RenameNodeHint(string name, string oldName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldName, "oldName");
      Name = name;
      OldName = oldName;
    }
  }
}