using System;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace Snake
{
    /// <summary>
    /// Interaktionslogik für Highscores.xaml
    /// </summary>
    public partial class Highscores : Window
    {
        public Highscores()
        {
            InitializeComponent();
            Headline.MouseDoubleClick += Headline_MouseDoubleClick;
        }

        private void Headline_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            const string HIGHSCORE_FILENAME = HighscoreManager.HIGHSCORE_FILENAME;

            if (File.Exists(HIGHSCORE_FILENAME))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = HIGHSCORE_FILENAME,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
    }
}
