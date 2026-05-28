namespace Catel.SourceGenerators.Generation;

internal static class SourceFileNameHelper
{
    private const int MaxLength = 100;

    /// <summary>
    /// Returns a unique generated file name prefix based on namespace and class name.
    /// If the combined name fits within <see cref="MaxLength"/> characters, it uses
    /// <c>{sanitized_namespace}_{className}</c>; otherwise it uses an 8-character
    /// deterministic hash of the namespace followed by the class name.
    /// </summary>
    internal static string GetGeneratedFileName(string namespaceName, string className)
    {
        var sanitizedNamespace = namespaceName.Replace('.', '_');
        var combined = $"{sanitizedNamespace}_{className}";

        if (combined.Length <= MaxLength)
        {
            return combined;
        }

        var hash = ComputeShortHash(namespaceName);
        return $"{hash}_{className}";
    }

    private static string ComputeShortHash(string input)
    {
        // FNV-1a 32-bit hash — simple, deterministic, and dependency-free
        unchecked
        {
            var hash = 2166136261u;
            foreach (var c in input)
            {
                hash ^= (uint)c;
                hash *= 16777619u;
            }

            return hash.ToString("X8");
        }
    }
}
