// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;

namespace Xtensive.Orm
{
  public interface IQueryEndpointProvider : IDomainService
  {
    QueryEndpoint GetQueryEndpoint(Session session);
  }
}