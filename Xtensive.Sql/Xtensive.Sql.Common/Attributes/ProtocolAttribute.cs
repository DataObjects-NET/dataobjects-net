// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Common.Resources;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Allows to specify protocol names those will identify certain RDBMS <see cref="Driver"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// It is necessary to mark any <see cref="Driver"/> descendant that is intended to be
  /// instantiated by <see cref="ConnectionProvider"/> by <see cref="ProtocolAttribute"/>
  /// and specify all protocol names the <see cref="Driver"/> will satisfy in the
  /// attribute constructor.
  /// </para>
  /// <para>
  /// This allows <see cref="ConnectionProvider"/> to identify desirable driver by 
  /// protocol name in a connection string.
  /// </para>
  /// </remarks>
  /// <example>
  /// You can apply the <see cref="ProtocolAttribute"/> on your own driver as following:
  /// 
  /// After that it will be possible to use specified protocol names 
  /// in a connection strings, like the following one:
  /// 
  /// </example>
  /// <seealso cref="ConnectionProvider"/>
  /// <seealso cref="Driver"/>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public sealed class ProtocolAttribute: Attribute
  {
    private readonly string name;

    /// <summary>
    /// <para>
    /// Holds the protocol names.
    /// </para>
    /// <para>
    /// <see cref="ConnectionProvider"/> takes that name and uses it to identify
    /// underlying RDBMS driver.
    /// </para>
    /// </summary>
    public string Name
    {
      get { return name; }
    }

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance or null.</param>
    /// <returns>
    /// true if <paramref name="obj"/> equals the type and value of this instance; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      ProtocolAttribute protocolAttribute = obj as ProtocolAttribute;
      if (protocolAttribute == null)
        return false;
      if (!base.Equals(obj))
        return false;
      return Name == protocolAttribute.Name;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() + 29*Name.GetHashCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolAttribute"/> class.
    /// </summary>
    /// <param name="name">
    /// The protocol name to be used to identify <see cref="Driver">RDBMS driver</see>
    /// the <see cref="ProtocolAttribute"/> is applied to.
    /// </param>
    public ProtocolAttribute(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentException(Strings.ExProtocolCantBeNullOrEmptyString, "name");
      this.name = name;
    }
  }
}