using System.Collections.Generic;
using System.Linq;

namespace MoleculeLab.Views;

public class MoleculeAnalyzer
{
    private static readonly Dictionary<string, double> AtomicMasses = new()
    {
        { "H", 1.008 }, { "He", 4.003 },

        { "Li", 6.94 }, { "Be", 9.012 }, { "B", 10.81 }, { "C", 12.011 }, { "N", 14.007 }, { "O", 15.999 }, { "F", 18.998 }, { "Ne", 20.180 },

        { "Na", 22.990 }, { "Mg", 24.305 }, { "Al", 26.982 }, { "Si", 28.085 }, { "P", 30.974 }, { "S", 32.06 }, { "Cl", 35.45 }, { "Ar", 39.95 },

        { "K", 39.098 }, { "Ca", 40.078 }, { "Sc", 44.956 }, { "Ti", 47.867 }, { "V", 50.942 }, { "Cr", 51.996 }, { "Mn", 54.938 },
        { "Fe", 55.845 }, { "Co", 58.933 }, { "Ni", 58.693 }, { "Cu", 63.546 }, { "Zn", 65.38 }, { "Ga", 69.723 }, { "Ge", 72.630 },
        { "As", 74.922 }, { "Se", 78.971 }, { "Br", 79.904 }, { "Kr", 83.798 },

        { "Rb", 85.468 }, { "Sr", 87.62 }, { "Y", 88.906 }, { "Zr", 91.224 }, { "Nb", 92.906 }, { "Mo", 95.95 }, { "Tc", 98.0 },
        { "Ru", 101.07 }, { "Rh", 102.91 }, { "Pd", 106.42 }, { "Ag", 107.87 }, { "Cd", 112.41 }, { "In", 114.82 }, { "Sn", 118.71 },
        { "Sb", 121.76 }, { "Te", 127.60 }, { "I", 126.90 }, { "Xe", 131.29 }
    };

    private static readonly Dictionary<string, int> ExpectedValencies = new()
    {
        { "H", 1 }, { "Li", 1 }, { "Na", 1 }, { "K", 1 }, { "Rb", 1 },
        { "Be", 2 }, { "Mg", 2 }, { "Ca", 2 }, { "Sr", 2 }, { "Zn", 2 }, { "Cd", 2 },
        { "B", 3 }, { "Al", 3 }, { "Ga", 3 }, { "In", 3 }, { "Sc", 3 }, { "Y", 3 },
        { "C", 4 }, { "Si", 4 }, { "Ge", 4 }, { "Sn", 4 }, { "Ti", 4 }, { "Zr", 4 }, { "Nb", 5 }, { "V", 5 },
        { "N", 3 }, { "P", 3 }, { "As", 3 }, { "Sb", 3 },
        { "O", 2 }, { "S", 2 }, { "Se", 2 }, { "Te", 2 },
        { "F", 1 }, { "Cl", 1 }, { "Br", 1 }, { "I", 1 },
        { "He", 0 }, { "Ne", 0 }, { "Ar", 0 }, { "Kr", 0 }, { "Xe", 0 },
        { "Fe", 2 }, { "Co", 2 }, { "Ni", 2 }, { "Cu", 2 }, { "Mn", 2 }, { "Cr", 2 }, { "Mo", 6 }, { "Ru", 3 }, { "Rh", 3 }, { "Pd", 2 }, { "Ag", 1 }
    };

    public static (string Formula, double TotalMass, bool IsStable) Analyze(List<GraphNode> moleculeNodes)
    {
        var elementCounts = new Dictionary<string, int>();
        double totalMass = 0;
        bool isStable = true;

        foreach (var node in moleculeNodes)
        {
            if (elementCounts.ContainsKey(node.Symbol))
                elementCounts[node.Symbol]++;
            else
                elementCounts[node.Symbol] = 1;

            if (AtomicMasses.TryGetValue(node.Symbol, out double mass))
            {
                totalMass += mass;
            }

            int actualBonds = node.Neighbors.Sum(n => n.BondType);
            if (ExpectedValencies.TryGetValue(node.Symbol, out int expected))
            {
                if (expected > 0 && actualBonds != expected && !IsTransitionMetal(node.Symbol))
                {
                    isStable = false;
                }
            }
        }

        string formula = "";
        foreach (var pair in elementCounts.OrderBy(p => p.Key))
        {
            formula += pair.Key;
            if (pair.Value > 1) formula += pair.Value;
        }

        return (formula, totalMass, isStable);
    }

    private static bool IsTransitionMetal(string symbol)
    {
        string[] metals = { "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Y", "Zr", "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd" };
        return metals.Contains(symbol);
    }
}