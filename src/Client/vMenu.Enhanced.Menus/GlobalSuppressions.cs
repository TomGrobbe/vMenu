
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1846:Prefer 'AsSpan' over 'Substring'", Justification = "As span is blocked in sandboxing")]
