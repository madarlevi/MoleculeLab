using System.Collections.Generic;
using Avalonia.Controls;

namespace MoleculeLab.Views;

public class GraphNode
{
    public string Symbol { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public Button ButtonRef { get; set; } = null!;

    public List<(GraphNode Neighbor, int BondType)> Neighbors { get; set; } = new();
}

public static class MolecularGraphBuilder
{
    public static List<List<GraphNode>> BuildIndependentMolecules(List<CanvasAtomInfo> atoms, List<CanvasBondInfo> bonds)
    {
        var nodeMap = new Dictionary<Button, GraphNode>();

        foreach (var atom in atoms)
        {
            var node = new GraphNode
            {
                Symbol = atom.Symbol,
                X = atom.X,
                Y = atom.Y,
                ButtonRef = atom.ButtonRef
            };
            nodeMap[atom.ButtonRef] = node;
        }

        foreach (var bond in bonds)
        {
            if (nodeMap.ContainsKey(bond.Atom1Button) && nodeMap.ContainsKey(bond.Atom2Button))
            {
                var node1 = nodeMap[bond.Atom1Button];
                var node2 = nodeMap[bond.Atom2Button];

                int bondValue = bond.BondType switch
                {
                    "DoubleBond" => 2,
                    "TripleBond" => 3,
                    _ => 1
                };

                node1.Neighbors.Add((node2, bondValue));
                node2.Neighbors.Add((node1, bondValue));
            }
        }

        var allNodes = new List<GraphNode>(nodeMap.Values);
        var visited = new HashSet<GraphNode>();
        var independentMolecules = new List<List<GraphNode>>();

        foreach (var node in allNodes)
        {
            if (!visited.Contains(node))
            {
                var currentMolecule = new List<GraphNode>();
                TraverseMolecule(node, visited, currentMolecule);
                independentMolecules.Add(currentMolecule);
            }
        }

        return independentMolecules;
    }

    private static void TraverseMolecule(GraphNode current, HashSet<GraphNode> visited, List<GraphNode> moleculeList)
    {
        visited.Add(current);
        moleculeList.Add(current);

        foreach (var (neighbor, bondType) in current.Neighbors)
        {
            if (!visited.Contains(neighbor))
            {
                TraverseMolecule(neighbor, visited, moleculeList);
            }
        }
    }
}