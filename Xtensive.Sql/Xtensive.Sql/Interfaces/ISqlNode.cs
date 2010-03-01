// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  public interface ISqlNode : ICloneable
  {
    SqlNodeType NodeType { get; }
    void AcceptVisitor(ISqlVisitor visitor);
  }
}