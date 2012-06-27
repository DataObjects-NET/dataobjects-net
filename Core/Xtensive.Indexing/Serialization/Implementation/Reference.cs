// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Serialization.Implementation
{
  /// <summary>
  /// Reference to the object.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Value = {Value}")]
  public class Reference : IReference, 
    IEquatable<Reference>
  {
    private readonly string value;

    /// <summary>
    /// Reference to <see langword="null" />.
    /// </summary>
    public static Reference Null = new Reference(null);

    /// <inheritdoc/>
    public string Value {
      get { return value; }
    }

    /// <inheritdoc/>
    public bool TryResolve(out object target)
    {
      return SerializationContext.Current.ReferenceManager.TryResolve(this, out target);
    }

    /// <inheritdoc/>
    public bool IsCacheable 
    {
      get { return !this.IsNull(); }
    }

    /// <inheritdoc/>
    public bool IsQueueable 
    {
      get { return !this.IsNull(); }
    }

    #region Equals, GetHashCode methods

    public bool Equals(Reference obj)
    {
      return obj.value==value;
    }

    bool IEquatable<IReference>.Equals(IReference other)
    {
      return Equals(other);
    }

    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Reference))
        return false;
      return Equals((Reference) obj);
    }

    public override int GetHashCode()
    {
      return (value!=null ? value.GetHashCode() : 0);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.ReferenceFormat, Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    public Reference(string value)
    {
      this.value = value;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="target">The object this reference is pointing to.</param>
    public Reference(object target)
    {
      var rm = SerializationContext.Current.ReferenceManager;
      value = rm.GetNextReferenceValue();
      rm.Define(this, target);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    /// <param name="target">The object this reference is pointing to.</param>
    public Reference(string value, object target)
    {
      this.value = value;
      SerializationContext.Current.ReferenceManager.Define(this, target);
    }
  }
}