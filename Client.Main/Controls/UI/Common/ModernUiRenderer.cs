using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Client.Main.Controls.UI.Common
{
    /// <summary>
    /// Helper class for drawing modern UI elements with vector graphics
    /// Features: rounded corners, gradients, shadows, glassmorphism effects
    /// </summary>
    public static class ModernUiRenderer
    {
        private static Texture2D _whitePixel;
        private static Texture2D _roundCorner;

        /// <summary>
        /// Initializes the renderer with required textures
        /// </summary>
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(graphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }

            if (_roundCorner == null)
            {
                GenerateRoundedCornerTexture(graphicsDevice);
            }
        }

        private static void GenerateRoundedCornerTexture(GraphicsDevice graphicsDevice)
        {
            const int size = 16;
            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - (size - 1);
                    float dy = y - (size - 1);
                    float dist = MathF.Sqrt(dx * dx + dy * dy);

                    if (dist <= size - 1)
                    {
                        data[y * size + x] = Color.White;
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }

            _roundCorner = new Texture2D(graphicsDevice, size, size);
            _roundCorner.SetData(data);
        }

        /// <summary>
        /// Draws a rounded rectangle with gradient fill
        /// </summary>
        public static void DrawRoundedRectangle(
            SpriteBatch spriteBatch,
            Rectangle rect,
            int cornerRadius,
            Color fillColor,
            Color? borderColor = null,
            int borderThickness = 1,
            float opacity = 1f)
        {
            if (_whitePixel == null) return;

            int w = rect.Width;
            int h = rect.Height;
            int r = Math.Min(cornerRadius, Math.Min(w, h) / 2);

            // Calculate opacity color
            Color fillAlpha = new Color(
                (int)(fillColor.R * opacity),
                (int)(fillColor.G * opacity),
                (int)(fillColor.B * opacity),
                (int)(fillColor.A * opacity));

            // Center rectangle
            if (w > 2 * r && h > 2 * r)
            {
                spriteBatch.Draw(_whitePixel, new Rectangle(rect.X + r, rect.Y + r, w - 2 * r, h - 2 * r), fillAlpha);
            }

            // Top and bottom edges
            if (w > 2 * r)
            {
                spriteBatch.Draw(_whitePixel, new Rectangle(rect.X + r, rect.Y, w - 2 * r, r), fillAlpha);
                spriteBatch.Draw(_whitePixel, new Rectangle(rect.X + r, rect.Bottom - r, w - 2 * r, r), fillAlpha);
            }

            // Left and right edges
            if (h > 2 * r)
            {
                spriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y + r, r, h - 2 * r), fillAlpha);
                spriteBatch.Draw(_whitePixel, new Rectangle(rect.Right - r, rect.Y + r, r, h - 2 * r), fillAlpha);
            }

            // Corners
            if (_roundCorner != null)
            {
                float scale = r / (float)_roundCorner.Width;

                // Top-left
                spriteBatch.Draw(_roundCorner,
                    new Rectangle(rect.X, rect.Y, r, r),
                    null,
                    fillAlpha,
                    MathHelper.Pi,
                    new Vector2(_roundCorner.Width, 0),
                    SpriteEffects.None,
                    0);

                // Top-right
                spriteBatch.Draw(_roundCorner,
                    new Rectangle(rect.Right - r, rect.Y, r, r),
                    null,
                    fillAlpha,
                    MathHelper.PiOver2,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0);

                // Bottom-right
                spriteBatch.Draw(_roundCorner,
                    new Rectangle(rect.Right - r, rect.Bottom - r, r, r),
                    null,
                    fillAlpha,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0);

                // Bottom-left
                spriteBatch.Draw(_roundCorner,
                    new Rectangle(rect.X, rect.Bottom - r, r, r),
                    null,
                    fillAlpha,
                    -MathHelper.PiOver2,
                    new Vector2(0, _roundCorner.Height),
                    SpriteEffects.None,
                    0);
            }

            // Border
            if (borderColor.HasValue && borderThickness > 0)
            {
                DrawRoundedRectangleBorder(spriteBatch, rect, cornerRadius, borderColor.Value, borderThickness, opacity);
            }
        }

        private static void DrawRoundedRectangleBorder(
            SpriteBatch spriteBatch,
            Rectangle rect,
            int cornerRadius,
            Color color,
            int thickness,
            float opacity)
        {
            if (_whitePixel == null || _roundCorner == null) return;

            int w = rect.Width;
            int h = rect.Height;
            int r = Math.Min(cornerRadius, Math.Min(w, h) / 2);

            Color alphaColor = new Color(
                (int)(color.R * opacity),
                (int)(color.G * opacity),
                (int)(color.B * opacity),
                (int)(color.A * opacity));

            // Draw 4 rectangles for edges
            spriteBatch.Draw(_whitePixel, new Rectangle(rect.X + r, rect.Y, w - 2 * r, thickness), alphaColor);
            spriteBatch.Draw(_whitePixel, new Rectangle(rect.X + r, rect.Bottom - thickness, w - 2 * r, thickness), alphaColor);
            spriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y + r, thickness, h - 2 * r), alphaColor);
            spriteBatch.Draw(_whitePixel, new Rectangle(rect.Right - thickness, rect.Y + r, thickness, h - 2 * r), alphaColor);

            // Corner arcs (simplified as small rectangles at corners)
            float scale = r / (float)_roundCorner.Width;
            
            // Sample points around the corner arc
            int samples = 8;
            for (int i = 0; i < samples; i++)
            {
                float angleTopLeft = MathHelper.Pi + (MathHelper.PiOver2 * i / samples);
                float angleTopRight = -MathHelper.PiOver2 + (MathHelper.PiOver2 * i / samples);
                float angleBottomRight = 0 + (MathHelper.PiOver2 * i / samples);
                float angleBottomLeft = MathHelper.PiOver2 + (MathHelper.PiOver2 * i / samples);

                Vector2 posTL = new Vector2(rect.X + r + r * MathF.Cos(angleTopLeft), rect.Y + r + r * MathF.Sin(angleTopLeft));
                Vector2 posTR = new Vector2(rect.Right - r + r * MathF.Cos(angleTopRight), rect.Y + r + r * MathF.Sin(angleTopRight));
                Vector2 posBR = new Vector2(rect.Right - r + r * MathF.Cos(angleBottomRight), rect.Bottom - r + r * MathF.Sin(angleBottomRight));
                Vector2 posBL = new Vector2(rect.X + r + r * MathF.Cos(angleBottomLeft), rect.Bottom - r + r * MathF.Sin(angleBottomLeft));

                spriteBatch.Draw(_whitePixel, new Rectangle((int)posTL.X, (int)posTL.Y, thickness, thickness), alphaColor);
                spriteBatch.Draw(_whitePixel, new Rectangle((int)posTR.X, (int)posTR.Y, thickness, thickness), alphaColor);
                spriteBatch.Draw(_whitePixel, new Rectangle((int)posBR.X, (int)posBR.Y, thickness, thickness), alphaColor);
                spriteBatch.Draw(_whitePixel, new Rectangle((int)posBL.X, (int)posBL.Y, thickness, thickness), alphaColor);
            }
        }

        /// <summary>
        /// Draws a modern panel with glassmorphism effect
        /// </summary>
        public static void DrawGlassPanel(
            SpriteBatch spriteBatch,
            Rectangle rect,
            int cornerRadius,
            Color baseColor,
            float blurIntensity = 0f,
            Color? borderColor = null,
            bool withShadow = true)
        {
            if (_whitePixel == null) return;

            // Shadow
            if (withShadow)
            {
                Rectangle shadowRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);
                Color shadowColor = new Color(0, 0, 0, 80);
                DrawRoundedRectangle(spriteBatch, shadowRect, cornerRadius, shadowColor, null, 0, 0.5f);
            }

            // Main panel with semi-transparent fill
            Color glassColor = new Color(
                baseColor.R,
                baseColor.G,
                baseColor.B,
                (int)(baseColor.A * 0.7f));

            DrawRoundedRectangle(spriteBatch, rect, cornerRadius, glassColor, borderColor, 1, 1f);

            // Highlight gradient on top edge for glass effect
            if (rect.Height > 20)
            {
                Rectangle highlightRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, 1);
                Color highlightColor = new Color(255, 255, 255, 60);
                spriteBatch.Draw(_whitePixel, highlightRect, highlightColor);
            }
        }

        /// <summary>
        /// Draws a vertical gradient
        /// </summary>
        public static void DrawGradientVertical(
            SpriteBatch spriteBatch,
            Rectangle rect,
            Color topColor,
            Color bottomColor,
            int cornerRadius = 0)
        {
            if (_whitePixel == null) return;

            if (cornerRadius > 0)
            {
                // Simplified: just draw gradient without rounded corners for now
                int steps = Math.Min(rect.Height, 32);
                for (int i = 0; i < steps; i++)
                {
                    float t = (float)i / (steps - 1);
                    Color color = Color.Lerp(topColor, bottomColor, t);
                    int y = rect.Y + i * (rect.Height / steps);
                    int height = rect.Height / steps + 1;
                    spriteBatch.Draw(_whitePixel, new Rectangle(rect.X, y, rect.Width, height), color);
                }
            }
            else
            {
                int steps = Math.Min(rect.Height, 32);
                for (int i = 0; i < steps; i++)
                {
                    float t = (float)i / (steps - 1);
                    Color color = Color.Lerp(topColor, bottomColor, t);
                    int y = rect.Y + i * (rect.Height / steps);
                    int height = rect.Height / steps + 1;
                    spriteBatch.Draw(_whitePixel, new Rectangle(rect.X, y, rect.Width, height), color);
                }
            }
        }

        /// <summary>
        /// Draws a modern button with hover effect
        /// </summary>
        public static void DrawModernButton(
            SpriteBatch spriteBatch,
            Rectangle rect,
            int cornerRadius,
            Color normalColor,
            Color hoverColor,
            Color pressedColor,
            bool isHovered,
            bool isPressed,
            string text,
            SpriteFont font,
            Color textColor)
        {
            Color fillColor = isPressed ? pressedColor : (isHovered ? hoverColor : normalColor);
            
            DrawRoundedRectangle(spriteBatch, rect, cornerRadius, fillColor, null, 0, 1f);

            // Draw text centered
            if (font != null && !string.IsNullOrEmpty(text))
            {
                Vector2 textSize = font.MeasureString(text);
                Vector2 textPos = new Vector2(
                    rect.X + (rect.Width - textSize.X) / 2,
                    rect.Y + (rect.Height - textSize.Y) / 2);
                
                // Text shadow
                spriteBatch.DrawString(font, text, textPos + new Vector2(1, 1), Color.Black * 0.5f);
                spriteBatch.DrawString(font, text, textPos, textColor);
            }
        }

        /// <summary>
        /// Draws a checkbox with modern styling
        /// </summary>
        public static void DrawCheckbox(
            SpriteBatch spriteBatch,
            Rectangle rect,
            int cornerRadius,
            bool isChecked,
            Color uncheckedColor,
            Color checkedColor,
            Color checkmarkColor)
        {
            Color fillColor = isChecked ? checkedColor : uncheckedColor;
            DrawRoundedRectangle(spriteBatch, rect, cornerRadius, fillColor, null, 0, 1f);

            if (isChecked && _whitePixel != null)
            {
                // Draw checkmark
                int padding = rect.Width / 4;
                int centerX = rect.X + rect.Width / 2;
                int centerY = rect.Y + rect.Height / 2;

                // Simple checkmark shape using lines
                spriteBatch.Draw(_whitePixel, 
                    new Rectangle(centerX - rect.Width/4, centerY, 2, rect.Height/4), 
                    checkmarkColor);
                spriteBatch.Draw(_whitePixel, 
                    new Rectangle(centerX - rect.Width/4, centerY + rect.Height/6, rect.Width/3, 2), 
                    checkmarkColor);
            }
        }

        /// <summary>
        /// Gets the white pixel texture for primitive drawing
        /// </summary>
        public static Texture2D WhitePixel => _whitePixel;
    }
}
