// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Indexing.Composite;

namespace Xtensive.Indexing
{
  internal class EntireFactoryProvider : AssociateProvider
  {
    private static readonly EntireFactoryProvider @default = new EntireFactoryProvider();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy=true />
    public static EntireFactoryProvider Default
    {
      get { return @default; }
    }

    /// <summary>
    /// Gets comparer for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the <see cref="IEntireFactory{T}"/> instance for.</typeparam>
    /// <returns><see cref="IEntireFactory{T}"/> instance for <typeparamref name="T"/> type.</returns>
    public IEntireFactory<T> GetFactory<T>()
    {
      return GetAssociate<T, IEntireFactory<T>, IEntireFactory<T>>();
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public EntireFactoryProvider()
    {
      TypeSuffixes = new string[] {"EntireFactory"};
      ConstructorParams = new object[] {};
      Type t = typeof (TupleEntireFactory);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
      //t = typeof (SegmentBoundEntireFactory<>);
      //AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}