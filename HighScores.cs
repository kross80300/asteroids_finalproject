using System;
using System.IO;

namespace asteroids_finalproject
{
    public class HighScores
    {
        private const string HIGH_SCORE_FILE = "highscore.txt";
        private int _highScore;

        public HighScores()
        {
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
            try
            {
                File.WriteAllText(HIGH_SCORE_FILE, _highScore.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving high score: {ex.Message}");
            }
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