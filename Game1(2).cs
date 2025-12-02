using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace asteroids_finalproject
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _asteroidTexture;
        private Texture2D _spaceshipTexture;
        private Texture2D _projectileTexture;
        private Texture2D _powerupTexture;
        private Texture2D pixel;

        private List<Asteroid> _asteroids;
        private List<Asteroid> _asteroidsToRemove = new List<Asteroid>();
        private List<Projectile> _projectiles = new List<Projectile>();
        private List<Projectile> _projectilesToRemove = new List<Projectile>();
        private List<Powerup> _powerups = new List<Powerup>();
        private List<Powerup> _powerupsToRemove = new List<Powerup>();

        private Random _random;
        private float _asteroidSpawnTimer;
        private float _asteroidSpawnInterval = 2.5f;

        private float _levelTimer;
        private int _currentLevel = 1;
        private int _score = 0;
        private const float LEVEL_DURATION = 20f;

        private Spaceship spaceship;
        private SpriteFont _font;
        private GameGUI _gui;
        private bool _gameOver = false;

        private KeyboardState _previousKeyboardState;

        private HighScores _highScoreManager;

        private bool startTimer = false;
        private float ptimer = 0f;

        private bool isPaused = false;

        private SoundEffect _shootSound;

        private GameState _state = GameState.MainMenu;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
        }

        protected override void Initialize()
        {
            _asteroids = new List<Asteroid>();
            _random = new Random();
            _asteroidSpawnTimer = 0f;
            _levelTimer = 0f;

            _highScoreManager = new HighScores();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _asteroidTexture = Content.Load<Texture2D>("textures/asteroid");
            _spaceshipTexture = Content.Load<Texture2D>("textures/spaceshipTexture");
            _powerupTexture = Content.Load<Texture2D>("textures/powerupSS");
            Texture2D heartTexture = Content.Load<Texture2D>("textures/pixelheart");

            _font = Content.Load<SpriteFont>("font/GameFont");
            _gui = new GameGUI(_font, GraphicsDevice, heartTexture);

            _projectileTexture = new Texture2D(GraphicsDevice, 1, 1);
            _projectileTexture.SetData(new[] { Color.Yellow });

            spaceship = new Spaceship(
                _spaceshipTexture,
                new Vector2(_graphics.PreferredBackBufferWidth / 2f - 50, _graphics.PreferredBackBufferHeight / 2f)
            );

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _shootSound = Content.Load<SoundEffect>("sounds/shoot");
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState k = Keyboard.GetState();

            if (k.IsKeyDown(Keys.Escape))
                Exit();

            if (_state == GameState.MainMenu)
            {
                if (k.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
                {
                    _state = GameState.Playing;
                }

                _previousKeyboardState = k;
                return;
            }

            if (_state == GameState.Playing)
            {
                if (k.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
                    _state = GameState.Paused;
            }
            else if (_state == GameState.Paused)
            {
                if (k.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
                    _state = GameState.Playing;

                _previousKeyboardState = k;
                return;
            }

            if (_state == GameState.GameOver)
            {
                if (k.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                    _state = GameState.Playing;
                }

                _previousKeyboardState = k;
                return;
            }

            if (startTimer)
                ptimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ptimer >= 7f)
            {
                spaceship.changeSpeed(5f);
                spaceship.invincible = false;
                spaceship.trippleShot = false;
                spaceship.rapidFire = false;

                startTimer = false;
                ptimer = 0;
            }

            foreach (var powerup in _powerups)
                powerup.Update(gameTime);

            spaceship.Update(
                gameTime, k,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight
            );

            if (k.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                spaceship.Shoot(_projectiles, _projectileTexture);
                _shootSound.Play();
            }

            if (spaceship.GetLives() <= 0)
            {
                _state = GameState.GameOver;
                _highScoreManager.CheckAndUpdateHighScore(_score);
                return;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _levelTimer += deltaTime;
            if (_levelTimer >= LEVEL_DURATION)
            {
                _levelTimer = 0f;
                _currentLevel++;
                _asteroidSpawnInterval = Math.Max(0.5f, _asteroidSpawnInterval - 0.1f);
            }

            _asteroidSpawnTimer += deltaTime;
            if (_asteroidSpawnTimer >= _asteroidSpawnInterval)
            {
                _asteroidSpawnTimer = 0f;
                SpawnAsteroid();

                if (_random.NextDouble() < 0.1)
                    SpawnPowerup();
            }

            for (int i = _asteroids.Count - 1; i >= 0; i--)
            {
                _asteroids[i].Update(gameTime);

                if (_asteroids[i].ReachedBottom(_graphics.PreferredBackBufferHeight))
                {
                    if (!spaceship.invincible)
                        spaceship.LoseLife();

                    _asteroids.RemoveAt(i);
                }
                else if (_asteroids[i].IsOffScreen(
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight))
                {
                    _asteroids.RemoveAt(i);
                }
            }

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                _projectiles[i].Update(gameTime);

                if (_projectiles[i].Position.Y < 0)
                    _projectiles.RemoveAt(i);
            }

            _previousKeyboardState = k;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            if (_state == GameState.MainMenu)
            {
                string title = "ASTEROIDS";
                string sub = "Press ENTER to Start";
                string rules = "WASD = Move | SPACE = Shoot | F = Pause | R = Restart";

                Vector2 center = new Vector2(
                    _graphics.PreferredBackBufferWidth / 2f,
                    _graphics.PreferredBackBufferHeight / 2f
                );

                _spriteBatch.DrawString(_font, title,
                    center - new Vector2(_font.MeasureString(title).X / 2, 200),
                    Color.White);

                _spriteBatch.DrawString(_font, sub,
                    center - new Vector2(_font.MeasureString(sub).X / 2, 50),
                    Color.White);

                _spriteBatch.DrawString(_font, rules,
                    center - new Vector2(_font.MeasureString(rules).X / 2, -50),
                    Color.White);

                _spriteBatch.End();
                return;
            }

            foreach (var asteroid in _asteroids)
                asteroid.Draw(_spriteBatch);

            foreach (var projectile in _projectiles)
                projectile.Draw(_spriteBatch);

            foreach (var powerup in _powerups)
                powerup.Draw(_spriteBatch);

            spaceship.Draw(_spriteBatch);

            _gui.Draw(_spriteBatch, spaceship.GetLives(), _score, _currentLevel);

            if (_state == GameState.GameOver)
            {
                string msg = "GAME OVER — Press R to Restart";
                _spriteBatch.DrawString(
                    _font,
                    msg,
                    new Vector2(
                        _graphics.PreferredBackBufferWidth / 2 - _font.MeasureString(msg).X / 2,
                        _graphics.PreferredBackBufferHeight / 2
                    ),
                    Color.White
                );
            }

            _spriteBatch.End();
        }



        private void RestartGame()
        {
            _asteroids.Clear();
            _projectiles.Clear();
            _powerups.Clear();

            _currentLevel = 1;
            _score = 0;
            spaceship.Reset();

            _gameOver = false;
            _state = GameState.Playing;
        }

        private void SpawnAsteroid()
        {
            float x = _random.Next(0, _graphics.PreferredBackBufferWidth - _asteroidTexture.Width);
            _asteroids.Add(new Asteroid(_asteroidTexture, new Vector2(x, -50)));
        }

        private void SpawnPowerup()
        {
            float x = _random.Next(0, _graphics.PreferredBackBufferWidth - _powerupTexture.Width);
            _powerups.Add(new Powerup(_powerupTexture, new Vector2(x, -40)));
        }
    }
}
