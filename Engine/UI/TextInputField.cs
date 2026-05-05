using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.UI;

public class TextInputField
{
    private GameWindow? _window;
    private int _cursorPosition;
    private double _blinkTimer;
    private bool _showCursor = true;
    private string _originalText = string.Empty;

    public string Text { get; private set; }
    public bool IsActive { get; private set; }
    public Rectangle Bounds { get; set; }

    public event Action<string>? OnConfirmed;
    public event Action? OnCancelled;

    public TextInputField(Rectangle bounds, string startingText = "")
    {
        Bounds = bounds;
        Text = startingText ?? string.Empty;
        _cursorPosition = Text.Length;
    }

    public void Activate(GameWindow window)
    {
        if (IsActive)
            return;

        _window = window;
        IsActive = true;
        _originalText = Text;
        _cursorPosition = Text.Length;
        _blinkTimer = 0;
        _showCursor = true;
        window.TextInput += HandleTextInput;
    }

    public void Deactivate(GameWindow window)
    {
        if (!IsActive)
            return;

        IsActive = false;
        window.TextInput -= HandleTextInput;

        if (ReferenceEquals(_window, window))
            _window = null;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer < 0.5)
            return;

        _showCursor = !_showCursor;
        _blinkTimer = 0;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel)
    {
        var backgroundColor = IsActive ? new Color(50, 60, 72) : new Color(28, 28, 32);
        var borderColor = IsActive ? Color.White : Color.Gray;
        var textPosition = new Vector2(Bounds.X + 6, Bounds.Y + 6);

        spriteBatch.Draw(pixel, Bounds, backgroundColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - 1, Bounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, 1, Bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - 1, Bounds.Y, 1, Bounds.Height), borderColor);
        spriteBatch.DrawString(font, Text, textPosition, Color.White);

        if (!IsActive || !_showCursor)
            return;

        var beforeCursor = Text[..Math.Min(_cursorPosition, Text.Length)];
        var cursorX = textPosition.X + font.MeasureString(beforeCursor).X;
        var cursorRect = new Rectangle((int)cursorX, Bounds.Y + 5, 2, Math.Max(1, Bounds.Height - 10));
        spriteBatch.Draw(pixel, cursorRect, Color.White);
    }

    public bool ContainsPoint(Point point)
    {
        return Bounds.Contains(point);
    }

    public void SetText(string text)
    {
        Text = text ?? string.Empty;
        _cursorPosition = Text.Length;
    }

    private void HandleTextInput(object? sender, TextInputEventArgs e)
    {
        if (!IsActive)
            return;

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
            _originalText = Text;
            OnConfirmed?.Invoke(Text);
            DeactivateFromEvent();
            return;
        }

        if (e.Key == Keys.Escape)
        {
            Text = _originalText;
            OnCancelled?.Invoke();
            DeactivateFromEvent();
            return;
        }

        if (char.IsControl(e.Character))
            return;

        Text = Text.Insert(_cursorPosition, e.Character.ToString());
        _cursorPosition++;
    }

    private void DeactivateFromEvent()
    {
        if (_window != null)
            Deactivate(_window);
    }
}
