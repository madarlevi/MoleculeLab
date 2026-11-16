using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Numerics;

namespace MoleculeLab.Views;

public partial class MainWindow : Window
{
    bool TableOpen = true;
    int _atomCount = 0;
    string _activeTool = "Select";
    bool _isDragging = false;
    Button? _draggedAtom = null;
    Point _clickPosition;
    Button? _firstBondAtom = null;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenPeriodicTable_Click(object sender, RoutedEventArgs e)
    {
        TableOpen = !TableOpen;
        MainGrid.RowDefinitions = new Avalonia.Controls.RowDefinitions(TableOpen ? "300, *" : "0, *");
    }

    private void ToolButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string toolName)
        {
            _activeTool = toolName;
            _firstBondAtom = null;
            Title = "MoleculeLab - Aktív eszköz: " + _activeTool;
        }
    }

    private void AtomOption_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string symbol)
        {
            double startX = 60 + (_atomCount % 10) * 50;
            double startY = 60 + (_atomCount / 10) * 50;
            _atomCount++;

            CreateCanvasAtom(symbol, startX, startY);
        }
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

        atomButton.PointerPressed += AtomOnPointerPressed;
        atomButton.PointerMoved += AtomOnPointerMoved;
        atomButton.PointerReleased += AtomOnPointerReleased;

        MoleculeCanvas.Children.Add(atomButton);

        if (MoleculeCanvasText != null)
        {
            MoleculeCanvasText.IsVisible = false;
        }
    }

    private void AtomOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Button atom)
        {
            var point = e.GetCurrentPoint(MoleculeCanvas);

            if (_activeTool == "SingleBond" || _activeTool == "DoubleBond" || _activeTool == "TripleBond")
            {
                var modifiers = e.KeyModifiers;

                if (_firstBondAtom == null)
                {
                    _firstBondAtom = atom;
                    atom.Background = Brushes.LightBlue;
                }
                else if (modifiers.HasFlag(KeyModifiers.Shift) && _firstBondAtom != atom)
                {
                    CreateBond(_firstBondAtom, atom);
                    _firstBondAtom.Background = null;
                    _firstBondAtom = null;
                }
                return;
            }

            if (_activeTool == "Select" && point.Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _draggedAtom = atom;
                _clickPosition = point.Position;

                e.Pointer.Capture(atom);
                e.Handled = true;
            }
        }
    }

    private void AtomOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && _draggedAtom != null)
        {
            var currentPos = e.GetPosition(MoleculeCanvas);
            var offsetX = currentPos.X - _clickPosition.X;
            var offsetY = currentPos.Y - _clickPosition.Y;

            double newLeft = Canvas.GetLeft(_draggedAtom) + offsetX;
            double newTop = Canvas.GetTop(_draggedAtom) + offsetY;

            Canvas.SetLeft(_draggedAtom, newLeft);
            Canvas.SetTop(_draggedAtom, newTop);

            _clickPosition = currentPos;
            e.Handled = true;
        }
    }

    private void AtomOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _draggedAtom != null)
        {
            _isDragging = false;

            e.Pointer.Capture(null);
            _draggedAtom = null;
            e.Handled = true;
        }
    }

    private void CreateBond(Button atom1, Button atom2)
    {
        double x1 = Canvas.GetLeft(atom1) + 18;
        double y1 = Canvas.GetTop(atom1) + 18;
        double x2 = Canvas.GetLeft(atom2) + 18;
        double y2 = Canvas.GetTop(atom2) + 18;

        var line = new Line
        {
            StartPoint = new Point(x1, y1),
            EndPoint = new Point(x2, y2),
            Stroke = Brushes.Black,
            StrokeThickness = 3
        };

        MoleculeCanvas.Children.Insert(0, line);
    }
}