using System;
using System.IO;

namespace asteroids_finalproject
{
    public class HighScores
    {
        private readonly string HIGH_SCORE_FILE;
        private int _highScore;

        public HighScores()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            HIGH_SCORE_FILE = Path.Combine(projectDirectory, "Content", "highscore.txt");
            
            LoadHighScore();
        }

        public void LoadHighScore()
        {
            try
            {
                if (File.Exists(HIGH_SCORE_FILE))
                {
                    string content = File.ReadAllText(HIGH_SCORE_FILE);
                    if (int.TryParse(content, out int score))
                    {
                        _highScore = score;
                    }
                    else
                    {
                        _highScore = 0;
                    }
                }
                else
                {
                    _highScore = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading high score: {ex.Message}");
                _highScore = 0;
            }
        }

        public void SaveHighScore()
        {
            File.WriteAllText(HIGH_SCORE_FILE, _highScore.ToString());

        }

        public void CheckAndUpdateHighScore(int currentScore)
        {
            if (currentScore > _highScore)
            {
                _highScore = currentScore;
                SaveHighScore();
            }
        }

        public int GetHighScore()
        {
            return _highScore;
        }
    }
}