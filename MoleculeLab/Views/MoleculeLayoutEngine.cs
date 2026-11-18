using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace MoleculeLab.Views;

public static class MoleculeLayoutEngine
{
    public const double BondLength = 65.0;

    public static void ArrangeMolecule(List<GraphNode> moleculeNodes, double centerX, double centerY)
    {
        if (moleculeNodes.Count == 0) return;

        if (moleculeNodes.Count == 1)
        {
            SetNodePosition(moleculeNodes[0], centerX, centerY);
            return;
        }

        var carbonChain = moleculeNodes.Where(n => n.Symbol == "C").ToList();

        if (carbonChain.Count > 1)
        {
            double startX = centerX - ((carbonChain.Count - 1) * BondLength / 2);
            for (int i = 0; i < carbonChain.Count; i++)
            {
                SetNodePosition(carbonChain[i], startX + (i * BondLength), centerY);
            }

            foreach (var carbon in carbonChain)
            {
                var nonCarbons = carbon.Neighbors.Where(n => n.Neighbor.Symbol != "C").Select(n => n.Neighbor).ToList();
                int count = nonCarbons.Count;
                if (count == 0) continue;

                for (int i = 0; i < count; i++)
                {
                    double angle;
                    if (count == 3)
                    {
                        angle = i switch
                        {
                            0 => -Math.PI / 2,
                            1 => Math.PI / 2,
                            _ => (carbon == carbonChain.First()) ? Math.PI : 0
                        };
                    }
                    else if (count == 2)
                    {
                        angle = (i == 0) ? -Math.PI / 2 : Math.PI / 2;
                    }
                    else
                    {
                        angle = (i * 2 * Math.PI) / count;
                    }

                    double hX = carbon.X + BondLength * 0.7 * Math.Cos(angle);
                    double hY = carbon.Y + BondLength * 0.7 * Math.Sin(angle);
                    SetNodePosition(nonCarbons[i], hX, hY);
                }
            }
            return;
        }

        GraphNode centerNode = moleculeNodes.OrderByDescending(n => n.Neighbors.Count).First();
        SetNodePosition(centerNode, centerX, centerY);

        int neighborCount = centerNode.Neighbors.Count;
        if (neighborCount == 2 && (centerNode.Symbol == "O" || centerNode.Symbol == "S"))
        {
            SetNodePosition(centerNode.Neighbors[0].Neighbor, centerX - 45, centerY + 40);
            SetNodePosition(centerNode.Neighbors[1].Neighbor, centerX + 45, centerY + 40);
            return;
        }
        else if (neighborCount == 2 && centerNode.Symbol == "C")
        {
            SetNodePosition(centerNode.Neighbors[0].Neighbor, centerX - BondLength, centerY);
            SetNodePosition(centerNode.Neighbors[1].Neighbor, centerX + BondLength, centerY);
            return;
        }

        double angleStep = (2 * Math.PI) / neighborCount;
        for (int i = 0; i < neighborCount; i++)
        {
            double angle = i * angleStep;
            SetNodePosition(centerNode.Neighbors[i].Neighbor, centerX + BondLength * Math.Cos(angle), centerY + BondLength * Math.Sin(angle));
        }
    }

    private static void SetNodePosition(GraphNode node, double x, double y)
    {
        node.X = x;
        node.Y = y;
        Canvas.SetLeft(node.ButtonRef, x - 18);
        Canvas.SetTop(node.ButtonRef, y - 18);
    }
}