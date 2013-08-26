// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

namespace Xtensive.Orm.Configuration
{
  internal sealed class IgnoreRuleConstructionFlow : IIgnoreRuleConstructionFlow
  {
    private readonly IgnoreRule target;

    /// <inheritdoc/>
    public IIgnoreRuleConstructionFlow WhenDatabase(string databaseName)
    {
      target.Database = databaseName;
      return this;
    }

    /// <inheritdoc/>
    public IIgnoreRuleConstructionFlow WhenSchema(string schemaName)
    {
      target.Schema = schemaName;
      return this;
    }

    /// <inheritdoc/>
    public IIgnoreRuleConstructionFlow WhenTable(string tableName)
    {
      target.Table = tableName;
      return this;
    }

    //constructor

    /// <summary>
    /// Create instance of <see cref="IgnoreRuleConstructionFlow"/>
    /// </summary>
    /// <param name="target"></param>
    public IgnoreRuleConstructionFlow(IgnoreRule target)
    {
      this.target = target;
    }
  }
}