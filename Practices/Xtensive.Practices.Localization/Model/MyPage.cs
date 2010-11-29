// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;

namespace Xtensive.Practices.Localization.Model
{
  [Serializable]
  public class MyPage : Page
  {
    public string MyContent
    {
      get { return Localizations.Current.MyContent; }
      set { Localizations.Current.MyContent = value; }
    }
  }
}