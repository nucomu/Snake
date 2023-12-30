using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake
{
    public partial class MainWindow : Window
    {
        const int WIDTH = 30; //30
        const int HEIGHT = 20; //20
        const int MARGIN = 5;
        const int GRID_SIZE = 10; // Gitterabstand

        readonly Random random = new();
        System.Windows.Threading.DispatcherTimer timer = new();

        int[] xSnake; // eleganter: List<Tuple<int, int>>
        int[] ySnake;
        int xFood;
        int yFood;
        int direction = -1; // 0 1 2 3 = rechts rauf links runter; -1 = noch nicht gestartet

        double occupied; // Prozent belegte Feldfläche 
        int allpoints = 0;
        public int newpoints = -1;
        public bool bonusfive = false;
        bool restardAllowed = true;

        public double windowLeft;
        public double windowTop;
        
        HighscoreManager highscoreManager = new HighscoreManager();

        public MainWindow()
        {
            InitializeComponent();
            reset();

            windowLeft = this.Left;
            windowTop = this.Top;
            this.LocationChanged += MainWindow_LocationChanged;

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += animate;
            timer.IsEnabled = true;
        }

        void reset()
        {
            if (restardAllowed == false)
            {
                restardAllowed = true;
                return;
            }
            direction = -1;
            xSnake = new int[] { WIDTH / 2 };
            ySnake = new int[] { HEIGHT / 2 };

            allpoints = 0;
            Allpoints.Content = "score: " + allpoints;
            PointsPerFood.Content = "points per food: 0";

            placeFood();
            draw();
            timer.Start();
            restardAllowed = false;
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
                    restardAllowed = false;
                    return;
                case Key.Tab:
                    restardAllowed = true;
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
            if (direction == -1) { return; }

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
                    Save_Highscore();
                    restardAllowed = true;
                    return;
                }
            }

            //Wandkollission 
            if (xSnake[0] < 0 || xSnake[0] == WIDTH || ySnake[0] < 0 || ySnake[0] == HEIGHT)
            {
                timer.Stop();
                End.Content = "border collission. Press 'r' for restart.";
                Save_Highscore();
                restardAllowed = true;
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
            Polyline myPolyline = new()
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Fill = Brushes.DarkGray
            };
            Point Point1 = new(MARGIN * GRID_SIZE, MARGIN * GRID_SIZE);
            Point Point2 = new(MARGIN * GRID_SIZE, (MARGIN + HEIGHT) * GRID_SIZE);
            Point Point3 = new((MARGIN + WIDTH) * GRID_SIZE, (MARGIN + HEIGHT) * GRID_SIZE);
            Point Point4 = new((MARGIN + WIDTH) * GRID_SIZE, MARGIN * GRID_SIZE);
            PointCollection myPointCollection = new() { Point1, Point2, Point3, Point4, Point1 };
            myPolyline.Points = myPointCollection;
            myCanvas.Children.Add(myPolyline);

            for (int i = 0; i < xSnake.Length; i++)
            {
                Ellipse ellipse = new();
                ellipse.Width = GRID_SIZE;
                ellipse.Height = GRID_SIZE;
                // Kopf dunkelgrün, Körper hellgrün 
                if (i == 0) { ellipse.Fill = Brushes.DarkGreen; } else { ellipse.Fill = Brushes.LightGreen; }
                myCanvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, (xSnake[i] + MARGIN) * GRID_SIZE);
                Canvas.SetTop(ellipse, (ySnake[i] + MARGIN) * GRID_SIZE);
            }

            Ellipse ellipse2 = new();
            ellipse2.Width = GRID_SIZE;
            ellipse2.Height = GRID_SIZE;

            if (bonusfive)
            {
                double factor = (double)newpoints / 500.0;
                double sqrtFactor = Math.Sqrt(factor);

                byte red = (byte)(255 * sqrtFactor);
                byte green = (byte)(165 * sqrtFactor);
                byte blue = 0;

                ellipse2.Fill = new SolidColorBrush(Color.FromRgb(red, green, blue));
            }
            else
            {
                double factor = (double)newpoints / 100.0;
                double sqrtFactor = Math.Sqrt(factor);

                byte red = (byte)(255 * sqrtFactor);
                byte green = 0;
                byte blue = 0;

                ellipse2.Fill = new SolidColorBrush(Color.FromRgb(red, green, blue));
            };

            myCanvas.Children.Add(ellipse2);
            Canvas.SetLeft(ellipse2, (xFood + MARGIN) * GRID_SIZE);
            Canvas.SetTop(ellipse2, (yFood + MARGIN) * GRID_SIZE);

            //Spielestatistik 
            occupied = Math.Round(Convert.ToDouble(xSnake.Length) / (WIDTH * HEIGHT) * 100, 2);
            Statistics.Content = "Snake length: " + xSnake.Length + " / occupied fields: " + occupied + " %";
            myCanvas.Children.Add(Statistics);

            //Tipps 
            myCanvas.Children.Add(End);
            End.Content = "press 'p' for pause";

            myCanvas.Children.Add(Newpoints);
            myCanvas.Children.Add(Allpoints);
            myCanvas.Children.Add(PointsPerFood);
            myCanvas.Children.Add(Highscore);
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
                PointsPerFood.Content = "points per food: " + Math.Round(allpoints / (xSnake.Length - 1.0), 2);
            }
            else
            {
                PointsPerFood.Content = "points per food: 0";
            }

            int numFreeSquares = WIDTH * HEIGHT - xSnake.Length;
            if (numFreeSquares == 0)
            {
                MessageBox.Show("Congratulations!");
                Save_Highscore();
                reset();
            }
            else
            {
                // Welche Felder sind schon von der Schlange belegt?
                bool[] squareOccupied = new bool[WIDTH * HEIGHT];
                for (int i = 0; i < xSnake.Length; i++)
                {
                    if (xSnake[i] >= 0 && xSnake[i] < WIDTH && ySnake[i] >= 0 && ySnake[i] < HEIGHT)
                    {
                        squareOccupied[WIDTH * ySnake[i] + xSnake[i]] = true;
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
                xFood = k % WIDTH;
                yFood = k / WIDTH;
            }
        }

        private void Highscore_Click(object sender, RoutedEventArgs e)
        {
            highscoreManager.DisplayHighscores(windowLeft, windowTop);
        }

        private void Save_Highscore()
        {
            highscoreManager.AddHighscore(DateTime.Now.ToString(), xSnake.Length, allpoints);
        }
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            windowLeft = this.Left;
            windowTop = this.Top;
        }
    }
}