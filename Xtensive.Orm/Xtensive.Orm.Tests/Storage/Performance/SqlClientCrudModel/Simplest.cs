// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.23

namespace Xtensive.Orm.Tests.Storage.Performance.SqlClientCrudModel
{
  public class Simplest
  {
    public long Id { get; set; }

    public long Value { get; set; }

    
    // Constructors

    public Simplest()
    {
    }

    public Simplest(long id, long value)
    {
      Id = id;
      Value = value;
    }
  }
}