using Engine.Core;
using Engine.GameObjects;
using Engine.Serialization;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Engine.Editor;

public class EditorOverlay
{
    private sealed class InspectorFieldBinding
    {
        public required string Key { get; init; }
        public required string Label { get; set; }
        public required TextInputField Field { get; init; }
        public Action<string> Confirm { get; set; } = _ => { };
        public Action Cancel { get; set; } = () => { };
    }

    private readonly EngineGame _game;
    private readonly Dictionary<string, InspectorFieldBinding> _fieldBindings = new();
    private readonly List<InspectorFieldBinding> _visibleInspectorFields = new();
    private readonly List<(GameObject GameObject, Rectangle Bounds)> _hierarchyRows = new();
    private readonly List<(Component Component, Rectangle Bounds)> _componentRemoveButtons = new();
    private readonly List<(Type Type, Rectangle Bounds)> _componentDropdownRows = new();

    private bool _isVisible;
    private GameObject? _selectedObject;
    private bool _editorHasFocus;
    private bool _showComponentDropdown;
    private SpriteFont? _font;
    private Texture2D _pixel;
    private List<TextInputField> _fields = new();
    private bool _isDraggingSelectedObject;
    private string? _playModeBackupJson;
    private bool _isPlaying;

    private Rectangle _toolbarRect;
    private Rectangle _hierarchyRect;
    private Rectangle _inspectorRect;
    private Rectangle _viewportRect;
    private Rectangle _saveButtonRect;
    private Rectangle _loadButtonRect;
    private Rectangle _playButtonRect;
    private Rectangle _addObjectButtonRect;
    private Rectangle _removeObjectButtonRect;
    private Rectangle _addComponentButtonRect;

    public bool HasFocus => _isVisible && _editorHasFocus;
    public bool IsVisible => _isVisible;

