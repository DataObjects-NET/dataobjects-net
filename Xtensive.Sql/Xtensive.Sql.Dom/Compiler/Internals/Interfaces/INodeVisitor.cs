// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal interface INodeVisitor
  {
    void Visit(TextNode node);
    void Visit(NodeContainer node);
    void Visit(NodeDelimiter node);
    void Visit(VariantNode node);
  }
}