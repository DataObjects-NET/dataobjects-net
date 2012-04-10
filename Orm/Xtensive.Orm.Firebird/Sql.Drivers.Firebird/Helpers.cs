// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.13


namespace Xtensive.Sql.Drivers.Firebird
{
  public static class ModelExtensions
  {
    public static void Dump(this Model.Table table, int level, System.IO.TextWriter target)
    {
      target.WriteLine(string.Format("{0} Table:{1}", " ".PadRight(level), table.Name));
    }

    public static void Dump(this Model.View view, int level, System.IO.TextWriter target)
    {
      target.WriteLine(string.Format("{0} View:{1}", " ".PadRight(level), view.Name));
    }
  }
}