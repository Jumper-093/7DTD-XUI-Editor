using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using XuiEditor.Models;
using XuiEditor.Export;
using Microsoft.Win32;
using System.Linq;

namespace XuiEditor
{
    public partial class MainWindow : Window
    {
        // ================= STATE =================

        bool isDragging = false;
        Point mouseStart;
        double elementStartX;
        double elementStartY;

        Rectangle selectedElement = null;

        List<Rectangle> allElements = new List<Rectangle>();
        Dictionary<Rectangle, XuiElement> elementMap = new Dictionary<Rectangle, XuiElement>();

        bool snapToGrid = true;
        double gridSize = 10;

        // ================= INIT =================

        public MainWindow()
        {
            InitializeComponent();

            // vorhandene Rechtecke aus XAML einsammeln
            int counter = 1;
            foreach (var child in EditorCanvas.Children)
            {
                if (child is Rectangle rect)
                {
                    allElements.Add(rect);

                    elementMap[rect] = new XuiElement
                    {
                        Name = $"Rectangle {counter}",
                        X = Canvas.GetLeft(rect),
                        Y = Canvas.GetTop(rect),
                        Width = rect.Width,
                        Height = rect.Height
                    };

                    rect.MouseLeftButtonDown += TestElement_MouseLeftButtonDown;
                    rect.MouseLeftButtonUp += TestElement_MouseLeftButtonUp;
                    rect.MouseMove += TestElement_MouseMove;

                    counter++;
                }
            }

            RefreshElementList();

            // UI Events
            PosXBox.TextChanged += PosXBox_TextChanged;
            PosYBox.TextChanged += PosYBox_TextChanged;
            WidthBox.TextChanged += WidthBox_TextChanged;
            HeightBox.TextChanged += HeightBox_TextChanged;
            NameBox.TextChanged += NameBox_TextChanged;

            ElementsList.SelectionChanged += ElementsList_SelectionChanged;

            SnapCheckBox.Checked += SnapCheckBox_Changed;
            SnapCheckBox.Unchecked += SnapCheckBox_Changed;
            GridSizeBox.TextChanged += GridSizeBox_TextChanged;
        }

        // ================= CANVAS =================

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditorCanvas.Focus();

            if (selectedElement != null)
                selectedElement.StrokeThickness = 0;

            selectedElement = null;
        }

