// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Attributes
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class KeyProviderAttribute : Attribute
  {
    private Type[] fields;

    public Type[] Fields
    {
      get { return fields; }
      set { fields = value; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The first key field.</param>
    /// <param name="fields">The other (optional) key fields.</param>
    public KeyProviderAttribute(Type field, params Type[] fields)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (fields == null || fields.Length == 0)
        this.fields = new Type[] { field };

      this.fields = new Type[fields.Length+1];
      this.fields[0] = field;
      Array.Copy(fields, 0, Fields, 1, fields.Length);
    }
  }
}