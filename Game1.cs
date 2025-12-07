using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace asteroids_finalproject;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    EnteringInitials
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _explosionPlayed = false;
    private Texture2D _asteroidTexture;
    private SoundEffect _shootSound;
    private SoundEffect _explosionSound;
    private GameState _currentGameState = GameState.MainMenu;

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

    public Spaceship spaceship;
    private Texture2D _spaceshipTexture;
    private Texture2D _projectileTexture;

    private SpriteFont _font;
    private GameGUI _gui;
    private bool _gameOver = false;
    private KeyboardState _previousKeyboardState;

    private HighScores _highScoreManager;
    private Texture2D _powerupTexture;
    private Texture2D _shieldTexture;
    private List<Powerup> _powerups = new List<Powerup>();
    private List<Powerup> _powerupsToRemove = new List<Powerup>();
    private Shield shield;
    private bool startTimer = false;
    private float ptimer = 0f;
    private Texture2D pixel;
    private bool isPaused = false;
    
    private string _playerInitials = "";
    private bool _isNewHighScore = false;

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
        _shieldTexture = Content.Load<Texture2D>("textures/projectile");

        Texture2D heartTexture = Content.Load<Texture2D>("textures/pixelheart");

        _gui = new GameGUI(_font, GraphicsDevice, heartTexture);

        _projectileTexture = new Texture2D(GraphicsDevice, 1, 1);
        _projectileTexture.SetData(new[] { Color.Yellow });

        spaceship = new Spaceship(_spaceshipTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f - 50, _graphics.PreferredBackBufferHeight / 2f));

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        _shootSound = Content.Load<SoundEffect>("sounds/shoot");
        _explosionSound = Content.Load<SoundEffect>("sounds/explosion");
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState k = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            k.IsKeyDown(Keys.Escape))
            Exit();
            
        switch (_currentGameState)
        {
            case GameState.MainMenu:
                if (k.IsKeyDown(Keys.Enter))
                {
                    _currentGameState = GameState.Playing;
                }
                break;

            case GameState.Playing:
                if (k.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
                    _currentGameState = GameState.Paused;
                    
                if (startTimer)
                    ptimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (ptimer >= 7f)
                {
                    spaceship.superSpeed = false;
                    spaceship.invincible = false;
                    spaceship.trippleShot = false;
                    spaceship.rapidFire = false;
                    startTimer = false;
                    ptimer = 0;
                    shield = null;
                }

                foreach (var powerup in _powerups)
                {
                    powerup.Update(gameTime);
                }

                spaceship.Update(gameTime, k, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, _currentLevel);
                if (shield != null)
                {
                    shield.Update(ptimer);
                }
                
                if (spaceship.GetLives() <= 0 && !_explosionPlayed)
                {
                    _explosionSound.Play();
                    _explosionPlayed = true;
                    _gameOver = true;

                    if (_highScoreManager.IsHighScore(_score))
                    {
                        _isNewHighScore = true;
                        _playerInitials = "";
                        _currentGameState = GameState.EnteringInitials;
                    }
                    else
                    {
                        _currentGameState = GameState.GameOver;
                    }
                    return;
                }

                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                _levelTimer += deltaTime;
                if (_levelTimer >= LEVEL_DURATION)
                {
                    _levelTimer = 0f;
                    _currentLevel++;
                    _asteroidSpawnInterval = Math.Max(0.3f, _asteroidSpawnInterval - 0.2f);
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

                foreach (var a in _asteroids)
                {
                    if (spaceship.GetBounds().Intersects(a.GetBoundingBox()))
                    {
                        if (!spaceship.invincible)
                        {
                            spaceship.LoseLife();
                        }
                        _asteroidsToRemove.Add(a);
                    }
                }

                foreach (var po in _powerups)
                {
                    if (spaceship.GetBounds().Intersects(po.GetBoundingBox()))
                    {
                        if (po.getType() == 0 && ptimer <= 0f)
                        {
                            spaceship.superSpeed = true;
                            startTimer = true;
                        }
                        else if (po.getType() == 1 && ptimer <= 0f)
                        {
                            spaceship.invincible = true;
                            shield = new Shield(spaceship.position, _shieldTexture);
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
                                _explosionSound.Play();
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
                
                if (k.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
                {
                    spaceship.Shoot(_projectiles, _projectileTexture, _shootSound);
                }
                break;

            case GameState.Paused:
                if (k.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
                    _currentGameState = GameState.Playing;
                break;

            case GameState.EnteringInitials:
                HandleInitialsInput(k);
                break;

            case GameState.GameOver:
                if (k.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
                    RestartGame();
                break;
        }
        
        _previousKeyboardState = k;
        base.Update(gameTime);
    }

    private void HandleInitialsInput(KeyboardState k)
    {
        Keys[] pressedKeys = k.GetPressedKeys();
        
        foreach (Keys key in pressedKeys)
        {
            if (!_previousKeyboardState.IsKeyDown(key))
            {
                if (key >= Keys.A && key <= Keys.Z && _playerInitials.Length < 3)
                {
                    _playerInitials += key.ToString();
                }
                else if (key == Keys.Back && _playerInitials.Length > 0)
                {
                    _playerInitials = _playerInitials.Substring(0, _playerInitials.Length - 1);
                }
                else if (key == Keys.Enter && _playerInitials.Length > 0)
                {
                    _highScoreManager.AddHighScore(_playerInitials, _score);
                    _isNewHighScore = false;
                    _currentGameState = GameState.GameOver;
                }
            }
        }
    }

    private void RestartGame()
    {
        _gameOver = false;
        _explosionPlayed = false;
        _score = 0;
        _currentLevel = 1;
        _levelTimer = 0f;
        _asteroidSpawnInterval = 2.5f;
        _asteroidSpawnTimer = 0f;
        _playerInitials = "";
        _isNewHighScore = false;

        _asteroids.Clear();
        _projectiles.Clear();
        _powerups.Clear();
        _asteroidsToRemove.Clear();
        _projectilesToRemove.Clear();
        _powerupsToRemove.Clear();
        
        spaceship = new Spaceship(_spaceshipTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f - 50, _graphics.PreferredBackBufferHeight / 2f));
        _currentGameState = GameState.Playing;
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

        switch (_currentGameState)
        {
            case GameState.MainMenu:
                _spriteBatch.DrawString(_font, "ASTEROIDS GAME", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 180, 
                                _graphics.PreferredBackBufferHeight / 2f - 10), 
                    Color.RoyalBlue);
                _spriteBatch.DrawString(_font, "Press ENTER to Start", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 260, 
                                _graphics.PreferredBackBufferHeight / 2f + 40), 
                    Color.Red);
                _spriteBatch.DrawString(_font, "WASD to Move, SPACE to Shoot", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 350, 
                                _graphics.PreferredBackBufferHeight / 2f + 80), 
                    Color.Red);
                _spriteBatch.DrawString(_font, "F to Pause, R to Restart", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 310, 
                                _graphics.PreferredBackBufferHeight / 2f + 120), 
                    Color.Green);
                break;

            case GameState.Playing:
                spaceship.Draw(_spriteBatch, ptimer);
                if (shield != null)
                {
                    shield.Draw(_spriteBatch, spaceship);
                }

                foreach (var asteroid in _asteroids)
                    asteroid.Draw(_spriteBatch);

                foreach (var projectile in _projectiles)
                    projectile.Draw(_spriteBatch);

                foreach (var powerup in _powerups)
                    powerup.Draw(_spriteBatch);

                _gui.DrawHUD(_spriteBatch, _currentLevel, _score, spaceship.GetLives(), _highScoreManager.GetHighScore(), _highScoreManager.GetHighScoreInitials());
                break;

            case GameState.Paused:
                spaceship.Draw(_spriteBatch, ptimer);
                if (shield != null)
                {
                    shield.Draw(_spriteBatch, spaceship);
                }

                foreach (var asteroid in _asteroids)
                    asteroid.Draw(_spriteBatch);

                foreach (var projectile in _projectiles)
                    projectile.Draw(_spriteBatch);

                foreach (var powerup in _powerups)
                    powerup.Draw(_spriteBatch);

                _gui.DrawHUD(_spriteBatch, _currentLevel, _score, spaceship.GetLives(), _highScoreManager.GetHighScore(), _highScoreManager.GetHighScoreInitials());
                
                _spriteBatch.DrawString(_font, "| PAUSED |", 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 130, 
                                _graphics.PreferredBackBufferHeight / 2f - 10), 
                    Color.White);
                break;

            case GameState.EnteringInitials:
                int currentHighScore = _highScoreManager.GetHighScore();
                bool isActuallyNewHigh = _score > currentHighScore;
                string prompt = isActuallyNewHigh ? "NEW HIGH SCORE!" : "HIGH SCORE!";
                string instruction = "Enter Your Initials (Max 3 letters):";
                string currentInitials = _playerInitials + "_";
                string submitText = "Press ENTER to Submit";
                string scoreText = $"Score: {_score}";
                
                _spriteBatch.DrawString(_font, prompt, 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 200, 
                                _graphics.PreferredBackBufferHeight / 2f - 150), 
                    Color.Gold);
                
                _spriteBatch.DrawString(_font, scoreText, 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 100, 
                                _graphics.PreferredBackBufferHeight / 2f - 100), 
                    Color.White);
                
                _spriteBatch.DrawString(_font, instruction, 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 350, 
                                _graphics.PreferredBackBufferHeight / 2f - 40), 
                    Color.White);
                
                _spriteBatch.DrawString(_font, currentInitials, 
                    new Vector2(_graphics.PreferredBackBufferWidth / 2f - 60, 
                                _graphics.PreferredBackBufferHeight / 2f + 20), 
                    Color.Cyan, 0f, Vector2.Zero, 2.5f, SpriteEffects.None, 0f);
                
                if (_playerInitials.Length > 0)
                {
                    _spriteBatch.DrawString(_font, submitText, 
                        new Vector2(_graphics.PreferredBackBufferWidth / 2f - 220, 
                                    _graphics.PreferredBackBufferHeight / 2f + 120), 
                        Color.Green);
                }
                else
                {
                    _spriteBatch.DrawString(_font, "Press BACKSPACE to delete", 
                        new Vector2(_graphics.PreferredBackBufferWidth / 2f - 260, 
                                    _graphics.PreferredBackBufferHeight / 2f + 120), 
                        Color.Gray);
                }
                break;

            case GameState.GameOver:
                _gui.DrawGameOverScreen(_spriteBatch, _currentLevel, _score, _highScoreManager.GetHighScore());
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
