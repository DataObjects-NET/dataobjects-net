// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.10

using System;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public abstract class SqlCascadableAction : SqlAction
  {
    public bool Cascade { get; private set; }

    // Constructors

    protected SqlCascadableAction(bool cascade)
    {
      Cascade = cascade;
    }
  }
}