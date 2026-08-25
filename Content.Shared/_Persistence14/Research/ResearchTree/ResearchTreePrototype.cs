using System.Linq;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.ResearchTree;

/// <summary>
/// A simple prototype for storing the node graph. Allows multiple sources to easily use the same graph.
/// </summary>
[Prototype]
public sealed partial class ResearchTreePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("node", required: true)]
    private List<ResearchNode> _nodes = new();

    public Dictionary<ProtoId<TechnologyPrototype>, ResearchNode> GetNodes()
    {
        var dict = new Dictionary<ProtoId<TechnologyPrototype>, ResearchNode>();

        foreach (var node in _nodes)
        {
            if (dict.ContainsKey(node.Technology))
                continue; // TODO: Send some sort of error...

            dict.Add(node.Technology, node);
        }

        return dict;
    }
}