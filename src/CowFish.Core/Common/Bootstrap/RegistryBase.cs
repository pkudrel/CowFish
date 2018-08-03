namespace CowFish.Core.Common.Bootstrap
{
    public abstract class RegistryBase
    {
        protected RegistryBase(AppEnvironment env)
        {
            RootPath = env.RootDir;
            LogPath = env.LogDir;
            VarPath = env.VarDir;
            ConfigPath = env.ConfigDir;
            AppVersion = env.AppVersion;
        }

        public string LogPath { get; }
        public string ConfigPath { get; }
        public string RootPath { get; }
        public string VarPath { get; }
        public AppVersion AppVersion { get; }
    }
}