    public EditorOverlay(EngineGame game)
    {
        _game = game;
        _pixel = new Texture2D(game.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void LoadContent(SpriteFont font)
    {
        _font = font;
    }

    public void Update(GameTime gameTime)
    {
        if (_game.Input.IsKeyPressed(Keys.F2))
            Toggle();

        if (!_isVisible)
        {
            _editorHasFocus = false;
            _isDraggingSelectedObject = false;
            return;
        }

        ValidateSelection();
        RebuildLayout();

        foreach (var field in _fields)
            field.Update(gameTime);

        var mousePoint = _game.Input.MousePosition.ToPoint();
        var mouseWorld = _game.Camera.ScreenToWorld(_game.Input.MousePosition);
        var leftPressed = _game.Input.IsLeftMousePressed();
        var leftHeld = _game.Input.IsLeftMouseHeld();
        var leftReleased = _game.Input.IsLeftMouseReleased();

        _editorHasFocus = AnyFieldActive() || _showComponentDropdown || _isDraggingSelectedObject;

        if (leftPressed)
        {
            if (HandleToolbarClick(mousePoint) ||
                HandleHierarchyClick(mousePoint) ||
                HandleInspectorClick(mousePoint))
            {
                _editorHasFocus = true;
            }
            else
            {
                DeactivateAllFields();
                _showComponentDropdown = false;

                if (_viewportRect.Contains(mousePoint))
                {
                    HandleViewportClick(mouseWorld);
                    _editorHasFocus = true;
                }
            }
        }

        if (_isDraggingSelectedObject && leftHeld && _selectedObject != null)
        {
            _selectedObject.Transform.Position = mouseWorld;
            _editorHasFocus = true;
        }

        if (leftReleased)
            _isDraggingSelectedObject = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!_isVisible || _font == null)
            return;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawPanel(spriteBatch, _toolbarRect, new Color(24, 24, 28, 235));
        DrawPanel(spriteBatch, _hierarchyRect, new Color(32, 32, 38, 220));
        DrawPanel(spriteBatch, _inspectorRect, new Color(32, 32, 38, 220));
        DrawBorder(spriteBatch, _viewportRect, new Color(200, 200, 210, 80));

        DrawToolbar(spriteBatch);
        DrawHierarchy(spriteBatch);
        DrawInspector(spriteBatch);
        DrawSelectedOutline(spriteBatch);

        spriteBatch.End();
    }

    private void Toggle()
    {
        _isVisible = !_isVisible;

        if (_isVisible)
            return;

        DeactivateAllFields();
        _showComponentDropdown = false;
        _isDraggingSelectedObject = false;
        _editorHasFocus = false;
    }

    private void ValidateSelection()
    {
        var scene = _game.Scenes.ActiveScene;
        if (scene == null || _selectedObject == null)
        {
            if (scene == null)
            {
                _selectedObject = null;
                DeactivateAllFields();
                _showComponentDropdown = false;
            }

            return;
        }

        if (!scene.GameObjects.Contains(_selectedObject))
        {
            _selectedObject = null;
            DeactivateAllFields();
            _showComponentDropdown = false;
        }
    }

    private void RebuildLayout()
    {
        var toolbarHeight = 32;
        var leftWidth = Math.Max(180, (int)(_game.ScreenWidth * 0.15f));
        var rightWidth = Math.Max(250, (int)(_game.ScreenWidth * 0.20f));
        var bodyHeight = Math.Max(0, _game.ScreenHeight - toolbarHeight);

        _toolbarRect = new Rectangle(0, 0, _game.ScreenWidth, toolbarHeight);
        _hierarchyRect = new Rectangle(0, toolbarHeight, leftWidth, bodyHeight);
        _inspectorRect = new Rectangle(_game.ScreenWidth - rightWidth, toolbarHeight, rightWidth, bodyHeight);
        _viewportRect = new Rectangle(leftWidth, toolbarHeight, Math.Max(1, _game.ScreenWidth - leftWidth - rightWidth), bodyHeight);

        BuildToolbarLayout();
        BuildHierarchyLayout();
        BuildInspectorLayout();
    }

    private void BuildToolbarLayout()
    {
        var buttonY = _toolbarRect.Y + 4;
        _saveButtonRect = new Rectangle(_toolbarRect.Right - 270, buttonY, 72, 24);
        _loadButtonRect = new Rectangle(_toolbarRect.Right - 190, buttonY, 72, 24);
        _playButtonRect = new Rectangle(_toolbarRect.Right - 110, buttonY, 98, 24);
    }

    private void BuildHierarchyLayout()
    {
        _hierarchyRows.Clear();

        var scene = _game.Scenes.ActiveScene;
        var rowY = _hierarchyRect.Y + 36;
        var rowHeight = 24;

        _addObjectButtonRect = new Rectangle(_hierarchyRect.X + 8, _hierarchyRect.Bottom - 68, _hierarchyRect.Width - 16, 26);
        _removeObjectButtonRect = new Rectangle(_hierarchyRect.X + 8, _hierarchyRect.Bottom - 36, _hierarchyRect.Width - 16, 26);

        if (scene == null)
            return;

        foreach (var gameObject in scene.GameObjects)
        {
            if (rowY + rowHeight > _addObjectButtonRect.Y - 6)
                break;

            _hierarchyRows.Add((
                gameObject,
                new Rectangle(_hierarchyRect.X + 8, rowY, _hierarchyRect.Width - 16, rowHeight)));
            rowY += rowHeight + 4;
        }
    }

    private void BuildInspectorLayout()
    {
        _componentRemoveButtons.Clear();
        _componentDropdownRows.Clear();
        _visibleInspectorFields.Clear();
        _fields.Clear();

        var visibleKeys = new HashSet<string>();
        if (_selectedObject == null)
        {
            RemoveStaleFields(visibleKeys);
            return;
        }

        var objectId = RuntimeHelpers.GetHashCode(_selectedObject);
        var rowY = _inspectorRect.Y + 42;
        BindStringField($"object:{objectId}:name", "Name", _selectedObject.Name, value => _selectedObject.Name = value, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:position:x", "Position X", _selectedObject.Transform.Position.X, value =>
        {
            _selectedObject.Transform.Position = new Vector2(value, _selectedObject.Transform.Position.Y);
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:position:y", "Position Y", _selectedObject.Transform.Position.Y, value =>
        {
            _selectedObject.Transform.Position = new Vector2(_selectedObject.Transform.Position.X, value);
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:rotation", "Rotation", _selectedObject.Transform.Rotation, value =>
        {
            _selectedObject.Transform.Rotation = value;
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:scale:x", "Scale X", _selectedObject.Transform.Scale.X, value =>
        {
            _selectedObject.Transform.Scale = new Vector2(value, _selectedObject.Transform.Scale.Y);
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:scale:y", "Scale Y", _selectedObject.Transform.Scale.Y, value =>
        {
            _selectedObject.Transform.Scale = new Vector2(_selectedObject.Transform.Scale.X, value);
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:size:x", "Size X", _selectedObject.Transform.Size.X, value =>
        {
            _selectedObject.Transform.Size = new Vector2(value, _selectedObject.Transform.Size.Y);
        }, visibleKeys, ref rowY);
        BindFloatField($"object:{objectId}:transform:size:y", "Size Y", _selectedObject.Transform.Size.Y, value =>
        {
            _selectedObject.Transform.Size = new Vector2(_selectedObject.Transform.Size.X, value);
        }, visibleKeys, ref rowY);

        rowY += 10;

        foreach (var component in _selectedObject.Components)
        {
            rowY += 4;
            _componentRemoveButtons.Add((
                component,
                new Rectangle(_inspectorRect.Right - 76, rowY, 64, 22)));
            rowY += 28;

            foreach (var property in component.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttribute<EditorVisibleAttribute>() != null)
                .OrderBy(property => property.Name))
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                var propertyKeyPrefix = $"component:{RuntimeHelpers.GetHashCode(component)}:{property.Name}";
                if (property.PropertyType == typeof(float))
                {
                    BindFloatField(propertyKeyPrefix, property.Name, (float)(property.GetValue(component) ?? 0f), value =>
                    {
                        property.SetValue(component, value);
                    }, visibleKeys, ref rowY);
                }
                else if (property.PropertyType == typeof(int))
                {
                    BindIntField(propertyKeyPrefix, property.Name, (int)(property.GetValue(component) ?? 0), value =>
                    {
                        property.SetValue(component, value);
                    }, visibleKeys, ref rowY);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    BindBoolField(propertyKeyPrefix, property.Name, (bool)(property.GetValue(component) ?? false), value =>
                    {
                        property.SetValue(component, value);
                    }, visibleKeys, ref rowY);
                }
                else if (property.PropertyType == typeof(string))
                {
                    BindStringField(propertyKeyPrefix, property.Name, (string?)property.GetValue(component) ?? string.Empty, value =>
                    {
                        property.SetValue(component, value);
                    }, visibleKeys, ref rowY);
                }
                else if (property.PropertyType == typeof(Vector2))
                {
                    var vector = (Vector2)(property.GetValue(component) ?? Vector2.Zero);
                    BindVector2Field(propertyKeyPrefix, property.Name, vector, value =>
                    {
                        property.SetValue(component, value);
                    }, visibleKeys, ref rowY);
                }
            }
        }

        _addComponentButtonRect = new Rectangle(_inspectorRect.X + 10, rowY + 8, _inspectorRect.Width - 20, 26);

        if (_showComponentDropdown)
        {
            var dropdownY = _addComponentButtonRect.Bottom + 4;
            foreach (var componentType in ComponentRegistry.AvailableComponents)
            {
                var rowRect = new Rectangle(_addComponentButtonRect.X, dropdownY, _addComponentButtonRect.Width, 24);
                _componentDropdownRows.Add((componentType, rowRect));
                dropdownY += 24;

                if (dropdownY > _inspectorRect.Bottom - 8)
                    break;
            }
        }

        RemoveStaleFields(visibleKeys);
    }

    private void RemoveStaleFields(HashSet<string> visibleKeys)
    {
        var staleKeys = _fieldBindings.Keys
            .Where(key => !visibleKeys.Contains(key))
            .ToList();

        foreach (var key in staleKeys)
        {
            var binding = _fieldBindings[key];
            if (binding.Field.IsActive)
                binding.Field.Deactivate(_game.Window);

            _fieldBindings.Remove(key);
        }
    }

    private void BindFloatField(string key, string label, float value, Action<float> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        BindTextField(
            key,
            label,
            value.ToString(CultureInfo.InvariantCulture),
            text =>
            {
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                    setter(parsed);
            },
            visibleKeys,
            ref rowY);
    }

    private void BindIntField(string key, string label, int value, Action<int> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        BindTextField(
            key,
            label,
            value.ToString(CultureInfo.InvariantCulture),
            text =>
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    setter(parsed);
            },
            visibleKeys,
            ref rowY);
    }

    private void BindBoolField(string key, string label, bool value, Action<bool> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        BindTextField(
            key,
            label,
            value.ToString(),
            text =>
            {
                if (bool.TryParse(text, out var parsed))
                    setter(parsed);
            },
            visibleKeys,
            ref rowY);
    }

    private void BindStringField(string key, string label, string value, Action<string> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        BindTextField(key, label, value, setter, visibleKeys, ref rowY);
    }

    private void BindVector2Field(string keyPrefix, string label, Vector2 value, Action<Vector2> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        var labelRect = new Rectangle(_inspectorRect.X + 10, rowY + 4, _inspectorRect.Width - 20, 18);
        rowY += 18;

        var fieldWidth = (_inspectorRect.Width - 36) / 2;
        var leftRect = new Rectangle(_inspectorRect.X + 10, rowY, fieldWidth, 24);
        var rightRect = new Rectangle(_inspectorRect.X + 18 + fieldWidth, rowY, fieldWidth, 24);

        var leftKey = $"{keyPrefix}:x";
        var rightKey = $"{keyPrefix}:y";

        InspectorFieldBinding? leftBinding = null;
        InspectorFieldBinding? rightBinding = null;

        leftBinding = BindField(
            leftKey,
            $"{label} X",
            leftRect,
            value.X.ToString(CultureInfo.InvariantCulture),
            visibleKeys,
            text =>
            {
                if (leftBinding == null || rightBinding == null)
                    return;

                if (TryParseVectorFields(leftBinding.Field.Text, rightBinding.Field.Text, out var parsed))
                    setter(parsed);
            },
            () => { });

        rightBinding = BindField(
            rightKey,
            $"{label} Y",
            rightRect,
            value.Y.ToString(CultureInfo.InvariantCulture),
            visibleKeys,
            text =>
            {
                if (leftBinding == null || rightBinding == null)
                    return;

                if (TryParseVectorFields(leftBinding.Field.Text, rightBinding.Field.Text, out var parsed))
                    setter(parsed);
            },
            () => { });

        rowY += 30;
    }

    private void BindTextField(string key, string label, string value, Action<string> setter, HashSet<string> visibleKeys, ref int rowY)
    {
        var fieldRect = new Rectangle(_inspectorRect.X + 108, rowY, _inspectorRect.Width - 118, 24);
        BindField(key, label, fieldRect, value, visibleKeys, setter, () => { });
        rowY += 30;
    }

    private InspectorFieldBinding BindField(
        string key,
        string label,
        Rectangle bounds,
        string value,
        HashSet<string> visibleKeys,
        Action<string> onConfirm,
        Action onCancel)
    {
        visibleKeys.Add(key);

        if (!_fieldBindings.TryGetValue(key, out var binding))
        {
            binding = new InspectorFieldBinding
            {
                Key = key,
                Label = label,
                Field = new TextInputField(bounds, value)
            };
            binding.Field.OnConfirmed += text => binding.Confirm(text);
            binding.Field.OnCancelled += () => binding.Cancel();
            _fieldBindings.Add(key, binding);
        }

        binding.Label = label;
        binding.Confirm = onConfirm;
        binding.Cancel = onCancel;
        binding.Field.Bounds = bounds;

        if (!binding.Field.IsActive)
            binding.Field.SetText(value);

        _visibleInspectorFields.Add(binding);
        _fields.Add(binding.Field);
        return binding;
    }

    private bool HandleToolbarClick(Point mousePoint)
    {
        if (!_toolbarRect.Contains(mousePoint))
            return false;

        var scene = _game.Scenes.ActiveScene;
        if (scene != null)
        {
            if (_saveButtonRect.Contains(mousePoint))
                SceneSerializer.Save(scene, GetSceneFilePath(scene));
            else if (_loadButtonRect.Contains(mousePoint))
            {
                SceneSerializer.Load(GetSceneFilePath(scene), scene);
                ResetEditorStateAfterSceneReload();
            }
            else if (_playButtonRect.Contains(mousePoint))
            {
                if (!_isPlaying)
                {
                    _playModeBackupJson = SceneSerializer.SaveToString(scene);
                    _isPlaying = true;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_playModeBackupJson))
                        SceneSerializer.LoadFromString(_playModeBackupJson, scene);

                    _isPlaying = false;
                    ResetEditorStateAfterSceneReload();
                }
            }
        }

        return true;
    }

    private bool HandleHierarchyClick(Point mousePoint)
    {
        if (!_hierarchyRect.Contains(mousePoint))
            return false;

        var scene = _game.Scenes.ActiveScene;
        if (scene == null)
            return true;

        if (_addObjectButtonRect.Contains(mousePoint))
        {
            DeactivateAllFields();
            var gameObject = new GameObject("New Object");
            gameObject.Transform.Position = Vector2.Zero;
            gameObject.Transform.Size = new Vector2(40f, 40f);
            scene.AddGameObject(gameObject);
            _selectedObject = gameObject;
            _showComponentDropdown = false;
            return true;
        }

        if (_removeObjectButtonRect.Contains(mousePoint) && _selectedObject != null)
        {
            scene.RemoveGameObject(_selectedObject);
            _selectedObject = null;
            DeactivateAllFields();
            _showComponentDropdown = false;
            return true;
        }

        foreach (var row in _hierarchyRows)
        {
            if (!row.Bounds.Contains(mousePoint))
                continue;

            DeactivateAllFields();
            _selectedObject = row.GameObject;
            _showComponentDropdown = false;
            return true;
        }

        return true;
    }

    private bool HandleInspectorClick(Point mousePoint)
    {
        if (!_inspectorRect.Contains(mousePoint))
            return false;

        foreach (var binding in _visibleInspectorFields)
        {
            if (!binding.Field.ContainsPoint(mousePoint))
                continue;

            ActivateField(binding.Field);
            _showComponentDropdown = false;
            return true;
        }

        foreach (var button in _componentRemoveButtons)
        {
            if (!button.Bounds.Contains(mousePoint) || _selectedObject == null)
                continue;

            _selectedObject.RemoveComponent(button.Component);
            _showComponentDropdown = false;
            return true;
        }

        if (_selectedObject != null && _addComponentButtonRect.Contains(mousePoint))
        {
            _showComponentDropdown = !_showComponentDropdown;
            return true;
        }

        if (_showComponentDropdown)
        {
            foreach (var row in _componentDropdownRows)
            {
                if (!row.Bounds.Contains(mousePoint) || _selectedObject == null)
                    continue;

                if (Activator.CreateInstance(row.Type) is Component component)
                    _selectedObject.AddComponent(component);

                _showComponentDropdown = false;
                return true;
            }
        }

        DeactivateAllFields();
        return true;
    }

    private void HandleViewportClick(Vector2 mouseWorld)
    {
        var scene = _game.Scenes.ActiveScene;
        if (scene == null)
            return;

        GameObject? hitObject = null;
        var closestDistance = float.MaxValue;

        foreach (var gameObject in scene.GameObjects)
        {
            var position = gameObject.Transform.Position;
            var size = gameObject.Transform.Size;
            var left = position.X - size.X / 2f;
            var right = position.X + size.X / 2f;
            var top = position.Y - size.Y / 2f;
            var bottom = position.Y + size.Y / 2f;

            if (mouseWorld.X < left || mouseWorld.X > right || mouseWorld.Y < top || mouseWorld.Y > bottom)
                continue;

            var distance = Vector2.DistanceSquared(mouseWorld, position);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            hitObject = gameObject;
        }

        _selectedObject = hitObject;
        DeactivateAllFields();
        _showComponentDropdown = false;
        _isDraggingSelectedObject = hitObject != null;
    }

    private void DrawToolbar(SpriteBatch spriteBatch)
    {
        DrawText(spriteBatch, "[F2] Editor", new Vector2(10, 8), Color.White);
        DrawText(spriteBatch, "[F1] Debug", new Vector2(110, 8), Color.LightGray);

        DrawButton(spriteBatch, _saveButtonRect, "Save", new Color(60, 72, 88));
        DrawButton(spriteBatch, _loadButtonRect, "Load", new Color(60, 72, 88));
        DrawButton(spriteBatch, _playButtonRect, _isPlaying ? "Stop" : "Play", _isPlaying ? new Color(140, 55, 55) : new Color(55, 100, 60));
    }

    private void DrawHierarchy(SpriteBatch spriteBatch)
    {
        DrawText(spriteBatch, "Hierarchy", new Vector2(_hierarchyRect.X + 10, _hierarchyRect.Y + 10), Color.White);

        foreach (var row in _hierarchyRows)
        {
            var rowColor = row.GameObject == _selectedObject
                ? new Color(70, 95, 130)
                : new Color(44, 44, 50);

            spriteBatch.Draw(_pixel, row.Bounds, rowColor);
            DrawBorder(spriteBatch, row.Bounds, Color.Black);
            DrawText(spriteBatch, row.GameObject.Name, new Vector2(row.Bounds.X + 6, row.Bounds.Y + 4), Color.White);
        }

        DrawButton(spriteBatch, _addObjectButtonRect, "+ Add Object", new Color(55, 90, 60));
        DrawButton(spriteBatch, _removeObjectButtonRect, "Remove Object", new Color(110, 55, 55));
    }

    private void DrawInspector(SpriteBatch spriteBatch)
    {
        DrawText(spriteBatch, "Inspector", new Vector2(_inspectorRect.X + 10, _inspectorRect.Y + 10), Color.White);

        if (_selectedObject == null)
        {
            DrawText(spriteBatch, "No object selected", new Vector2(_inspectorRect.X + 10, _inspectorRect.Y + 42), Color.LightGray);
            return;
        }

        foreach (var binding in _visibleInspectorFields)
        {
            if (binding.Key.StartsWith("component:") && binding.Key.EndsWith(":x"))
            {
                var yLabel = binding.Label[..^2];
                DrawText(spriteBatch, yLabel, new Vector2(_inspectorRect.X + 10, binding.Field.Bounds.Y - 18), Color.LightGray);
            }
            else if (!binding.Key.StartsWith("component:") || !binding.Key.EndsWith(":y"))
            {
                DrawText(spriteBatch, binding.Label, new Vector2(_inspectorRect.X + 10, binding.Field.Bounds.Y + 4), Color.LightGray);
            }

            binding.Field.Draw(spriteBatch, _font!, _pixel);
        }

        var headerY = _inspectorRect.Y + 262;
        foreach (var button in _componentRemoveButtons)
        {
            headerY = Math.Max(headerY, button.Bounds.Y - 4);
            DrawText(spriteBatch, button.Component.GetType().Name, new Vector2(_inspectorRect.X + 10, button.Bounds.Y + 2), Color.White);
            DrawButton(spriteBatch, button.Bounds, "Remove", new Color(110, 55, 55));
        }

        DrawButton(spriteBatch, _addComponentButtonRect, "+ Add Component", new Color(55, 90, 60));

        if (_showComponentDropdown)
        {
            foreach (var row in _componentDropdownRows)
            {
                spriteBatch.Draw(_pixel, row.Bounds, new Color(46, 46, 54));
                DrawBorder(spriteBatch, row.Bounds, Color.Black);
                DrawText(spriteBatch, row.Type.Name, new Vector2(row.Bounds.X + 6, row.Bounds.Y + 4), Color.White);
            }
        }
    }

    private void DrawSelectedOutline(SpriteBatch spriteBatch)
    {
        if (_selectedObject == null)
            return;

        var center = _game.Camera.WorldToScreen(_selectedObject.Transform.Position);
        var size = _selectedObject.Transform.Size * _game.Camera.Zoom;
        var rect = new Rectangle(
            (int)(center.X - size.X / 2f),
            (int)(center.Y - size.Y / 2f),
            Math.Max(1, (int)size.X),
            Math.Max(1, (int)size.Y));

        DrawBorder(spriteBatch, rect, Color.Yellow);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string label, Color color)
    {
        spriteBatch.Draw(_pixel, rect, color);
        DrawBorder(spriteBatch, rect, Color.Black);

        if (_font == null)
            return;

        var size = _font.MeasureString(label);
        var position = new Vector2(
            rect.X + (rect.Width - size.X) / 2f,
            rect.Y + (rect.Height - size.Y) / 2f);
        DrawText(spriteBatch, label, position, Color.White);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, rect, color);
        DrawBorder(spriteBatch, rect, Color.Black);
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
    }

    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        if (_font == null)
            return;

