// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

namespace Xtensive.Orm.Configuration
{
  public interface IIgnoreRuleConstructionFlow
  {
    /// <summary>
    /// Adds tasgeted specified <paramref name="databaseName"/> to <see cref="IgnoreRule"/>
    /// </summary>
    /// <param name="databaseName">Value to add</param>
    /// <returns>Itself</returns>
    IIgnoreRuleConstructionFlow WhenDatabase(string databaseName);

    /// <summary>
    /// Adds tasgeted specified <paramref name="schemaName"/> to <see cref="IgnoreRule"/>
    /// </summary>
    /// <param name="schemaName">Value to add</param>
    /// <returns>Itself</returns>
    IIgnoreRuleConstructionFlow WhenSchema(string schemaName);

    /// <summary>
    /// Adds tasgeted specified <paramref name="tableName"/> to <see cref="IgnoreRule"/>
    /// </summary>
    /// <param name="tableName">Value to add</param>
    /// <returns>Itself</returns>
    IIgnoreRuleConstructionFlow WhenTable(string tableName);
  }
}