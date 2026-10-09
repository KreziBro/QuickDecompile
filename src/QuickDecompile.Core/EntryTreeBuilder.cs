namespace QuickDecompile.Core;

public static class EntryTreeBuilder
{
    public static List<ContainerEntry> Build(IEnumerable<string> entryPaths)
    {
        var roots = new List<ContainerEntry>();
        var nodeMap = new Dictionary<string, ContainerEntry>();

        foreach (var path in entryPaths.OrderBy(x => x))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            ContainerEntry? parent = null;
            string cumulative = "";

            for (int i = 0; i < segments.Length; i++)
            {
                bool isLeaf = i == segments.Length - 1;
                cumulative = cumulative.Length == 0 ? segments[i] : $"{cumulative}/{segments[i]}";

                if (!nodeMap.TryGetValue(cumulative, out var node))
                {
                    node = new ContainerEntry
                    {
                        EntryPath   = isLeaf ? path : cumulative,
                        DisplayName = segments[i],
                        IsContainer = !isLeaf,
                    };
                    nodeMap[cumulative] = node;

                    if (parent == null)
                        roots.Add(node);
                    else
                        parent.Children.Add(node);
                }

                parent = node;
            }
        }

        return roots;
    }
}
