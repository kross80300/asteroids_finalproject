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

        private List<Asteroid> _asteroids;
        private List<Asteroid> _asteroidsToRemove = new List<Asteroid>();
        private List<Projectile> _projectiles = new List<Projectile>();
        private List<Projectile> _projectilesToRemove = new List<Projectile>();
        private Random _random;
        private float _asteroidSpawnTimer;
        private float _asteroidSpawnInterval = 2.5f;

        private float _levelTimer;
        private int _currentLevel = 1;
        private int _score = 0;
        private const float LEVEL_DURATION = 20f;

        private Spaceship spaceship;
        private Texture2D _spaceshipTexture;
        private Texture2D _projectileTexture;

        private SpriteFont _font;
        private GameGUI _gui;
        private KeyboardState _previousKeyboardState;

        private HighScores _highScoreManager;
        private Texture2D _powerupTexture;
        private List<Powerup> _powerups = new List<Powerup>();
        private List<Powerup> _powerupsToRemove = new List<Powerup>();
        private bool startTimer = false;
        private float ptimer = 0f;
        private Texture2D pixel;

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
            _font = Content.Load<SpriteFont>("font/GameFont");
            _powerupTexture = Content.Load<Texture2D>("textures/powerupSS");

            Texture2D heartTexture = Content.Load<Texture2D>("textures/pixelheart");

            _gui = new GameGUI(_font, GraphicsDevice, heartTexture);

            _projectileTexture = new Texture2D(GraphicsDevice, 1, 1);
            _projectileTexture.SetData(new[] { Color.Yellow });

            spaceship = new Spaceship(_spaceshipTexture,
                new Vector2(_graphics.PreferredBackBufferWidth / 2f - 50, _graphics.PreferredBackBufferHeight / 2f));

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _shootSound = Content.Load<SoundEffect>("sounds/shoot");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState k = Keyboard.GetState();
            
            if (k.IsKeyDown(Keys.Escape))
                Exit();

            // Main Menu State
            if (_state == GameState.MainMenu)
            {
                if (k.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
                {
                    _state = GameState.Playing;
                }

                _previousKeyboardState = k;
                return;
            }

            // Pause/Unpause
            if (_state == GameState.Playing)
            {
                if (k.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P))
                {
                    _state = GameState.Paused;
                }
            }
            else if (_state == GameState.Paused)
            {
                if (k.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P))
                {
                    _state = GameState.Playing;
                }

                _previousKeyboardState = k;
                return;
            }

            // Game Over State
            if (_state == GameState.GameOver)
            {
                if (k.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }

                _previousKeyboardState = k;
                return;
            }

            // Powerup Timer
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
            {
                powerup.Update(gameTime);
            }

            spaceship.Update(gameTime, k, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            if (k.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                spaceship.Shoot(_projectiles, _projectileTexture);
                _shootSound.Play();
            }

            if (spaceship.GetLives() <= 0)
            {
                _state = GameState.GameOver;
                _highScoreManager.CheckAndUpdateHighScore(_score);
                _previousKeyboardState = k;
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
                {
                    SpawnPowerup();
                }
            }

            for (int i = _asteroids.Count - 1; i >= 0; i--)
            {
                _asteroids[i].Update(gameTime);

                if (_asteroids[i].ReachedBottom(_graphics.PreferredBackBufferHeight))
                {
                    if (!spaceship.invincible)
                    {
                        spaceship.LoseLife();
                    }
                    _asteroids.RemoveAt(i);
                }
                else if (_asteroids[i].IsOffScreen(_graphics.PreferredBackBufferWidth,
                             _graphics.PreferredBackBufferHeight))
                {
                    _asteroids.RemoveAt(i);
                }
            }

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                _projectiles[i].Update(gameTime);

                if (_projectiles[i].IsOffScreen(_graphics.PreferredBackBufferHeight))
                {
                    _projectiles.RemoveAt(i);
                }
            }

            // Collision: Spaceship with Asteroids
            foreach (var a in _asteroids)
            {
                if (spaceship.GetBounds().Intersects(a.GetBoundingBox()) && !spaceship.invincible)
                {
                    spaceship.LoseLife();
                    _asteroidsToRemove.Add(a);
                }
            }

            // Collision: Spaceship with Powerups
            foreach (var po in _powerups)
            {
                if (spaceship.GetBounds().Intersects(po.GetBoundingBox()))
                {
                    if (po.getType() == 0 && ptimer <= 0f)
                    {
                        spaceship.changeSpeed(9f);
                        startTimer = true;
                    }
                    else if (po.getType() == 1 && ptimer <= 0f)
                    {
                        spaceship.invincible = true;
                        startTimer = true;
                    }
                    else if (po.getType() == 2 && ptimer <= 0f)
                    {
                        spaceship.trippleShot = true;
                        startTimer = true;
                    }
                    else if (po.getType() == 3 && ptimer <= 0f)
                    {
                        spaceship.rapidFire = true;
                        startTimer = true;
                    }
                    _powerupsToRemove.Add(po);
                }
            }

            // Collision: Projectiles with Asteroids
            foreach (var projectile in _projectiles)
            {
                foreach (var asteroid in _asteroids)
                {
                    if (projectile.GetBounds().Intersects(asteroid.GetBoundingBox()))
                    {
                        _projectilesToRemove.Add(projectile);
                        if (asteroid.TakeDamage())
                        {
                            _asteroidsToRemove.Add(asteroid);
                            _score += 10;
                        }

                        break;
                    }
                }
            }

            _asteroids.RemoveAll(a => _asteroidsToRemove.Contains(a));
            _asteroidsToRemove.Clear();

            _projectiles.RemoveAll(p => _projectilesToRemove.Contains(p));
            _projectilesToRemove.Clear();

            _powerups.RemoveAll(po => _powerupsToRemove.Contains(po));
            _powerupsToRemove.Clear();

            _previousKeyboardState = k;

            base.Update(gameTime);
        }

        private void RestartGame()
        {
            _state = GameState.Playing;
            _score = 0;
            _currentLevel = 1;
            _levelTimer = 0f;
            _asteroidSpawnInterval = 2.5f;
            _asteroidSpawnTimer = 0f;

            _asteroids.Clear();
            _projectiles.Clear();
            _powerups.Clear();
            _asteroidsToRemove.Clear();
            _projectilesToRemove.Clear();
            _powerupsToRemove.Clear();

            startTimer = false;
            ptimer = 0f;

            spaceship = new Spaceship(_spaceshipTexture,
                new Vector2(_graphics.PreferredBackBufferWidth / 2f - 50, _graphics.PreferredBackBufferHeight / 2f));
        }

        private void SpawnAsteroid()
        {
            int screenWidth = _graphics.PreferredBackBufferWidth;

            Vector2 spawnPosition = new Vector2(_random.Next(screenWidth), -50);

            float baseSpeed = 1f + (_currentLevel * 0.15f);
            float speed = baseSpeed + (float)(_random.NextDouble() * 0.5f);
            Vector2 velocity = new Vector2(0, speed);

            float rotationSpeed = (float)(_random.NextDouble() - 0.5) * 0.1f;

            Asteroid asteroid = new Asteroid(spawnPosition, velocity, _asteroidTexture, rotationSpeed);
            _asteroids.Add(asteroid);
        }

        private void SpawnPowerup()
        {
            int screenWidth = _graphics.PreferredBackBufferWidth;
            Vector2 spawnPosition = new Vector2(_random.Next(screenWidth), -50);
            float yspeed = 2f + (float)(_random.NextDouble() * 1f);
            float xspeed = (float)(_random.NextDouble() * 2f - 1f);
            Vector2 velocity = new Vector2(xspeed, yspeed);
            int type = _random.Next(4);
            Powerup powerup = new Powerup(spawnPosition, velocity, _powerupTexture, type);
            _powerups.Add(powerup);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Main Menu
            if (_state == GameState.MainMenu)
            {
                string title = "ASTEROIDS";
                string sub = "Press ENTER to Start";
                string rules = "WASD = Move | SPACE = Shoot | P = Pause | R = Restart";

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

            // Game Over Screen
            if (_state == GameState.GameOver)
            {
                _gui.DrawGameOverScreen(_spriteBatch, _currentLevel, _score, _highScoreManager.GetHighScore());
                spaceship.Update(gameTime, Keyboard.GetState(), _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                spaceship.Draw(_spriteBatch, ptimer);
                _spriteBatch.End();
                return;
            }

            // Playing or Paused
            spaceship.Draw(_spriteBatch, ptimer);

            foreach (var asteroid in _asteroids)
            {
                asteroid.Draw(_spriteBatch);
            }

            foreach (var projectile in _projectiles)
            {
                projectile.Draw(_spriteBatch);
            }

            foreach (var powerup in _powerups)
            {
                powerup.Draw(_spriteBatch);
            }

            if (_state == GameState.Paused)
            {
                _spriteBatch.DrawString(_font, " | PAUSED |", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2 - 130, _graphics.PreferredBackBufferHeight / 2 - 10), 
                    Color.White);
            }

            _gui.DrawHUD(_spriteBatch, _currentLevel, _score, spaceship.GetLives(), _highScoreManager.GetHighScore());

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
