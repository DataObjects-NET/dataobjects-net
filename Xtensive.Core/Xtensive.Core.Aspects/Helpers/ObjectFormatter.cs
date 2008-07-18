// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  internal class ObjectFormatter
  {
    private readonly bool formatContentOnly;
    private readonly object obj;


    public bool FormatContentOnly
    {
      get { return formatContentOnly; }
    }

    public object Object
    {
      get { return obj; }
    }

    public override string ToString()
    {
      return ToString(obj, formatContentOnly);
    }

    public static string ToString(object obj)
    {
      return ToString(obj, false);
    }

    public static string ToString(object obj, bool formatContentOnly)
    {
      StringBuilder sb = new StringBuilder();
      AppendObject(sb, obj, formatContentOnly);
      return sb.ToString();
    }

    private static void AppendObject(StringBuilder sb, object obj, bool formatContentOnly)
    {
      if (obj==null)
        sb.Append("null");
      else if (obj is string)
        sb.AppendFormat("\"{0}\"", obj);
      else if (obj is Array) {
        if (!formatContentOnly) 
          sb.Append("{");
        AppendArrayContent(sb, obj as Array);
        if (!formatContentOnly) 
          sb.Append("}");
      }
      else
        sb.Append(obj.ToString());
    }

    private static void AppendArrayContent(StringBuilder sb, Array array)
    {
      int length = array.Length;
      for (int i = 0; i<length; i++) {
        if (i!=0)
          sb.Append(", ");
        AppendObject(sb, array.GetValue(i), false);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="obj">The object to format.</param>
    public ObjectFormatter(object obj)
      : this(obj, false)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="obj">The object to format.</param>
    /// <param name="formatContentOnly">if set to <c>true</c> format content only.</param>
    public ObjectFormatter(object obj, bool formatContentOnly)
    {
      this.formatContentOnly = formatContentOnly;
      this.obj  = obj;
    }
  }
}