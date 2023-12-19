using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Snake
{
    class HighscoreEntry
    {
        public string? dt { get; set; }
        public int länge { get; set; }
        public int score { get; set; }
    }

    class HighscoreManager
    {
        const string HighscoreFileName = "highscores.txt";

        private List<HighscoreEntry> highscores = new List<HighscoreEntry>();

        public HighscoreManager()
        {
            // Lade vorhandene Highscores aus der Datei
            LoadHighscores();
        }

        public void AddHighscore(string dt, int länge, int score) 
        {
            // Füge neuen Highscore-Eintrag hinzu
            highscores.Add(new HighscoreEntry { dt = dt, länge = länge, score = score });

            // Sortiere die Highscores absteigend nach Punkten
            highscores = highscores.OrderByDescending(h => h.länge).ToList();

            // Begrenze die Anzahl der Highscores (z.B. auf die besten 100)
            highscores = highscores.Take(100).ToList();

            // Speichere die Highscores in der Datei
            SaveHighscores();
        }

        public void DisplayHighscores()
        {
            if (File.Exists(HighscoreFileName))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = HighscoreFileName,
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

            //Console.WriteLine("Highscores:");
            //foreach (var entry in highscores)
            //{
            //    Console.WriteLine($"{entry.dt}: {entry.länge} - {entry.score} - {entry.ppf}");
            //    Console.ReadLine();
            //}
        }

        private void LoadHighscores()
        {
            // Lese die Highscores aus der Datei, falls vorhanden
            if (File.Exists(HighscoreFileName))
            {
                string[] lines = File.ReadAllLines(HighscoreFileName);
                highscores = lines.Select(line =>
                {
                    var parts = line.Split(',');
                    return new HighscoreEntry { dt = parts[0], länge = int.Parse(parts[1]), score = int.Parse(parts[2]) };
                }).ToList();
            }
            else
            {
                // Wenn die Datei nicht existiert, erstelle eine leere Liste
                highscores = new List<HighscoreEntry>();
            }
        }

        private void SaveHighscores()
        {
            // Speichere die Highscores in der Datei
            using (StreamWriter writer = new StreamWriter(HighscoreFileName))
            {
                foreach (var entry in highscores)
                {
                    writer.WriteLine($"{entry.dt},{entry.länge},{entry.score}");
                }
            }
        }
    }

    //class Program
    //{
    //    static void Main()
    //    {
    //        HighscoreManager highscoreManager = new HighscoreManager();

    //        // Beispiel: Füge einen neuen Highscore hinzu
    //        highscoreManager.AddHighscore("Spieler1", 1000);

    //        // Zeige die Highscores an
    //        highscoreManager.DisplayHighscores();
    //    }
    //}
}
