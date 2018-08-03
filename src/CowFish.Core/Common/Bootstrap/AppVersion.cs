namespace CowFish.Core.Common.Bootstrap
{
    public class AppVersion
    {
        public AppVersion(string assemblyVersion, string assemblyFileVersion, string assemblyName)
        {
            AssemblyVersion = assemblyVersion;
            AssemblyFileVersion = assemblyFileVersion;
            AssemblyName = assemblyName;
        }

        public AppVersion(string assemblyVersion, string assemblyFileVersion, string assemblyName,
            string assemblyProductVersion, string semVer, string buildCounter,
            string branch, string dateTime, string env, string sha, string commitsCounter)
        {
            AssemblyVersion = assemblyVersion;
            AssemblyFileVersion = assemblyFileVersion;
            AssemblyName = assemblyName;
            AssemblyProductVersion = assemblyProductVersion;
            SemVer = semVer;
            BuildCounter = buildCounter;
            Branch = branch;
            DateTime = dateTime;
            Env = env;
            Sha = sha;
            CommitsCounter = commitsCounter;
        }

        public string AssemblyVersion { get; }
        public string AssemblyFileVersion { get; }
        public string AssemblyName { get; set; }
        public string AssemblyProductVersion { get; }
        public string SemVer { get; }
        public string BuildCounter { get; }
        public string Branch { get; }
        public string DateTime { get; }
        public string Env { get; }
        public string Sha { get; }
        public string CommitsCounter { get; }

        public string MainVersion => string.IsNullOrEmpty(SemVer) ? AssemblyFileVersion : SemVer;
        public string FullName => $"{AssemblyName} {MainVersion}";
        public string FullInfo => string.IsNullOrEmpty(AssemblyProductVersion) ? FullName : $"{AssemblyName} {AssemblyProductVersion}";

        public override string ToString()
        {
            return MainVersion;
        }
    }
}