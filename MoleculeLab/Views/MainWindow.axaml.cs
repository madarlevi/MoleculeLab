using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MoleculeLab.Views;

public partial class MainWindow : Window
{
    bool TableOpen = true;

    bool animationInProgress = false;

    string _activeTool = "Select";

    Button? _selectedPeriodicButton = null;
    string? _selectedAtomSymbol = null;

    Button? _selectedCanvasAtom = null;

    Button? _firstBondAtom = null;

    bool _isShiftPressed = false;

    public MainWindow()
    {
        InitializeComponent();

        this.AddHandler(KeyDownEvent, (s, e) => {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) _isShiftPressed = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        this.AddHandler(KeyUpEvent, (s, e) => {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) _isShiftPressed = false;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        MoleculeCanvas.PointerPressed += MoleculeCanvas_PointerPressed;
    }

    private void OpenPeriodicTable_Click(object? sender, RoutedEventArgs e)
    {
        TableOpen = !TableOpen;
        MainGrid.RowDefinitions = new Avalonia.Controls.RowDefinitions(TableOpen ? "300, *" : "0, *");
    }

    private void ToolButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string toolName)
        {
            _activeTool = toolName;
            ResetSelection();
            Title = "MoleculeLab - Active tool: " + _activeTool;
        }
    }

