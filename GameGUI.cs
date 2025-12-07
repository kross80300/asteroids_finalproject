using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace asteroids_finalproject
{
    public class GameGUI
    {
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _heartTexture;

        public GameGUI(SpriteFont font, GraphicsDevice graphicsDevice, Texture2D heartTexture)
        {
            _font = font;
            _graphicsDevice = graphicsDevice;
            _heartTexture = heartTexture;
        }

        public void DrawHUD(SpriteBatch spriteBatch, int currentLevel, int score, int lives, int highScore, string highScoreInitials = "")
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            string highScoreText = string.IsNullOrEmpty(highScoreInitials) 
                ? $"High Score: {highScore}" 
                : $"High Score: {highScore} ({highScoreInitials})";
            string levelText = $"Wave: {currentLevel}";
            string scoreText = $"Score: {score}";
            
            Vector2 highScorePos = new Vector2(20, 20);
            Vector2 levelPos = new Vector2(20, 55);
            Vector2 scorePos = new Vector2(20, 90);

            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            Vector2 highScoreSize = _font.MeasureString(highScoreText);
            Vector2 levelSize = _font.MeasureString(levelText);
            Vector2 scoreSize = _font.MeasureString(scoreText);
            
            float maxWidth = Math.Max(Math.Max(highScoreSize.X, levelSize.X), scoreSize.X);
            
            spriteBatch.Draw(pixel, new Rectangle(15, 15, (int)maxWidth + 10, 105), Color.Black * 0.5f);
            
            spriteBatch.DrawString(_font, highScoreText, highScorePos, Color.Gold);
            spriteBatch.DrawString(_font, levelText, levelPos, Color.White);
            spriteBatch.DrawString(_font, scoreText, scorePos, Color.White);

            float heartSize = 40f;
            float heartSpacing = 50f;
            float totalHeartsWidth = (3 * heartSpacing) - (heartSpacing - heartSize);
            Vector2 heartsStartPos = new Vector2(screenWidth - totalHeartsWidth - 20, 20);

            spriteBatch.Draw(pixel, new Rectangle((int)(screenWidth - totalHeartsWidth - 25), 15, (int)totalHeartsWidth + 10, (int)heartSize + 20), Color.Black * 0.5f);
            
            for (int i = 0; i < 3; i++)
            {
                Color heartColor = i < lives ? Color.Red : Color.Gray;
                Vector2 heartPos = new Vector2(heartsStartPos.X + (i * heartSpacing), heartsStartPos.Y);
                
                spriteBatch.Draw(
                    _heartTexture,
                    new Rectangle((int)heartPos.X, (int)heartPos.Y, (int)heartSize, (int)heartSize),
                    heartColor
                );
            }
        }

        public void DrawGameOverScreen(SpriteBatch spriteBatch, int finalLevel, int finalScore, int highScore)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(
                pixel,
                new Rectangle(0, 0, screenWidth, screenHeight),
                Color.Black * 0.7f
            );
            
            string gameOverText = "GAME OVER";
            Vector2 gameOverSize = _font.MeasureString(gameOverText);
            Vector2 gameOverPos = new Vector2(
                (screenWidth - gameOverSize.X) / 2f,
                screenHeight / 2f - 100
            );
            spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.Red);

            string levelText = $"Wave Reached: {finalLevel}";
            Vector2 levelSize = _font.MeasureString(levelText);
            Vector2 levelPos = new Vector2(
                (screenWidth - levelSize.X) / 2f,
                screenHeight / 2f - 20
            );
            spriteBatch.DrawString(_font, levelText, levelPos, Color.White);
            
            string scoreText = $"Final Score: {finalScore}";
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 scorePos = new Vector2(
                (screenWidth - scoreSize.X) / 2f,
                screenHeight / 2f + 20
            );
            spriteBatch.DrawString(_font, scoreText, scorePos, Color.White);

            string highScoreText = $"High Score: {highScore}";
            Vector2 highScoreSize = _font.MeasureString(highScoreText);
            Vector2 highScorePos = new Vector2(
                (screenWidth - highScoreSize.X) / 2f,
                screenHeight / 2f + 60
            );
            Color highScoreColor = finalScore >= highScore ? Color.Yellow : Color.Gold;
            spriteBatch.DrawString(_font, highScoreText, highScorePos, highScoreColor);

            if (finalScore >= highScore)
            {
                string newHighScoreText = "NEW HIGH SCORE!";
                Vector2 newHighScoreSize = _font.MeasureString(newHighScoreText);
                Vector2 newHighScorePos = new Vector2(
                    (screenWidth - newHighScoreSize.X) / 2f,
                    screenHeight / 2f + 100
                );
                spriteBatch.DrawString(_font, newHighScoreText, newHighScorePos, Color.Yellow);
            }
        }
    }
}