        private void Canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedElement == null)
                return;

            double step =
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                ? gridSize
                : 1;

            double x = Canvas.GetLeft(selectedElement);
            double y = Canvas.GetTop(selectedElement);

            switch (e.Key)
            {
                case Key.Left: x -= step; break;
                case Key.Right: x += step; break;
                case Key.Up: y -= step; break;
                case Key.Down: y += step; break;
                default:
                    return;
            }

            if (snapToGrid)
            {
                x = Snap(x);
                y = Snap(y);
            }

            Canvas.SetLeft(selectedElement, x);
            Canvas.SetTop(selectedElement, y);

            elementMap[selectedElement].X = x;
            elementMap[selectedElement].Y = y;

            PosXBox.Text = x.ToString();
            PosYBox.Text = y.ToString();

            e.Handled = true;
        }

        // ================= SELECTION & DRAG =================

        private void TestElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditorCanvas.Focus();

            if (selectedElement != null)
                selectedElement.StrokeThickness = 0;

            selectedElement = sender as Rectangle;
            if (selectedElement == null) return;

            selectedElement.Stroke = Brushes.Yellow;
            selectedElement.StrokeThickness = 2;

            isDragging = true;
            mouseStart = e.GetPosition(EditorCanvas);

            elementStartX = Canvas.GetLeft(selectedElement);
            elementStartY = Canvas.GetTop(selectedElement);

            selectedElement.CaptureMouse();

            var model = elementMap[selectedElement];

            NameBox.Text = model.Name;
            PosXBox.Text = model.X.ToString();
            PosYBox.Text = model.Y.ToString();
            WidthBox.Text = model.Width.ToString();
            HeightBox.Text = model.Height.ToString();

            e.Handled = true;
        }

        private void TestElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            selectedElement?.ReleaseMouseCapture();
        }

        private void TestElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || selectedElement == null) return;

            Point current = e.GetPosition(EditorCanvas);

            double newX = elementStartX + (current.X - mouseStart.X);
            double newY = elementStartY + (current.Y - mouseStart.Y);

            if (snapToGrid)
            {
                newX = Snap(newX);
                newY = Snap(newY);
            }

            Canvas.SetLeft(selectedElement, newX);
            Canvas.SetTop(selectedElement, newY);

            elementMap[selectedElement].X = newX;
            elementMap[selectedElement].Y = newY;
        }

        // ================= PROPERTIES =================

        private void PosXBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement == null) return;

            if (double.TryParse(PosXBox.Text, out double x))
            {
                Canvas.SetLeft(selectedElement, x);
                elementMap[selectedElement].X = x;
            }
        }

        private void PosYBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement == null) return;

            if (double.TryParse(PosYBox.Text, out double y))
            {
                Canvas.SetTop(selectedElement, y);
                elementMap[selectedElement].Y = y;
            }
        }

        private void WidthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement == null) return;

            if (double.TryParse(WidthBox.Text, out double w))
            {
                selectedElement.Width = w;
                elementMap[selectedElement].Width = w;
            }
        }

        private void HeightBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement == null) return;

            if (double.TryParse(HeightBox.Text, out double h))
            {
                selectedElement.Height = h;
                elementMap[selectedElement].Height = h;
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement == null) return;

            elementMap[selectedElement].Name = NameBox.Text;
            RefreshElementList();
        }

        // ================= ELEMENT LIST =================

        private void RefreshElementList()
        {
            ElementsList.Items.Clear();

            foreach (var rect in allElements)
                ElementsList.Items.Add(elementMap[rect].Name);
        }

        private void ElementsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElementsList.SelectedIndex < 0) return;

            if (selectedElement != null)
                selectedElement.StrokeThickness = 0;

            selectedElement = allElements[ElementsList.SelectedIndex];

            selectedElement.Stroke = Brushes.Yellow;
            selectedElement.StrokeThickness = 2;

            var model = elementMap[selectedElement];

            NameBox.Text = model.Name;
            PosXBox.Text = model.X.ToString();
            PosYBox.Text = model.Y.ToString();
            WidthBox.Text = model.Width.ToString();
            HeightBox.Text = model.Height.ToString();
        }

        // ================= ADD / DELETE =================

        private void AddRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            Rectangle rect = new Rectangle
            {
                Width = 120,
                Height = 80,
                Fill = Brushes.SteelBlue
            };

            Canvas.SetLeft(rect, 50);
            Canvas.SetTop(rect, 50);

            rect.MouseLeftButtonDown += TestElement_MouseLeftButtonDown;
            rect.MouseLeftButtonUp += TestElement_MouseLeftButtonUp;
            rect.MouseMove += TestElement_MouseMove;

            EditorCanvas.Children.Add(rect);
            allElements.Add(rect);

            elementMap[rect] = new XuiElement
            {
                Name = $"Rectangle {allElements.Count}",
                X = 50,
                Y = 50,
                Width = 120,
                Height = 80
            };

            RefreshElementList();
            ElementsList.SelectedIndex = allElements.Count - 1;
        }

        private void DeleteElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null) return;

            var model = elementMap[selectedElement];

            if (MessageBox.Show(
                $"Möchtest du \"{model.Name}\" wirklich löschen?",
                "Element löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            EditorCanvas.Children.Remove(selectedElement);
            elementMap.Remove(selectedElement);
            allElements.Remove(selectedElement);

            selectedElement = null;

            NameBox.Text = "";
            PosXBox.Text = "";
            PosYBox.Text = "";
            WidthBox.Text = "";
            HeightBox.Text = "";

            RefreshElementList();
        }

        // ================= GRID =================

        private double Snap(double value)
        {
            return Math.Round(value / gridSize) * gridSize;
        }

        private void SnapCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            snapToGrid = SnapCheckBox.IsChecked == true;
        }

        private void GridSizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(GridSizeBox.Text, out double value) && value > 0)
                gridSize = value;
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var elements = elementMap.Values.ToList();

            var doc = XuiExporter.ExportWindow("myWindow", elements);

            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "XUI XML (*.xml)|*.xml",
                FileName = "window.xml"
            };

            if (dlg.ShowDialog() == true)
            {
                doc.Save(dlg.FileName);
                MessageBox.Show("XUI exportiert!", "Fertig");
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "XUI XML (*.xml)|*.xml"
            };

            if (dlg.ShowDialog() != true)
                return;

            // Alles leeren
            EditorCanvas.Children.Clear();
            allElements.Clear();
            elementMap.Clear();
            ElementsList.Items.Clear();
            selectedElement = null;

            var imported = XuiImporter.ImportWindow(dlg.FileName);

            foreach (var el in imported)
            {
                Rectangle rect = new Rectangle
                {
                    Width = el.Width,
                    Height = el.Height,
                    Fill = Brushes.SteelBlue
                };

                Canvas.SetLeft(rect, el.X);
                Canvas.SetTop(rect, el.Y);

                rect.MouseLeftButtonDown += TestElement_MouseLeftButtonDown;
                rect.MouseLeftButtonUp += TestElement_MouseLeftButtonUp;
                rect.MouseMove += TestElement_MouseMove;

                EditorCanvas.Children.Add(rect);
                allElements.Add(rect);
                elementMap[rect] = el;
            }

            RefreshElementList();
        }

    }
}
