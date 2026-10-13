#if NETFRAMEWORK
namespace System.Runtime.CompilerServices;

// Lets records and init-only setters compile for net472 (RimWorld 1.6).
internal static class IsExternalInit;
#endif
