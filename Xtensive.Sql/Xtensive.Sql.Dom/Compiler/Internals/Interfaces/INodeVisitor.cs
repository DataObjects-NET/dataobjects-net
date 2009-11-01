// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Dom.Compiler.Internals;

namespace Xtensive.Sql.Dom.Compiler
{
  internal interface INodeVisitor
  {
    void Visit(TextNode node);
    void Visit(NodeContainer node);
    void Visit(NodeDelimiter node);
  }
}