        spriteBatch.DrawString(_font, text, position, color);
    }

    private void ActivateField(TextInputField targetField)
    {
        foreach (var field in _fieldBindings.Values.Select(binding => binding.Field))
        {
            if (field == targetField)
                continue;

            if (field.IsActive)
                field.Deactivate(_game.Window);
        }

        targetField.Activate(_game.Window);
    }

    private void DeactivateAllFields()
    {
        foreach (var field in _fieldBindings.Values.Select(binding => binding.Field))
        {
            if (field.IsActive)
                field.Deactivate(_game.Window);
        }
    }

    private bool AnyFieldActive()
    {
        return _fieldBindings.Values.Any(binding => binding.Field.IsActive);
    }

    private void ResetEditorStateAfterSceneReload()
    {
        _selectedObject = null;
        _showComponentDropdown = false;
        _isDraggingSelectedObject = false;
        DeactivateAllFields();
    }

    private static bool TryParseVectorFields(string xText, string yText, out Vector2 value)
    {
        if (float.TryParse(xText, NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(yText, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            value = new Vector2(x, y);
            return true;
        }

        value = Vector2.Zero;
        return false;
    }

    private static string GetSceneFilePath(Scene scene)
    {
        return Path.Combine("SceneData", $"{scene.GetType().Name}.json");
    }
}
