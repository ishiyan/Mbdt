using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace EuronextInstrumentIndexConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string[] commandArgs = null;

        public static string[] CommandArgs { get { return commandArgs; } }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (null != e.Args && 0 < e.Args.Length)
                commandArgs = e.Args;
        }
    }
}
