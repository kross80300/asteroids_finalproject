namespace asteroids_finalproject;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Spaceship
{
    private Texture2D spriteSheet;
    public Vector2 position;
    private Rectangle img;
    private int frame = 99;
    private int explosionFrameY = 285;
    private int lives;
    public float speed = 5f;
    private float gunCooldown = 0.4f;
    private float lastShotTime = 0f;
    private int explosionFrames = 7;
    private int currentExplosionFrame = -1;
    private float explosionTime = 0f;
    public Spaceship(Texture2D spriteSheet, Vector2 position, float speed)
    {
        this.spriteSheet = spriteSheet;
        this.position = position;
        this.speed = speed;
        lives = 3;
    }

    public void Update(GameTime time, KeyboardState keyboardState, int width, int height)
    {
        lastShotTime += (float)time.ElapsedGameTime.TotalSeconds;
        if (lives <= 0)
        {
            Explode(time);
            return;
        } 
        img = new Rectangle(0, 0, frame, frame);
        if (keyboardState.IsKeyDown(Keys.Left) && position.X > 0)
        {
            position.X -= speed;
            img = new Rectangle(frame * 6, 0, frame, frame);
    
        }
        if (keyboardState.IsKeyDown(Keys.Right) && position.X < width - frame)
        {
            position.X += speed;
            img = new Rectangle(frame * 3, 0, frame, frame);
        }
        if (keyboardState.IsKeyDown(Keys.Up) && position.Y > 0)
        {
            position.Y -= speed;
        }
        if (keyboardState.IsKeyDown(Keys.Down) && position.Y < height - frame)
        {
            position.Y += speed;
            img = new Rectangle(frame * 3, frame, frame, frame);
        }

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 origin = new Vector2(frame / 2f, frame / 2f);
        spriteBatch.Draw(spriteSheet, position + origin, img, Color.White, -MathF.PI / 2, origin, new Vector2(1.3f, 1.3f), SpriteEffects.None, 0f);
    }

    public int GetLives()
    {
        return lives;
    }

    public Rectangle GetBounds()
    {
        return new Rectangle((int)position.X + 20, (int)position.Y + 25, 60, 50);
    }

    public void LoseLife()
    {
        lives = Math.Max(0, lives - 1);

    }

    public void Explode(GameTime time)
    {
        explosionTime += (float)time.ElapsedGameTime.TotalSeconds;
        if (explosionTime >= 0.25f)
        {
            explosionTime = 0f;
            currentExplosionFrame++;
            if (currentExplosionFrame > explosionFrames)
            {
                currentExplosionFrame = explosionFrames - 1;
                explosionFrameY = 186;
            }
            img = new Rectangle(currentExplosionFrame * frame, explosionFrameY, frame, frame);
            currentExplosionFrame++;
        }
    }

    public void Shoot(List<Projectile> projectiles, Texture2D pixel, GameTime gameTime)
    {
        if (lastShotTime >= gunCooldown)
        {
            lastShotTime = 0f;
            projectiles.Add(new Projectile(pixel, new Vector2(position.X + 48, position.Y + 10), new Vector2(0, -10f)));
        }
    }
}