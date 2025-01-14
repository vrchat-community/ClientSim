#if UNITY_EDITOR

using UnityEditor.PackageManager;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimResourceLoader
    {
        private const string PACKAGE_PATH = "Packages/com.vrchat.clientsim";

        public static PackageInfo GetPackageInfo()
        {    
            return PackageInfo.FindForAssetPath(PACKAGE_PATH);
        }

        public static string GetVersion()
        {
            var package = GetPackageInfo();
            if (package != null)
            {
                return package.version;
            }

            return "";
        }

        public static string GetPackagePath()
        {
            var package = GetPackageInfo();
            if (package != null)
            {
                return package.assetPath;
            }

            return "";
        }

        public static string GetResolvePath()
        {
            var package = GetPackageInfo();
            if (package != null)
            {
                return package.resolvedPath;
            }
            
            // TODO find package path based on known asset in the case where a user installs ClientSim not as a package.
            return "";
        }
    }
}
#endif