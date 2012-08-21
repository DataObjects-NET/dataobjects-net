// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.21

using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Drivers.Sqlite.Resources;

namespace Xtensive.Sql.Drivers.Sqlite
{
  internal static class ProviderInitializer
  {
    private const string NativeModuleResourceNameFormat = "Xtensive.Sql.Drivers.Sqlite.NativeModules.{0}_{1}.SQLite.Interop.dll";
    private const string NativeModuleFileNameFormat = @"Xtensive\{0}\Native\{1}\SQLite.Interop.dll";

#if NET40
    private const string FrameworkName = "Net40";
#else
    private const string FrameworkName = "Net35";
#endif

    private static volatile bool IsInitialized;
    private static readonly object SyncRoot = new object();

    public static void Run()
    {
      if (IsInitialized)
        return;
      lock (SyncRoot) {
        if (IsInitialized)
          return;
        IsInitialized = true;
        LoadNativeModule();
        RegisterCollations();
      }
    }

    private static Stream GetNativeModuleStream()
    {
      var resourceName = string.Format(NativeModuleResourceNameFormat, FrameworkName, IntPtr.Size * 8);
      var result = typeof (ProviderInitializer).Assembly.GetManifestResourceStream(resourceName);
      if (result==null)
        throw new InvalidOperationException(string.Format(Strings.ResourceXIsMissing, resourceName));
      return result;
    }

    private static string GetNativeModuleHash()
    {
      using (var hashProvider = new SHA1Managed()) {
        hashProvider.Initialize();
        using (var stream = GetNativeModuleStream())
          hashProvider.ComputeHash(stream);
        var hash = hashProvider.Hash.Take(8).ToArray();
        return new StringBuilder().AppendHexArray(hash).ToString();
      }
    }

    private static string GetNativeModuleFileName()
    {
      return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        string.Format(NativeModuleFileNameFormat, ThisAssembly.ProductName, GetNativeModuleHash()));
    }

    private static void WriteNativeModuleToFile(string moduleFileName)
    {
      var moduleDirName = Path.GetDirectoryName(moduleFileName);
      if (!Directory.Exists(moduleDirName))
        Directory.CreateDirectory(moduleDirName);
      using (var resourceStream = GetNativeModuleStream())
      using (var fileStream = File.Create(moduleFileName))
        CopyStream(resourceStream, fileStream);
    }

    private static void CopyStream(Stream source, Stream target)
    {
      var buffer = new byte[1024 * 32];
      int count;
      while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
        target.Write(buffer, 0, count);
    }

    private static void LoadNativeModule()
    {
      var moduleFileName = GetNativeModuleFileName();

      if (!File.Exists(moduleFileName))
        WriteNativeModuleToFile(moduleFileName);

      // LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008

      LoadLibraryEx(moduleFileName, IntPtr.Zero, 0x00000008);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    private static void RegisterCollations()
    {
      // Register our helper collations

      var functions = typeof (DriverFactory).Assembly.GetTypes()
        .Where(t => typeof (SQLiteFunction).IsAssignableFrom(t));

      foreach (var f in functions)
        SQLiteFunction.RegisterFunction(f);
    }
  }
}