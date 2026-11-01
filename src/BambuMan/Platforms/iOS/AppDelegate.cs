using BambuMan.Implementations;
using BambuMan.Interfaces;
using BambuMan.Shared.Interfaces;
using Foundation;
using Serilog;

namespace BambuMan
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public static void SetupImplementations(IServiceCollection services)
        {
            services.AddSingleton<IToneGenerator, IosToneGenerator>();
            services.AddTransient<IInvokeIndent, IosInvokeIndent>();
            services.AddSingleton<ISoftKeyboard, IosSoftKeyboard>();
            services.AddSingleton<ITagProbe, IosTagProbe>();
        }

        public static void InitBuildVersion()
        {
            BuildVersionModel.CurrentBuildVersion = AppInfo.Current.VersionString;
            BuildVersionModel.PackageFullName = AppInfo.Current.PackageName;
        }

        public static void SetupSerilog()
        {
            var logConfig = new LoggerConfiguration();
            logConfig.WriteTo.Debug();

            MauiProgram.SetupSerilog(logConfig, AppInfo.Current.VersionString, AppInfo.Current.PackageName);
        }
    }
}
