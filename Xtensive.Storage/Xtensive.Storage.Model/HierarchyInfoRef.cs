// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="HierarchyInfo"/> instance.
  /// </summary>
  [Serializable]
  public sealed class HierarchyInfoRef
  {
    private const string ToStringFormat = "Hierarchy '{0}'";

    /// <summary>
    /// Name of the base type in the hierarchy.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="HierarchyInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public HierarchyInfo Resolve(DomainModel model)
    {
      TypeInfo type;
      if (!model.Types.TryGetValue(TypeName, out type))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));

      return type.Hierarchy;
    }

    /// <summary>
    /// Creates reference for <see cref="HierarchyInfo"/>.
    /// </summary>
    public static implicit operator HierarchyInfoRef (HierarchyInfo hierarchyInfo)
    {
      return new HierarchyInfoRef(hierarchyInfo);
    }

    #region Equality members, ==, !=

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator !=(HierarchyInfoRef x, HierarchyInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator ==(HierarchyInfoRef x, HierarchyInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(HierarchyInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        TypeName==TypeName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as HierarchyInfoRef);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return unchecked( TypeName.GetHashCode() );
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, TypeName);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchyInfo"><see cref="HierarchyInfo"/> object to make reference for.</param>
    public HierarchyInfoRef(HierarchyInfo hierarchyInfo)
    {
      TypeName = hierarchyInfo.Root.Name;      
    }
  }
}