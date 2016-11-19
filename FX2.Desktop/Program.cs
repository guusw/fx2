// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Diagnostics;
using FX2.Game;
using osu.Framework;
using osu.Framework.Desktop;
using osu.Framework.Desktop.Platform;

namespace FX2.Desktop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            using(DesktopGameHost host = Host.GetSuitableHost(@"FX2", true))
            {
                BaseGame osu = new FXGame(args);
                host.Add(osu);
                host.Run();
                return 0;
            }
        }
    }
}
