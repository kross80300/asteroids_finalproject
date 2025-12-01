using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace asteroids_finalproject;

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
    private bool _gameOver = false;
    private KeyboardState _previousKeyboardState;
    
    private HighScores _highScoreManager;

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
            new Vector2(_spaceshipTexture.Width / 2f, _spaceshipTexture.Height / 2f), 5f);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState k = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            k.IsKeyDown(Keys.Escape))
            Exit();

        if (startTimer)
            ptimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ptimer >= 5)
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
            spaceship.Shoot(_projectiles, _projectileTexture, gameTime);
        }

        _previousKeyboardState = k;

        if (spaceship.GetLives() <= 0)
        {
            _gameOver = true;
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
            {
                SpawnPowerup();
            }
        }

        for (int i = _asteroids.Count - 1; i >= 0; i--)
        {
            _asteroids[i].Update(gameTime);

            if (_asteroids[i].ReachedBottom(_graphics.PreferredBackBufferHeight))
            {
                spaceship.LoseLife();
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
                spaceship.LoseLife();
                _asteroidsToRemove.Add(a);
            }
        }

        foreach (var po in _powerups)
        {
            if (spaceship.GetBounds().Intersects(po.GetBoundingBox()))
            {
                if (po.getType() == 0)
                {
                    spaceship.changeSpeed(9f);
                    startTimer = true;
                }
                else if (po.getType() == 1 && !startTimer)
                {
                    spaceship.invincible = true;
                    startTimer = true;
                }
                else if (po.getType() == 2 && !startTimer)
                {
                    spaceship.trippleShot = true;
                    startTimer = true;
                }
                else if (po.getType() == 3 && !startTimer)
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

        base.Update(gameTime);
    }

    private void RestartGame()
    {
        _gameOver = false;
        _score = 0;
        _currentLevel = 1;
        _levelTimer = 0f;
        _asteroidSpawnInterval = 2.5f;
        _asteroidSpawnTimer = 0f;
        
        _asteroids.Clear();
        _projectiles.Clear();
        _asteroidsToRemove.Clear();
        _projectilesToRemove.Clear();
        
        spaceship = new Spaceship(_spaceshipTexture,
            new Vector2(_spaceshipTexture.Width / 2f, _spaceshipTexture.Height / 2f), 5f);
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

        if (!_gameOver)
        {
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

            _gui.DrawHUD(_spriteBatch, _currentLevel, _score, spaceship.GetLives(), _highScoreManager.GetHighScore());
        }
        else
        {
            _gui.DrawGameOverScreen(_spriteBatch, _currentLevel, _score, spaceship.GetLives(), _highScoreManager.GetHighScore());
            spaceship.Update(gameTime, Keyboard.GetState(), _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            spaceship.Draw(_spriteBatch, ptimer);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
