using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace asteroids_finalproject
{
    public class GameGUI
    {
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public GameGUI(SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _font = font;
            _graphicsDevice = graphicsDevice;
        }

        public void DrawHUD(SpriteBatch spriteBatch, int currentLevel, int score, int lives)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            string levelText = $"Level: {currentLevel}";
            string scoreText = $"Score: {score}";
            string livesText = $"Lives: {lives}";
            
            Vector2 levelPos = new Vector2(20, 20);
            Vector2 scorePos = new Vector2(20, 55);
            Vector2 livesPos = new Vector2(20, 90);

            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            Vector2 levelSize = _font.MeasureString(levelText);
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 livesSize = _font.MeasureString(livesText);
            
            float maxWidth = Math.Max(Math.Max(levelSize.X, scoreSize.X), livesSize.X);
            
            spriteBatch.Draw(pixel, new Rectangle(15, 15, (int)maxWidth + 10, 105), Color.Black * 0.5f);
            
            spriteBatch.DrawString(_font, levelText, levelPos, Color.White);
            spriteBatch.DrawString(_font, scoreText, scorePos, Color.White);
            spriteBatch.DrawString(_font, livesText, livesPos, Color.White);
        }

        public void DrawGameOverScreen(SpriteBatch spriteBatch, int finalLevel, int finalScore)
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

            string levelText = $"Level Reached: {finalLevel}";
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
        }
    }
}