// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Globalization;

namespace Xtensive.Orm.Weaver
{
  public sealed class MessageLocation
  {
    private readonly string file;
    private readonly int line;
    private readonly int column;

    public string File
    {
      get { return file; }
    }

    public int Line
    {
      get { return line; }
    }

    public int Column
    {
      get { return column; }
    }

    public override string ToString()
    {
      return string.IsNullOrEmpty(File)
        ? "<unknown>"
        : string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", file, line, column);
    }

    public MessageLocation(string file = null, int line = 0, int column = 0)
    {
      this.file = file;
      this.line = line;
      this.column = column;
    }
  }
}