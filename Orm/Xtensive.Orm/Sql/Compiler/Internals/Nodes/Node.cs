// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Node in SQL DOM query model.
  /// </summary>
  public abstract class Node
  {
    internal readonly bool IsTextNode;

    internal abstract void AcceptVisitor(NodeVisitor visitor);

    public Node()
    {
    }

    internal Node(bool isTextNode)
    {
      IsTextNode = isTextNode;
    }
  }
}