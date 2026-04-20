using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Main.Helpers.UI
{
    /// <summary>
    /// Renderizador vectorial moderno para interfaces de usuario en MonoGame.
    /// Elimina la dependencia de imágenes estáticas para controles básicos.
    /// </summary>
    public static class ModernUiRenderer
    {
        private static Texture2D _whitePixel;
        private static GraphicsDevice _graphicsDevice;

        // Paleta de Colores Modernos (Tema Base: Dark Glass)
        public static class Colors
        {
            public static readonly Color BackgroundDark = new Color(20, 20, 25);
            public static readonly Color PanelBg = new Color(40, 44, 52);
            public static readonly Color PanelBgTransparent = new Color(180, 40, 44, 52);
            public static readonly Color AccentPrimary = new Color(0, 198, 255); // Cyan
            public static readonly Color AccentSecondary = new Color(114, 9, 183); // Purple
            public static readonly Color Success = new Color(0, 200, 83);
            public static readonly Color Danger = new Color(255, 82, 82);
            public static readonly Color Warning = new Color(255, 193, 7);
            public static readonly Color TextMain = new Color(240, 240, 240);
            public static readonly Color TextMuted = new Color(150, 150, 160);
            public static readonly Color Border = new Color(60, 60, 70);
            public static readonly Color Hover = new Color(60, 65, 75);
        }

        /// <summary>
        /// Inicializa el renderizador con los recursos necesarios.
        /// Debe llamarse una vez al iniciar el juego.
        /// </summary>
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(graphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
        }

        /// <summary>
        /// Textura blanca de 1x1 para dibujar rectángulos.
        /// </summary>
        public static Texture2D WhitePixel => _whitePixel;

        #region Panel

        /// <summary>
        /// Dibuja un panel moderno con esquinas redondeadas y efecto glass.
        /// </summary>
        public static void DrawGlassPanel(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds, int cornerRadius = 8, Color? bgColor = null, Color? borderColor = null, bool withShadow = true)
        {
            var background = bgColor ?? Colors.PanelBgTransparent;
            var border = borderColor ?? Colors.Border;

            // Sombra
            if (withShadow)
            {
                var shadowRect = new Microsoft.Xna.Framework.Rectangle(bounds.X + 3, bounds.Y + 3, bounds.Width, bounds.Height);
                DrawRoundedRectangle(spriteBatch, shadowRect, cornerRadius, Color.Black * 0.3f, true);
            }

            // Fondo principal
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, background, true);

            // Brillo superior (efecto vidrio)
            var gradientHeight = bounds.Height / 3;
            var highlightRect = new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y, bounds.Width, gradientHeight);
            
            // Recortar para solo dibujar en la parte superior
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp);
            
            for (int i = 0; i < gradientHeight; i++)
            {
                float alpha = 1f - (i / (float)gradientHeight);
                var lineRect = new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y + i, bounds.Width, 1);
                DrawRoundedRectangle(spriteBatch, lineRect, cornerRadius, Color.White * alpha * 0.1f, true, clipToTop: true);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp);

            // Borde
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, border, false, thickness: 1);
        }

        #endregion

        #region Button

        public enum ButtonState { Normal, Hover, Pressed, Disabled }

        /// <summary>
        /// Dibuja un botón moderno con estados visuales.
        /// </summary>
        public static void DrawModernButton(
            SpriteBatch spriteBatch,
            Microsoft.Xna.Framework.Rectangle bounds,
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
            Color bgColor = isPressed ? pressedColor : (isHovered ? hoverColor : normalColor);
            
            // Fondo botón
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, bgColor, true);
            
            // Borde sutil
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, Color.White * 0.1f, false, thickness: 1);

            // Texto centrado
            if (!string.IsNullOrEmpty(text) && font != null)
            {
                var textSize = font.MeasureString(text);
                float x = bounds.X + (bounds.Width - textSize.X) / 2;
                float y = bounds.Y + (bounds.Height - textSize.Y) / 2;
                
                // Sombra texto
                spriteBatch.DrawString(font, text, new Vector2(x + 1, y + 1), Color.Black * 0.5f);
                spriteBatch.DrawString(font, text, new Vector2(x, y), textColor);
            }
        }

        #endregion

        #region Checkbox

        /// <summary>
        /// Dibuja un checkbox moderno.
        /// </summary>
        public static void DrawCheckbox(
            SpriteBatch spriteBatch,
            Microsoft.Xna.Framework.Rectangle bounds,
            int cornerRadius,
            bool isChecked,
            Color uncheckedColor,
            Color checkedColor,
            Color checkmarkColor)
        {
            // Fondo caja
            var bgColor = isChecked ? checkedColor : uncheckedColor;
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, bgColor, true);
            
            // Borde
            DrawRoundedRectangle(spriteBatch, bounds, cornerRadius, Color.White * 0.2f, false, thickness: 1);

            // Checkmark
            if (isChecked)
            {
                int padding = bounds.Width / 4;
                int midY = bounds.Y + bounds.Height / 2;
                
                // Línea 1
                DrawLine(spriteBatch, 
                    new Vector2(bounds.X + padding, midY),
                    new Vector2(bounds.X + bounds.Width / 3, bounds.Y + bounds.Height - padding),
                    checkmarkColor, 2);
                
                // Línea 2
                DrawLine(spriteBatch,
                    new Vector2(bounds.X + bounds.Width / 3, bounds.Y + bounds.Height - padding),
                    new Vector2(bounds.X + bounds.Width - padding, bounds.Y + padding),
                    checkmarkColor, 2);
            }
        }

        #endregion

        #region Helpers

        private static void DrawRoundedRectangle(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds, int radius, Color color, bool filled, int thickness = 1, bool clipToTop = false)
        {
            if (radius <= 0 || filled)
            {
                // Dibujar rectángulo relleno simple (las esquinas redondeadas reales requieren más lógica)
                spriteBatch.Draw(_whitePixel, bounds, color);
                
                // Simular esquinas redondeadas dibujando píxeles en las esquinas (simplificado)
                if (radius > 0 && filled)
                {
                    // Esquinas superiores
                    spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X + radius, bounds.Y, bounds.Width - radius * 2, radius), color);
                    spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y + radius, radius, bounds.Height - radius * 2), color);
                    spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.Right - radius, bounds.Y + radius, radius, bounds.Height - radius * 2), color);
                    spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X + radius, bounds.Bottom - radius, bounds.Width - radius * 2, radius), color);
                }
                return;
            }

            // Para bordes redondeados, dibujamos líneas y arcos simplificados
            // Líneas laterales
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y + radius, thickness, bounds.Height - radius * 2), color); // Izquierda
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.Right - thickness, bounds.Y + radius, thickness, bounds.Height - radius * 2), color); // Derecha
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X + radius, bounds.Y, bounds.Width - radius * 2, thickness), color); // Arriba
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X + radius, bounds.Bottom - thickness, bounds.Width - radius * 2, thickness), color); // Abajo

            // Arcos simplificados (cuadrantes)
            int diameter = radius * 2;
            
            // Superior izquierda
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y, radius, radius), color);
            // Superior derecha
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.Right - radius, bounds.Y, radius, radius), color);
            // Inferior izquierda
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Bottom - radius, radius, radius), color);
            // Inferior derecha
            spriteBatch.Draw(_whitePixel, new Microsoft.Xna.Framework.Rectangle(bounds.Right - radius, bounds.Bottom - radius, radius, radius), color);
        }

        private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            spriteBatch.Draw(_whitePixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
        }

        #endregion
    }
}
