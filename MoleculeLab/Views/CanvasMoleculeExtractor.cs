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
}

public static class CanvasMoleculeExtractor
{
    public static (List<CanvasAtomInfo> Atoms, List<CanvasBondInfo> Bonds) ExtractFromCanvas(Canvas canvas)
    {
        var atoms = new List<CanvasAtomInfo>();
        var bonds = new List<CanvasBondInfo>();

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
        }

        foreach (var child in canvas.Children)
        {
            if (child is Line line && line.Tag is Button[] connectedAtoms)
            {
                if (connectedAtoms.Length == 2)
                {
                    bonds.Add(new CanvasBondInfo
                    {
                        Atom1Button = connectedAtoms[0],
                        Atom2Button = connectedAtoms[1]
                    });
                }
            }
        }

        return (atoms, bonds);
    }
}