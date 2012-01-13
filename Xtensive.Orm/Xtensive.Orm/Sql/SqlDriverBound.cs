// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.25

using Xtensive.Internals.DocTemplates;

namespace Xtensive.Sql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> bound object.
  /// </summary>
  public abstract class SqlDriverBound
  {
    /// <summary>
    /// Gets the driver.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="driver">The driver.</param>
    public SqlDriverBound(SqlDriver driver)
    {
      Driver = driver;
    }
  }
}