    private void AtomOption_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string symbol)
        {
            if (_selectedPeriodicButton == btn)
            {
                ResetSelection();
                return;
            }

            if (_selectedPeriodicButton != null)
            {
                _selectedPeriodicButton.Background = Brushes.Transparent;
            }

            if (_selectedCanvasAtom != null)
            {
                _selectedCanvasAtom.Background = null;
                _selectedCanvasAtom = null;
            }

            _selectedPeriodicButton = btn;
            _selectedAtomSymbol = symbol;
            btn.Background = Brushes.LightBlue;

            if (_activeTool != "Select" && _activeTool != "Move")
            {
                _activeTool = "Select";
            }
        }
    }

    private void MoleculeCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(MoleculeCanvas);
        if (!point.Properties.IsLeftButtonPressed) return;

        if (e.Source is Canvas || e.Source == MoleculeCanvas)
        {
            var pos = point.Position;
            double atomSize = 36;
            double targetX = pos.X - (atomSize / 2);
            double targetY = pos.Y - (atomSize / 2);

            if (_activeTool == "Move" && _selectedCanvasAtom != null)
            {
                if (CheckCollision(targetX, targetY, atomSize, _selectedCanvasAtom))
                {
                    return;
                }

                string symbol = _selectedCanvasAtom.Content?.ToString() ?? "C";
                MoveAtomTo(_selectedCanvasAtom, targetX, targetY, symbol);

                _selectedCanvasAtom.Background = null;
                _selectedCanvasAtom = null;
                e.Handled = true;
                return;
            }

            if (_selectedAtomSymbol != null && (_activeTool == "Select" || _activeTool == "Move"))
            {
                if (CheckCollision(targetX, targetY, atomSize, null))
                {
                    return;
                }

                CreateCanvasAtom(_selectedAtomSymbol, targetX, targetY);

                e.Handled = true;
                return;
            }

            if (_activeTool == "Select")
            {
                ResetSelection();
            }
        }
    }

    private void MoveAtomTo(Button atomToMove, double newX, double newY, string symbol)
    {
        RemoveBondsForAtom(atomToMove);

        MoleculeCanvas.Children.Remove(atomToMove);

        CreateCanvasAtom(symbol, newX, newY);
    }

    private bool CheckCollision(double newX, double newY, double size, Button? ignoredAtom)
    {
        foreach (var child in MoleculeCanvas.Children)
        {
            if (child is Button existingAtom)
            {
                if (existingAtom == ignoredAtom) continue;

                double exX = Canvas.GetLeft(existingAtom);
                double exY = Canvas.GetTop(existingAtom);

                double dx = (newX + size / 2) - (exX + size / 2);
                double dy = (newY + size / 2) - (exY + size / 2);
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < size)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CreateCanvasAtom(string symbol, double x, double y)
    {
        var atomButton = new Button
        {
            Content = symbol,
            Classes = { "canvas-atom" }
        };

        Canvas.SetLeft(atomButton, x);
        Canvas.SetTop(atomButton, y);

        atomButton.Click += AtomOnClicked;

        MoleculeCanvas.Children.Add(atomButton);
    }

    private void AtomOnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button atom)
        {
            if (_activeTool == "Delete")
            {
                RemoveBondsForAtom(atom);
                if (_firstBondAtom == atom) _firstBondAtom = null;
                if (_selectedCanvasAtom == atom) _selectedCanvasAtom = null;
                MoleculeCanvas.Children.Remove(atom);
                return;
            }

            if (_activeTool == "SingleBond" || _activeTool == "DoubleBond" || _activeTool == "TripleBond")
            {
                if (_firstBondAtom == null)
                {
                    _firstBondAtom = atom;
                    atom.Background = Brushes.LightBlue;
                }
                else if (_firstBondAtom == atom)
                {
                    atom.Background = null;
                    _firstBondAtom = null;
                }
                else if (_isShiftPressed && _firstBondAtom != atom)
                {
                    CreateBonds(_firstBondAtom, atom, _activeTool);
                    _firstBondAtom.Background = null;
                    _firstBondAtom = null;
                }
                return;
            }

            if (_activeTool == "Select")
            {
                OnAtomSelected(atom);

                if (_selectedCanvasAtom != null) _selectedCanvasAtom.Background = null;
                _selectedCanvasAtom = atom;
                atom.Background = Brushes.LightBlue;
                return;
            }

            if (_activeTool == "Move")
            {
                if (_selectedCanvasAtom == atom)
                {
                    atom.Background = null;
                    _selectedCanvasAtom = null;
                }
                else
                {
                    if (_selectedCanvasAtom != null) _selectedCanvasAtom.Background = null;
                    _selectedCanvasAtom = atom;
                    atom.Background = Brushes.Orange;
                }
                return;
            }
        }
    }

    private void RemoveBondsForAtom(Button atomToRemove)
    {
        var linesToRemove = new List<Line>();

        foreach (var child in MoleculeCanvas.Children)
        {
            if (child is Line line && line.Tag is Button[] atoms)
            {
                if (atoms[0] == atomToRemove || atoms[1] == atomToRemove)
                {
                    linesToRemove.Add(line);
                }
            }
        }

        foreach (var line in linesToRemove)
        {
            MoleculeCanvas.Children.Remove(line);
        }
    }

    private void ResetSelection()
    {
        if (_firstBondAtom != null)
        {
            _firstBondAtom.Background = null;
            _firstBondAtom = null;
        }

        if (_selectedCanvasAtom != null)
        {
            _selectedCanvasAtom.Background = null;
            _selectedCanvasAtom = null;
        }

        if (_selectedPeriodicButton != null)
        {
            _selectedPeriodicButton.Background = Brushes.Transparent;
            _selectedPeriodicButton = null;
        }

        _selectedAtomSymbol = null;
    }

    private void CreateBonds(Button atom1, Button atom2, string bondType)
    {
        double radius = 18;

        double cx1 = Canvas.GetLeft(atom1) + radius;
        double cy1 = Canvas.GetTop(atom1) + radius;
        double cx2 = Canvas.GetLeft(atom2) + radius;
        double cy2 = Canvas.GetTop(atom2) + radius;

        double dx = cx2 - cx1;
        double dy = cy2 - cy1;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance == 0) return;

        double x1 = cx1 + (dx / distance) * radius;
        double y1 = cy1 + (dy / distance) * radius;
        double x2 = cx2 - (dx / distance) * radius;
        double y2 = cy2 - (dy / distance) * radius;

        double nx = -dy / distance;
        double ny = dx / distance;
        double spacing = 5;

        if (bondType == "SingleBond")
        {
            DrawBondLine(x1, y1, x2, y2, atom1, atom2);
        }
        else if (bondType == "DoubleBond")
        {
            DrawBondLine(x1 + nx * (spacing / 2), y1 + ny * (spacing / 2), x2 + nx * (spacing / 2), y2 + ny * (spacing / 2), atom1, atom2);
            DrawBondLine(x1 - nx * (spacing / 2), y1 - ny * (spacing / 2), x2 - nx * (spacing / 2), y2 - ny * (spacing / 2), atom1, atom2);
        }
        else if (bondType == "TripleBond")
        {
            DrawBondLine(x1, y1, x2, y2, atom1, atom2);
            DrawBondLine(x1 + nx * spacing, y1 + ny * spacing, x2 + nx * spacing, y2 + ny * spacing, atom1, atom2);
            DrawBondLine(x1 - nx * spacing, y1 - ny * spacing, x2 - nx * spacing, y2 - ny * spacing, atom1, atom2);
        }
    }

    private void DrawBondLine(double x1, double y1, double x2, double y2, Button atom1, Button atom2)
    {
        var line = new Line
        {
            StartPoint = new Point(x1, y1),
            EndPoint = new Point(x2, y2),
            Stroke = Brushes.Black,
            StrokeThickness = 2.5,
            Tag = new Button[] { atom1, atom2 }
        };

        MoleculeCanvas.Children.Insert(0, line);
    }

    private void StartStopButton_Click(object sender, RoutedEventArgs e)
    {
        if (animationInProgress)
        {
            animationInProgress = false;
            StartStopButton.Content = "Start";
            Title = "MoleculeLab - Szimuláció leállítva";
        }
        else
        {
            animationInProgress = true;
            StartStopButton.Content = "Stop";
            Title = "MoleculeLab - Szimuláció fut...";

            RunSimulationStep();
        }
    }
    private void RunSimulationStep()
    {
        var (atoms, bonds) = CanvasMoleculeExtractor.ExtractFromCanvas(MoleculeCanvas);

        if (atoms.Count == 0)
        {
            Title = "MoleculeLab - Nincsenek atomok a vásznon!";
            animationInProgress = false;
            StartStopButton.Content = "Start";
            return;
        }

        var independentMolecules = MolecularGraphBuilder.BuildIndependentMolecules(atoms, bonds);

        double startX = 200;
        double startY = 200;

        foreach (var mol in independentMolecules)
        {
            MoleculeLayoutEngine.ArrangeMolecule(mol, startX, startY);
            startX += 250;
        }

        var linesToRemove = MoleculeCanvas.Children.OfType<Line>().ToList();
        foreach (var line in linesToRemove)
        {
            MoleculeCanvas.Children.Remove(line);
        }

        foreach (var bond in bonds)
        {
            CreateBonds(bond.Atom1Button, bond.Atom2Button, bond.BondType);
            Debug.WriteLine($"Bond: {bond.Atom1Button.Content}-{bond.Atom2Button.Content}, Type: {bond.BondType}");
        }

        Title = $"MoleculeLab - Elrendezve! Molekulák száma: {independentMolecules.Count}";
    }

    public void OnAtomSelected(Button clickedAtomButton)
    {
        var (canvasAtoms, canvasBonds) = CanvasMoleculeExtractor.ExtractFromCanvas(MoleculeCanvas);

        var independentMolecules = MolecularGraphBuilder.BuildIndependentMolecules(canvasAtoms, canvasBonds);

        List<GraphNode>? targetMolecule = null;
        foreach (var mol in independentMolecules)
        {
            if (mol.Any(node => node.ButtonRef == clickedAtomButton))
            {
                targetMolecule = mol;
                break;
            }
        }

        if (targetMolecule != null)
        {
            var (formula, name, mass, isStable) = MoleculeAnalyzer.Analyze(targetMolecule);

            TxtMoleculeFormula.Text = $"Formula: {formula}";
            TxtMoleculeName.Text = $"Name: {name}";
            TxtMoleculeMass.Text = $"Molecular mass: {mass:F3} g/mol";
            TxtMoleculeStatus.Text = isStable ? "Status: Stable ✓" : "Status: Unstable / Invalid ⚠";
        }
    }
}