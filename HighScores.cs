using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace asteroids_finalproject
{
    public class HighScoreEntry
    {
        public string Initials { get; set; }
        public int Score { get; set; }

        public HighScoreEntry(string initials, int score)
        {
            Initials = initials;
            Score = score;
        }
    }

    public class HighScores
    {
        private readonly string HIGH_SCORE_FILE;
        private List<HighScoreEntry> _highScores;
        private const int MAX_HIGH_SCORES = 10;

        public HighScores()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            HIGH_SCORE_FILE = Path.Combine(projectDirectory, "Content", "highscore.txt");
            _highScores = new List<HighScoreEntry>();
            
            LoadHighScores();
        }

        public void LoadHighScores()
        {
            _highScores.Clear();
            
            if (File.Exists(HIGH_SCORE_FILE))
            {
                string[] lines = File.ReadAllLines(HIGH_SCORE_FILE);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        string initials = parts[0].Trim();
                        if (int.TryParse(parts[1].Trim(), out int score))
                        {
                            _highScores.Add(new HighScoreEntry(initials, score));
                        }
                    }
                }
            }
            
            _highScores = _highScores.OrderByDescending(e => e.Score).Take(MAX_HIGH_SCORES).ToList();
        }

        public void SaveHighScores()
        {
            List<string> lines = new List<string>();
            foreach (var entry in _highScores)
            {
                lines.Add($"{entry.Initials},{entry.Score}");
            }
            File.WriteAllLines(HIGH_SCORE_FILE, lines);
        }

        public bool IsHighScore(int score)
        {
            if (_highScores.Count < MAX_HIGH_SCORES)
                return true;
            
            return score > _highScores.Last().Score;
        }

        public void AddHighScore(string initials, int score)
        {
            _highScores.Add(new HighScoreEntry(initials, score));
            _highScores = _highScores.OrderByDescending(e => e.Score).Take(MAX_HIGH_SCORES).ToList();
            SaveHighScores();
        }

        public int GetHighScore()
        {
            if (_highScores.Count > 0)
                return _highScores[0].Score;
            return 0;
        }

        public List<HighScoreEntry> GetHighScores()
        {
            return _highScores;
        }
    }
}