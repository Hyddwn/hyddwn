﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Globalization;
using System.Threading;

namespace Aura.Shared.Util.Configuration.Files
{
    /// <summary>
    ///     Represents localization.conf
    /// </summary>
    public class LocalizationConfFile : ConfFile
    {
        public string Language { get; protected set; }
        public string Culture { get; protected set; }
        public string CultureUi { get; protected set; }

        public void Load()
        {
            Require("system/conf/localization.conf");

            Language = GetString("language", "en-US");
            Culture = GetString("culture", "en-US");
            CultureUi = GetString("culture_ui", "en-US");

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(Culture);
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(CultureUi);
            Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture;
        }
    }
}