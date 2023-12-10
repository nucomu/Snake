using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace Snake
{
    public partial class MainWindow : Window
    {
        const int width = 30; //30
        const int height = 20; //20
        const int margin = 5;
        const int gridSize = 10; // Gitterabstand

        Random random = new ();
        System.Windows.Threading.DispatcherTimer timer = new ();

        int[] xSnake; // eleganter: List<Tuple<int, int>>
        int[] ySnake;
        int xFood;
        int yFood;
        int direction = -1; // 0 1 2 3 = rechts rauf links runter; -1 = noch nicht gestartet

        double occupied; // Prozent belegte Feldfläche 
        int allpoints = 0;
        public int newpoints = -1;
        public bool bonusfive = false;

        public MainWindow()
        {
            InitializeComponent();
            reset();

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += animate;
            timer.IsEnabled = true;
        }

        void reset()
        {
            direction = -1; 
            xSnake = new int[] { width / 2 };
            ySnake = new int[] { height / 2 };

            allpoints = 0;
            Allpoints.Content = "score: " + allpoints;

            placeFood();
            draw();
            timer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int oldDirection = direction;

            switch (e.Key)
            {
                case Key.Right:
                    direction = 0;
                    break;
                case Key.Up:
                    direction = 1;
                    break;
                case Key.Left:
                    direction = 2;
                    break;
                case Key.Down:
                    direction = 3;
                    break;
                case Key.P: //Pause 
                    if (timer.IsEnabled)
                    {
                        timer.Stop();
                        End.Content = "press 'p' for resume";
                    }
                    else
                    {
                        timer.Start();
                        End.Content = "press 'p' for pause";
                    }
                    break;
                case Key.R: // nur zum Test der reset-Methode
                    reset();
                    return;
                default:
                    break;
            }

            // die Schlange nicht entlang sich selbst zurückbewegen
            bool accept = true;
            if (xSnake.Length > 1)
            {
                int xNew = xSnake[0];
                int yNew = ySnake[0];
                switch (direction)
                {
                    case 0:
                        xNew++;
                        break;
                    case 1:
                        yNew--;
                        break;
                    case 2:
                        xNew--;
                        break;
                    case 3:
                        yNew++;
                        break;
                }
                if (xNew == xSnake[1] && yNew == ySnake[1])
                {
                    accept = false;
                }
            }
            if (!accept)
            {
                direction = oldDirection;
            }
        }

        void animate(object sender, EventArgs e)
        {
            if (direction == -1)
            {
                return;
            }

            Newpoints.Content = "+" + newpoints;

            // Wo soll der Kopf hin?
            int xNew = xSnake[0];
            int yNew = ySnake[0];
            switch (direction)
            {
                case 0:
                    xNew++;
                    break;
                case 1:
                    yNew--;
                    break;
                case 2:
                    xNew--;
                    break;
                case 3:
                    yNew++;
                    break;
            }

            if (xNew == xFood && yNew == yFood) // Treffer
            {
                // Schlange vorne um Position des Essens verlängern

                int[] xLongerSnake = new int[xSnake.Length + 1];
                int[] yLongerSnake = new int[xSnake.Length + 1];
                xLongerSnake[0] = xNew;
                yLongerSnake[0] = yNew;
                for (int i = 0; i < xSnake.Length; i++)
                {
                    xLongerSnake[i + 1] = xSnake[i];
                    yLongerSnake[i + 1] = ySnake[i];
                }
                xSnake = xLongerSnake;
                ySnake = yLongerSnake;
                allpoints += newpoints;
                Allpoints.Content = "score: " + allpoints;
                placeFood();
            }
            else
            {
                // alle eins weiterrutschen

                for (int i = xSnake.Length - 1; i > 0; i--)
                {
                    xSnake[i] = xSnake[i - 1];
                    ySnake[i] = ySnake[i - 1];
                }
                xSnake[0] = xNew;
                ySnake[0] = yNew;
            }

            //Selbstkollission 
            for (int i = xSnake.Length - 1; i > 0; i--)                     
            {
                if (xSnake[0] == xSnake[i] && ySnake[0] == ySnake[i])
                {
                    timer.Stop();
                    End.Content = "self collission. Press 'r' for restart.";
                    return;
                }
            }
            
            //Wandkollission 
            if (xSnake[0] < 0 || xSnake[0] == width || ySnake[0] < 0 || ySnake[0] == height)
            {
                timer.Stop();
                End.Content = "border collission. Press 'r' for restart.";
                return;
            }

            if (bonusfive)
            {
                if (newpoints > 1) { newpoints -= 5; }
            }
            else
            {
                if (newpoints > 1) { newpoints -= 2; }
            }

            if (newpoints == 0) { newpoints = 1; }

            draw();
        }

        void draw()
        {
            myCanvas.Children.Clear();

            // Spielfeldrand 
            Polyline myPolyline = new ()
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Fill = Brushes.DarkGray
            };
            Point Point1 = new (margin * gridSize, margin * gridSize);
            Point Point2 = new (margin * gridSize, (margin + height) * gridSize);
            Point Point3 = new ((margin + width) * gridSize, (margin + height) * gridSize);
            Point Point4 = new ((margin + width) * gridSize, margin * gridSize);
            PointCollection myPointCollection = new () { Point1, Point2, Point3, Point4, Point1 };
            myPolyline.Points = myPointCollection;
            myCanvas.Children.Add(myPolyline);

            for (int i = 0; i < xSnake.Length; i++)
            {
                Ellipse ellipse = new ();
                ellipse.Width = gridSize;
                ellipse.Height = gridSize;
                // Kopf dunkelgrün, Körper hellgrün 
                if (i == 0) { ellipse.Fill = Brushes.DarkGreen; } else { ellipse.Fill = Brushes.LightGreen; } 
                myCanvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, (xSnake[i] + margin) * gridSize);
                Canvas.SetTop(ellipse, (ySnake[i] + margin) * gridSize);
            }

            Ellipse ellipse2 = new ();
            ellipse2.Width = gridSize;
            ellipse2.Height = gridSize;

            if (bonusfive)
            {
                if (newpoints == 1) { ellipse2.Fill = Brushes.DarkOrange; } else { ellipse2.Fill = Brushes.Orange; }
            }
            else
            {
                if (newpoints == 1) { ellipse2.Fill = Brushes.DarkRed; } else { ellipse2.Fill = Brushes.Red; }
            };

            myCanvas.Children.Add(ellipse2);
            Canvas.SetLeft(ellipse2, (xFood + margin) * gridSize);
            Canvas.SetTop(ellipse2, (yFood + margin) * gridSize);

            //Spielestatistik 
            occupied = Math.Round(Convert.ToDouble(xSnake.Length) / (width * height) * 100, 2);
            Statistics.Content = "Snake length: " + xSnake.Length + " / occupied fields: " + occupied + " %";
            myCanvas.Children.Add(Statistics);

            //Tipps 
            myCanvas.Children.Add(End);
            End.Content = "press 'p' for pause";

            myCanvas.Children.Add(Newpoints);
            myCanvas.Children.Add(Allpoints);
            myCanvas.Children.Add(PointsPerFood);
        }

        void placeFood()
        {
            if ((xSnake.Length % 5) == 0) 
            { 
                bonusfive = true;
                newpoints = 500;
            } 
            else 
            { 
                bonusfive = false;
                newpoints = 100;
            }

            if (xSnake.Length > 1)
            {
                PointsPerFood.Content = allpoints / (xSnake.Length - 1.0);
            }
            else
            {
                PointsPerFood.Content = 0;
            }

            int numFreeSquares = width * height - xSnake.Length;
            if (numFreeSquares == 0)
            {
                MessageBox.Show("Congratulations!");
                reset();
            }
            else
            {
                // Welche Felder sind schon von der Schlange belegt?
                bool[] squareOccupied = new bool[width * height];
                for (int i = 0; i < xSnake.Length; i++)
                {
                    if (xSnake[i] >= 0 && xSnake[i] < width && ySnake[i] >= 0 && ySnake[i] < height)
                    {
                        squareOccupied[width * ySnake[i] + xSnake[i]] = true;
                    }
                }

                int mouseSquare = random.Next(numFreeSquares);
                // Gehe zum ersten freien Platz.
                int k = 0;
                while (squareOccupied[k])
                {
                    k++;
                }
                // Zähle nun bis zum freien Platz mit der Nummer mouseSquare und nehme den.
                for (int j = 0; j < mouseSquare; j++)
                {
                    // Finde den nächsten freien Platz.
                    k++;
                    while (squareOccupied[k])
                    {
                        k++;
                    }
                }
                xFood = k % width;
                yFood = k / width;
            }
        }
    }
}
