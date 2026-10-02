namespace ROCHEFORT6000.Core.Common
{
    /// <summary>
    /// Canonical application phase and build identification.
    /// Single source of truth for versioning across the UI, release builder, and analytics.
    /// Invariant: Lower-right frame must always display v-{Phase}-build{BuildNumber}.
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Current development / operational phase (e.g. "alpha4", "alpha5", "beta1").
        /// Change this when the operational phase advances; build tooling will adapt automatically.
        /// </summary>
        public const string Phase = "alpha4";

        /// <summary>
        /// Sequential build number within the active phase.
        /// Automatically synchronized and incremented by build_release.ps1.
        /// </summary>
        public const int BuildNumber = 33;

        /// <summary>
        /// Formatted display version string placed in the lower-right frame (e.g. "v-alpha4-build24").
        /// </summary>
        public static string DisplayVersion => $"v-{Phase}-build{BuildNumber}";

        /// <summary>
        /// Artifact executable filename corresponding to this phase and build.
        /// </summary>
        public static string ExecutableName => $"witp-cic-rochefort-{Phase}-{BuildNumber}.exe";
    }
}
