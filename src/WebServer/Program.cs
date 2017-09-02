﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Util;

namespace Aura.Web
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                WebServer.Instance.Run();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "An exception occured while starting the server.");
                CliUtil.Exit(1);
            }
        }
    }
}