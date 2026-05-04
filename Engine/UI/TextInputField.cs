using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleEngine2D.Engine.UI
{
    public class TextInputField
    {
        public string Text { get; private set; }
        public bool IsActive { get; private set; }
        public Rectangle Bounds { get; set; }

        private int _cursorPosition;
        private double _blinkTimer;
        private bool _showCursor = true;

        private string _originalText = "";

        public event Action<string>? OnConfirmed;
        public event Action? OnCancelled;

        public TextInputField(Rectangle bounds, string startingText = "")
        {
            Bounds = bounds;
            Text = startingText;
            _cursorPosition = Text.Length;
        }

        public void Activate(GameWindow window)
        {
            if (IsActive) return;

            IsActive = true;
            _originalText = Text;
            _cursorPosition = Text.Length;

            window.TextInput += HandleTextInput;
        }

        public void Deactivate(GameWindow window)
        {
            if (!IsActive) return;

            IsActive = false;
            window.TextInput -= HandleTextInput;
        }

        private void HandleTextInput(object? sender, TextInputEventArgs e)
        {
            if (!IsActive) return;

            if (e.Key == Keys.Back)
            {
                if (_cursorPosition > 0 && Text.Length > 0)
                {
                    Text = Text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                }
                return;
            }

            if (e.Key == Keys.Enter)
            {
                OnConfirmed?.Invoke(Text);
                return;
            }

            if (e.Key == Keys.Escape)
            {
                Text = _originalText;
                OnCancelled?.Invoke();
                return;
            }

            if (!char.IsControl(e.Character))
            {
                Text = Text.Insert(_cursorPosition, e.Character.ToString());
                _cursorPosition++;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_blinkTimer >= 0.5)
            {
                _showCursor = !_showCursor;
                _blinkTimer = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel)
        {
            Color backgroundColor = IsActive ? Color.DarkSlateGray : Color.Black;
            Color borderColor = IsActive ? Color.White : Color.Gray;

            spriteBatch.Draw(pixel, Bounds, backgroundColor);

            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - 2, Bounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, 2, Bounds.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - 2, Bounds.Y, 2, Bounds.Height), borderColor);

            Vector2 textPosition = new Vector2(Bounds.X + 6, Bounds.Y + 6);
            spriteBatch.DrawString(font, Text, textPosition, Color.White);

            if (IsActive && _showCursor)
            {
                string beforeCursor = Text.Substring(0, _cursorPosition);
                float cursorX = textPosition.X + font.MeasureString(beforeCursor).X;

                Rectangle cursorRect = new Rectangle(
                    (int)cursorX,
                    Bounds.Y + 5,
                    2,
                    Bounds.Height - 10
                );

                spriteBatch.Draw(pixel, cursorRect, Color.White);
            }
        }

        public bool ContainsPoint(Point point)
        {
            return Bounds.Contains(point);
        }

        public void SetText(string text)
        {
            Text = text ?? "";
            _cursorPosition = Text.Length;
        }
    }
}