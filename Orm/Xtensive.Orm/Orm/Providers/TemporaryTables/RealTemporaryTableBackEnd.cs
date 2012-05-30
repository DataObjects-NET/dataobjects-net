// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.24

using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  public class RealTemporaryTableBackEnd : TemporaryTableBackEnd
  {
    public override Table CreateTemporaryTable(Schema schema, string tableName)
    {
      return schema.CreateTemporaryTable(tableName);
    }

    public override void InitializeTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      ExecuteNonQuery(context, descriptor.CreateStatement);
    }

    public override void AcquireTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
    }

    public override void ReleaseTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
    }
  }
}