// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.12

using System;
using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents an object that describes <see cref="Type"/>.
  /// </summary>
  [Serializable]
  public class TypeInfo: ITypeInfo
  {
    private readonly AssemblyName assemblyName;
    private readonly string typeName;
    private readonly IAttributeInfo[] attributes;

    #region ITypeInfo Members

    /// <summary>
    /// Gets the type's <see cref="System.Type.FullName"/>.
    /// </summary>
    /// <value>The full name of the type.</value>
    public string TypeName
    {
      get { return typeName; }
    }

    /// <summary>
    /// Gets the <see cref="System.Reflection.AssemblyName"/> for the assembly that contains this instance.
    /// </summary>
    /// <value>The <see cref="System.Reflection.AssemblyName"/>.</value>
    public AssemblyName AssemblyName
    {
      get { return assemblyName; }
    }

    /// <summary>
    /// Gets the array of <see cref="IAttributeInfo"/> objects for the type.
    /// </summary>
    /// <returns></returns>
    /// <value>The array of <see cref="IAttributeInfo"/> objects for the type.</value>
    public IAttributeInfo[] GetAttributes()
    {
      return attributes;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeInfo"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    public TypeInfo(Type type)
      : this(type, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeInfo"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="attributes">The type attributes.</param>
    public TypeInfo(Type type, IAttributeInfo[] attributes)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      assemblyName = type.Assembly.GetName();
      typeName = type.FullName;
      this.attributes = attributes;
    }

    #endregion
  }
}