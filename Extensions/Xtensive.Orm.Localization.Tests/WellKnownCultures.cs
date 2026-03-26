using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Xtensive.Orm.Localization.Tests
{
  public static class WellKnownCultures
  {
    public static class English
    {
      public static CultureInfo Culture = new CultureInfo("en-US");
      public const string Title = "Welcome!";
      public const string Content = "My dear guests, welcome to my birthday party!";
    }

    public static class Spanish
    {
      public static CultureInfo Culture = new CultureInfo("es-ES");
      public const string Title = "Bienvenido!";
      public const string Content = "Mis amigos mejores! Bienvenido a mi cumpleanos!";
    }
  }
}
