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
            ResetGame();

            windowLeft = this.Left;
            windowTop = this.Top;
            this.LocationChanged += MainWindow_LocationChanged;

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += AnimateSnake;
            timer.IsEnabled = true;
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