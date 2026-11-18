using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace MoleculeLab.Views;

public class CanvasAtomInfo
{
    public Button ButtonRef { get; set; } = null!;
    public string Symbol { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}

public class CanvasBondInfo
{
    public Button Atom1Button { get; set; } = null!;
    public Button Atom2Button { get; set; } = null!;
    public string BondType { get; set; } = "SingleBond";
}

public static class CanvasMoleculeExtractor
{
    public static (List<CanvasAtomInfo> Atoms, List<CanvasBondInfo> Bonds) ExtractFromCanvas(Canvas canvas)
    {
        var atoms = new List<CanvasAtomInfo>();
        var rawBondCounts = new Dictionary<(Button, Button), int>();

        foreach (var child in canvas.Children)
        {
            if (child is Button atomButton)
            {
                double x = Canvas.GetLeft(atomButton);
                double y = Canvas.GetTop(atomButton);
                string symbol = atomButton.Content?.ToString() ?? "C";

                atoms.Add(new CanvasAtomInfo
                {
                    ButtonRef = atomButton,
                    Symbol = symbol,
                    X = x,
                    Y = y
                });
            }
            else if (child is Line line && line.Tag is Button[] connectedAtoms && connectedAtoms.Length == 2)
            {
                var a1 = connectedAtoms[0];
                var a2 = connectedAtoms[1];

                var pair = a1.GetHashCode() < a2.GetHashCode() ? (a1, a2) : (a2, a1);

                if (!rawBondCounts.ContainsKey(pair))
                {
                    rawBondCounts[pair] = 0;
                }
                rawBondCounts[pair]++;
            }
        }

        var bonds = new List<CanvasBondInfo>();
        foreach (var kvp in rawBondCounts)
        {
            string bondType = kvp.Value switch
            {
                3 => "TripleBond",
                2 => "DoubleBond",
                _ => "SingleBond"
            };

            bonds.Add(new CanvasBondInfo
            {
                Atom1Button = kvp.Key.Item1,
                Atom2Button = kvp.Key.Item2,
                BondType = bondType
            });
        }

        return (atoms, bonds);
    }
}