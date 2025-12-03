using Microsoft.Xna.Framework.Audio;

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
    public int frame = 99;
    private int explosionFrameY = 285;
    public int lives;
    public float speed;
    private float gunCooldown = 0.4f;
    private float lastShotTime = 0f;
    private int explosionFrames = 7;
    private int currentExplosionFrame = -1;
    private float explosionTime = 0f;
    public bool invincible = false;
    public bool trippleShot = false;
    public bool rapidFire = false;
    private Color color = Color.White;
    public Spaceship(Texture2D spriteSheet, Vector2 position)
    {
        this.spriteSheet = spriteSheet;
        this.position = position;
        lives = 3;
        speed = 5f;
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
        if (keyboardState.IsKeyDown(Keys.A) && position.X > 0)
        {
            position.X -= speed;
            img = new Rectangle(frame * 6, 0, frame, frame);
    
        }
        if (keyboardState.IsKeyDown(Keys.D) && position.X < width - frame)
        {
            position.X += speed;
            img = new Rectangle(frame * 3, 0, frame, frame);
        }
        if (keyboardState.IsKeyDown(Keys.W) && position.Y > 0)
        {
            position.Y -= speed;
        }
        if (keyboardState.IsKeyDown(Keys.S) && position.Y < height - frame)
        {
            position.Y += speed;
            img = new Rectangle(frame * 3, frame, frame, frame);
        }

    }

    public void Draw(SpriteBatch spriteBatch, float ptimer)
    {
        Vector2 origin = new Vector2(frame / 2f, frame / 2f);
        
        if (speed > 6f)
        {
            color = new Color(1f, 0.6f, 0.6f);
        }
        else if (rapidFire)
        {
            color = new Color(0.6f, 1f, 0.6f);
        }
        else if (trippleShot)
        {
            color = new Color(0.6f, 0.6f, 1f);
        }
        else
        {
            color = Color.White;
        }

        spriteBatch.Draw(spriteSheet, position + origin, img, color, -MathF.PI / 2, origin, new Vector2(1.3f, 1.3f), SpriteEffects.None, 0f);
        
    }

    public int GetLives()
    {
        return lives;
    }

    public void changeSpeed(float newSpeed)
    {
        speed = newSpeed;
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

    public void Shoot(List<Projectile> projectiles, Texture2D pixel, SoundEffect shootSound)
    {
        if (lastShotTime >= gunCooldown)
        {
            lastShotTime = 0f;
            if (trippleShot)
            {
                projectiles.Add(new Projectile(pixel, new Vector2(position.X + 43, position.Y + 10), new Vector2(-2.5f, -10f)));
                projectiles.Add(new Projectile(pixel, new Vector2(position.X + 53, position.Y + 10), new Vector2(2.5f, -10f)));
            }
            projectiles.Add(new Projectile(pixel, new Vector2(position.X + 48, position.Y + 10), new Vector2(0, -10f)));
            if (rapidFire)
            {
                lastShotTime = gunCooldown;
            }
            shootSound.Play();
        }
    }
}
