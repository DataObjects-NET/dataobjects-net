// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Providers.Sql
{
  internal struct SqlScalarTask
  {
    public SqlScalarRequest Request;

    // Constructors

    public SqlScalarTask(SqlScalarRequest request)
    {
      Request = request;
    }
  }
}