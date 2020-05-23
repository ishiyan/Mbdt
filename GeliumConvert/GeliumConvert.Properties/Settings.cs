using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace GeliumConvert.Properties
{
	[CompilerGenerated, GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}
		[DebuggerNonUserCode, DefaultSettingValue("repository\\gelium_fx\\"), ApplicationScopedSetting]
		public string RepositoryDir
		{
			get
			{
				return (string)this["RepositoryDir"];
			}
		}
		[DefaultSettingValue("/gelium/"), ApplicationScopedSetting, DebuggerNonUserCode]
		public string InstrumentPath
		{
			get
			{
				return (string)this["InstrumentPath"];
			}
		}
		[DefaultSettingValue("True"), ApplicationScopedSetting, DebuggerNonUserCode]
		public bool Hdf5CorkTheCache
		{
			get
			{
				return (bool)this["Hdf5CorkTheCache"];
			}
		}
		[ApplicationScopedSetting, DebuggerNonUserCode, DefaultSettingValue("1048576")]
		public int Hdf5MaxReadBufferBytes
		{
			get
			{
				return (int)this["Hdf5MaxReadBufferBytes"];
			}
		}
		[DebuggerNonUserCode, DefaultSettingValue("True"), ApplicationScopedSetting]
		public bool UpdateExisting
		{
			get
			{
				return (bool)this["UpdateExisting"];
			}
		}
		[ApplicationScopedSetting, DefaultSettingValue("0"), DebuggerNonUserCode]
		public int AddHours
		{
			get
			{
				return (int)this["AddHours"];
			}
		}
	}
}
