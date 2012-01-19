// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using System;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public enum SqlCommandType
  {
    SetConstraintsAllDeferred,
    SetConstraintsAllImmediate,
  }
}