using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CowFish.Core.Common.Bootstrap
{
    public class AppEnvironmentBuilder
    {
        private const string _WORK_DIR = "work-dir";
        private const string _GLOBAL_DIR = "global-dir";
        private const string _LOG_DIR = "log";
        private const string _VAR_DIR = "var";
        private const string _DEV_DIR = ".dev";
        private const string _GIT_DIR = ".git";
        private const string _APP_VS_DIR = ".app.vs";
        private const string _SYRUP_DIR = ".syrup";
        private const string _DEV_FILE = "dev.json";
        private const string _CONFIG_DIR = "config";
        private const string _DROPBOX_DIR = "Dropbox";
        private static readonly AppEnvironmentBuilder _instance = new AppEnvironmentBuilder();

        private static AppEnvironment _appEnvironmentValue;
        private static readonly object _padlock = new object();

        static AppEnvironmentBuilder()
        {
        }

        private AppEnvironmentBuilder()
        {
        }

        // ReSharper disable once ConvertToAutoProperty
        public static AppEnvironmentBuilder Instance => _instance;

        public AppEnvironment GetAppEnvironment(Assembly asm)
        {
            if (_appEnvironmentValue == null)
                lock (_padlock)
                {
                    if (_appEnvironmentValue != null) return _appEnvironmentValue;
                    _appEnvironmentValue = GetTemporaryRegistryImpl(asm);
                }

            return _appEnvironmentValue;
        }


        public AppEnvironment GetAppEnvironment()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            return GetAppEnvironment(asm);
        }

        private AppEnvironment GetTemporaryRegistryImpl(Assembly asm)
        {
            var res = new AppEnvironment();

            // main 
            res.AssemblyFilePath = new Uri(asm.CodeBase).LocalPath;
            res.ExeFileDir = Path.GetDirectoryName(res.AssemblyFilePath);
            res.CommandLineArgs = Environment.GetCommandLineArgs();

            // syrup
            res.SyrupDir = FindDir(res.ExeFileDir, _SYRUP_DIR);
            res.IsSyrup = !string.IsNullOrEmpty(res.SyrupDir);

            // dev
            res.DevDir = FindDir(res.ExeFileDir, _DEV_DIR);
            var dv = GetDevSettings(res.DevDir);
            res.IsDeveloperMode = IsDeveloperMode(res.IsSyrup, res.DevDir, dv);

            // root
            res.RootDir = GetRoot(res.ExeFileDir, res.SyrupDir, res.DevDir);

            // important dev dirs
            res.GitDir = FindDir(res.RootDir, _GIT_DIR);
            res.RepositoryRootDir = GetRepositoryRoot(res.GitDir);

            // important app dirs
            res.LogDir = GetSimpleDir(res.RootDir, _LOG_DIR, res.IsDeveloperMode);
            res.ConfigDir = GetSimpleDir(res.RootDir, _CONFIG_DIR, res.IsDeveloperMode);
            res.VarDir = GetDir(res.RootDir, _VAR_DIR);
            res.WorkDir = GetDir(res.RootDir, _WORK_DIR);
            res.GlobalDir = GetGlobalDir(res.RootDir);

            // others
            res.AssemblyName = asm.GetName().Name;
            res.AppVersion = AppVersionBuilder.Init(asm);
            res.MachineName = Environment.MachineName;
            res.Is64BitProcess = Environment.Is64BitProcess;
            res.ProcessorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");


#if DEBUG
            res.IsDebug = true;
#else
            res.IsDebug = false;
#endif
            Helpers.CreateDirIfNotExist(res.ConfigDir);
            Helpers.CreateDirIfNotExist(res.LogDir);

            return res;
        }

        private string GetGlobalDir(string rootDir)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dropboxPath = Path.Combine(userProfile, _DROPBOX_DIR);
            return Directory.Exists(dropboxPath) ? dropboxPath : Path.Combine(rootDir, _GLOBAL_DIR);
        }


        private static string GetSimpleDir(string rootDir, string dirName, bool useShort)
        {
            return useShort ? Path.Combine(rootDir, dirName) : Path.Combine(rootDir, dirName, Environment.MachineName);
        }

        private static string GetDir(string rootDir, string dirName)
        {
            return Path.Combine(rootDir, dirName);
        }


        private static bool IsDeveloperMode(bool isSyrup, string devDir, DeveloperConfig dv)
        {
            if (isSyrup) return false;

            return Directory.Exists(devDir) && dv.IgnoreMe == false;
        }

        private static string GetRepositoryRoot(string gitDir)
        {
            var parentGit = string.IsNullOrEmpty(gitDir)
                ? string.Empty
                : new DirectoryInfo(gitDir)?.Parent?.FullName;

            return parentGit;
        }


        private string GetRoot(string appDir, string syrupDir, string devDir)
        {
            // syrup is present - use syrup
            if (!string.IsNullOrEmpty(syrupDir)) return new DirectoryInfo(syrupDir)?.Parent?.FullName;


            if (!string.IsNullOrEmpty(devDir))
            {
                var vsAppDir = Path.Combine(devDir, _APP_VS_DIR);
                if (Directory.Exists(vsAppDir)) return vsAppDir;
            }

            return appDir;
        }


        private static DeveloperConfig GetDevSettings(string devDir)
        {
            var res = new DeveloperConfig();
            if (!Directory.Exists(devDir)) return res;
            var devConfig = Path.Combine(devDir, _DEV_FILE);
            if (!File.Exists(devConfig)) return res;
            var json = File.ReadAllText(devConfig);
            var conf = JsonConvert.DeserializeObject<DeveloperConfig>(json);

            return conf;
        }


        private static string FindDir(string startPath, string dirToFind)
        {
            var di = new DirectoryInfo(startPath);
            while (true)
            {
                var path = Path.Combine(di.FullName, dirToFind);
                if (Directory.Exists(path))
                    return path;

                if (di.Parent == null) return null;
                di = di.Parent;
            }
        }


        internal class DevSettings
        {
            public bool IsDeveloperMode { get; set; }
            public DeveloperConfig DeveloperConfig { get; set; }
        }
    }
}