// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.03

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ReferenceComparer<T> : INodeComparer<T>
  {
    private readonly INodeComparerProvider provider;
    private NodeComparer<T> comparer;

    public IComparisonResult<T> Compare(T originalNode, T newNode)
    {
      
      if (comparer == null)
        comparer = provider.GetNodeComparer<T>();
      throw new System.NotImplementedException();
    }

    public INodeComparerProvider Provider
    {
      get { return provider; }
    }

    public ReferenceComparer(INodeComparerProvider provider)
    {
      this.provider = provider;
    }
  }
}