﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

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
        public const string HIGHSCORE_FILENAME = "highscores.txt";

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

        public void DisplayHighscores(double windowLeft, double windowTop)
        {
            //Dialog-Show Version:

            Highscores highscoresScreen = new Highscores();

            for (int i = 0; i < 5 && i < highscores.Count; i++)
            {
                string dtName = $"dt{i + 1}";
                string lengthName = $"length{i + 1}";
                string scoreName = $"score{i + 1}";

                // Finde die Labels mit den dynamischen Namen
                var dtLabel = highscoresScreen.FindName(dtName) as Label;
                var lengthLabel = highscoresScreen.FindName(lengthName) as Label;
                var scoreLabel = highscoresScreen.FindName(scoreName) as Label;

                // Setze die Inhalte der gefundenen Labels
                if (dtLabel != null) dtLabel.Content = highscores[i].dt;
                if (lengthLabel != null) lengthLabel.Content = highscores[i].länge;
                if (scoreLabel != null) scoreLabel.Content = highscores[i].score;
            }

            highscoresScreen.Left = windowLeft + 25;
            highscoresScreen.Top = windowTop + 25;
            highscoresScreen.Show();
        }

        private void LoadHighscores()
        {
            // Lese die Highscores aus der Datei, falls vorhanden
            if (File.Exists(HIGHSCORE_FILENAME))
            {
                string[] lines = File.ReadAllLines(HIGHSCORE_FILENAME);
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
            using (StreamWriter writer = new StreamWriter(HIGHSCORE_FILENAME))
            {
                foreach (var entry in highscores)
                {
                    writer.WriteLine($"{entry.dt},{entry.länge},{entry.score}");
                }
            }
        }
    }
}
