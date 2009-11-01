// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom
{
  public interface ISqlNode : ICloneable
  {
    SqlNodeType NodeType { get; }
    void AcceptVisitor(ISqlVisitor visitor);
  }
}