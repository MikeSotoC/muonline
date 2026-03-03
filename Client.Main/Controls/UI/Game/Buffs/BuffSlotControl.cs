#nullable enable
using System.Threading.Tasks;
using Client.Main.Content;
using Client.Main.Controllers;
using Client.Main.Core.Client;
using Client.Main.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Main.Controls.UI.Game.Buffs
{
    /// <summary>
    /// Single buff slot display - shows buff icon and basic info.
    /// </summary>
    public class BuffSlotControl : UIControl
    {
        private ActiveBuffState? _buff;
        private readonly LabelControl _fallbackLabel;
        private Texture2D? _iconTexture;
        private Rectangle _iconSource;

        public const int SLOT_SIZE = 36;

        public ActiveBuffState? Buff
        {
            get => _buff;
            set
            {
                _buff = value;
                UpdateDisplay();
            }
        }

        public BuffSlotControl()
        {
            AutoViewSize = false;
            ControlSize = new Point(SLOT_SIZE, SLOT_SIZE);
            ViewSize = ControlSize;
            Interactive = false;

            _fallbackLabel = new LabelControl
            {
                Text = string.Empty,
                TextColor = Color.Cyan,
                X = 2,
                Y = 8,
                ViewSize = new Point(SLOT_SIZE - 4, SLOT_SIZE - 4),
                TextAlign = HorizontalAlign.Center,
                Scale = 0.65f,
                Visible = false
            };
            Controls.Add(_fallbackLabel);

            UpdateDisplay();
        }

        public override async Task Load()
        {
            foreach (string texturePath in BuffIconAtlas.TexturePaths)
            {
                await TextureLoader.Instance.Prepare(texturePath);
            }

            await base.Load();
            RefreshIconTexture();
        }

        public override void Draw(GameTime gameTime)
        {
            if (Status != GameControlStatus.Ready || !Visible)
            {
                return;
            }

            if (_buff != null)
            {
                if (_iconTexture == null || _iconTexture.IsDisposed)
                {
                    RefreshIconTexture();
                }

                if (_iconTexture != null && !_iconTexture.IsDisposed)
                {
                    int iconX = DisplayRectangle.X + (DisplayRectangle.Width - BuffIconAtlas.IconWidth) / 2;
                    int iconY = DisplayRectangle.Y + (DisplayRectangle.Height - BuffIconAtlas.IconHeight) / 2;
                    var iconRect = new Rectangle(iconX, iconY, BuffIconAtlas.IconWidth, BuffIconAtlas.IconHeight);

                    GraphicsManager.Instance.Sprite.Draw(_iconTexture, iconRect, _iconSource, Color.White * Alpha);
                }
            }

            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Draw(gameTime);
            }
        }

        private void UpdateDisplay()
        {
            BackgroundColor = Color.Transparent;
            BorderColor = Color.Transparent;
            BorderThickness = 0;

            if (_buff == null)
            {
                _fallbackLabel.Visible = false;
                _iconTexture = null;
                return;
            }

            RefreshIconTexture();

            bool showFallback = _iconTexture == null;
            _fallbackLabel.Visible = showFallback;
            _fallbackLabel.Text = showFallback ? $"E{_buff.EffectId}" : string.Empty;
        }

        private void RefreshIconTexture()
        {
            _iconTexture = null;

            if (_buff == null)
            {
                return;
            }

            if (!BuffIconAtlas.TryResolve(_buff.EffectId, out var frame))
            {
                return;
            }

            _iconTexture = TextureLoader.Instance.GetTexture2D(frame.TexturePath);
            _iconSource = frame.SourceRectangle;
        }
    }
}
