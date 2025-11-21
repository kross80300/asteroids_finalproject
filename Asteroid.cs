using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace asteroids_finalproject;

public class Asteroid
{
   Vector2 position;
    Vector2 velocity;
    Texture2D asteroidTexture;
    Rectangle boundingBox;
    int hitPoints = 3;
    float rotationAngle;
    float rotationSpeed;
    float scale = 0.2f;
    
    public Asteroid(Vector2 pos, Vector2 vel, Texture2D texture, float rotSpeed)
    {
        position = pos;
        velocity = vel;
        asteroidTexture = texture;
        rotationSpeed = rotSpeed;
        hitPoints = 1;
        rotationAngle = 0f;
    }

    public void Update(GameTime gameTime)
    {
        position += velocity;
        
        rotationAngle += rotationSpeed;

        if (rotationAngle > MathHelper.TwoPi)
            rotationAngle -= MathHelper.TwoPi;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            asteroidTexture,
            position,
            null,
            Color.White,
            rotationAngle,
            new Vector2(asteroidTexture.Width / 2f, asteroidTexture.Height / 2f),
            scale,
            SpriteEffects.None,
            0f
        );
    }

    public bool TakeDamage()
    {
        hitPoints--;
        if (hitPoints <= 0)
        {
            return true;
        }
        return false;
    }

    public Rectangle GetBoundingBox()
    {
        int scaledWidth = (int)(asteroidTexture.Width * scale);
        int scaledHeight = (int)(asteroidTexture.Height * scale);

        return new Rectangle(
            (int)(position.X - scaledWidth / 2f),
            (int)(position.Y - scaledHeight / 2f),
            scaledWidth,
            scaledHeight
        );
    }

    public bool IsOffScreen(int screenWidth, int screenHeight)
    {
        float scaledHalfWidth = (asteroidTexture.Width * scale) / 2f;
        float scaledHalfHeight = (asteroidTexture.Height * scale) / 2f;

        if (position.X + scaledHalfWidth < 0 ||
            position.X - scaledHalfWidth > screenWidth ||
            position.Y + scaledHalfHeight < 0 ||
            position.Y - scaledHalfHeight > screenHeight)
        {
            return true;
        }
        return false;
    } 
}