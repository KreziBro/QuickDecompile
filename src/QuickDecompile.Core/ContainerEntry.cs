namespace QuickDecompile.Core;

public sealed class ContainerEntry
{
    public required string EntryPath { get; init; }

    public required string DisplayName { get; init; }

    public bool IsContainer { get; init; }

    public List<ContainerEntry> Children { get; } = [];
}
