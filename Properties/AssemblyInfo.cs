using System.Reflection;
using System.Resources;

// General Information
[assembly: AssemblyTitle("MooseSounds")]
[assembly: AssemblyDescription("Adds moose death/run sounds")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tommo J. Productions / Pentti \"Einari\" Kervinen")]
[assembly: AssemblyProduct("MooseSounds")]
[assembly: AssemblyCopyright("Tommo J. Productions / Pentti \"Einari\" Kervinen Copyright © 2022")]
[assembly: AssemblyTrademark("Azine")]
[assembly: NeutralResourcesLanguage("en-AU")]

// Version information
[assembly: AssemblyVersion("1.0.315.2")]
[assembly: AssemblyFileVersion("1.0.315.2")]

namespace TommoJProductions.MooseSounds
{

    public class VersionInfo
    {
	    public const string LASTEST_RELEASE = "13.11.2022 07:42 AM";
	    public const string VERSION = "1.0.315.2";

        /// <summary>
        /// Represents if the mod has been complied for x64
        /// </summary>
        #if x64
            internal const bool IS_64_BIT = true;
        #else
            internal const bool IS_64_BIT = false;
        #endif
        /// <summary>
        /// Represents if the mod has been complied in Debug mode
        /// </summary>
        #if DEBUG
            internal const bool IS_DEBUG_CONFIG = true;
        #else
            internal const bool IS_DEBUG_CONFIG = false;
        #endif
    }
}
