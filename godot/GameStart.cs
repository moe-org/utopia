using System;
using System.Diagnostics;
using System.Threading;
using Godot;
using Utopia.Godot.Utility;

public partial class GameStart : Node
{

    private const string NextScenePath = "res://scenes/Main.tscn";

    [GodotNodeBind("ProgressBar")]
    private ProgressBar _progressBar { get; set; } = null!;

    [GodotNodeBind("AudioStreamPlayer")]
    private AudioStreamPlayer _player { get; set; } = null!;

    [GodotNodeBind("ColorRect")]
    private ColorRect _colorRect { get; set; } = null!;

    // [GodotNodeBind("Container")]
    // private Container _container { get; set; } = null!;

    [GodotNodeBind("TextureRect")]
    private TextureRect _textureRect { get; set; } = null!;

    [GodotResourceBind("res://images/logo/license.png")]
    private CompressedTexture2D _license { get; set; } = null!;

    [GodotResourceBind("res://images/logo/utopia.png")]
    private CompressedTexture2D _logo { get; set; } = null!;

    [GodotResourceBind("res://images/logo/dotnet-logo.png")]
    private CompressedTexture2D _dotnetLogo { get; set; } = null!;

    [GodotResourceBind("res://images/logo/godot-logo.png")]
    private CompressedTexture2D _godotLogo { get; set; } = null!;


    [GodotResourceBind("res://audio/em-theme.ogg")]
    private AudioStream _audioStream { get; set; } = null!;

    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// usage see <see cref="_Process(double)"/>
    /// </summary>
    private int _status = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GodotBinder.BindBoth(this);

        _progressBar.Visible = false;
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = 1;

        _textureRect.TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled;
        _textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

        // set the node size
        _colorRect.AnchorBottom = 0;
        _colorRect.AnchorLeft = 0;
        _colorRect.AnchorRight = 0;
        _colorRect.AnchorTop = 0;

        _textureRect.AnchorLeft = 0.5f;
        _textureRect.AnchorRight = 0.5f;
        _textureRect.AnchorTop = 0.5f;
        _textureRect.AnchorBottom = 0.5f;

        SetSize();
        GetViewport().SizeChanged += SetSize;

        // player the music
        _player.Stream = _audioStream;
        _player.Play();
        _stopwatch.Start();

        // do not forge to load game!
        var err = ResourceLoader.LoadThreadedRequest(NextScenePath);

        // TODO: PROCESS ERROR
    }

    private void SetSize()
    {
        // clear the screen
        var size = GetViewport().GetCamera2D().GetViewportRect().Size;
        _colorRect.Size = size;
        _colorRect.OffsetLeft = -size.X / 2;
        _colorRect.OffsetTop = -size.Y / 2;

        if (_textureRect.Texture == null)
        {
            return;
        }
        // center the image
        var textureSize = _textureRect.Texture.GetSize();

        _textureRect.OffsetLeft = -textureSize.X / 2 * 0.4f;
        _textureRect.OffsetRight = textureSize.X / 2 * 0.4f;
        _textureRect.OffsetTop = -textureSize.Y / 2 * 0.4f;
        _textureRect.OffsetBottom = textureSize.Y / 2 * 0.4f;

        _textureRect.Scale = new(0.4f, 0.4f);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        long ms = _stopwatch.ElapsedMilliseconds;

        // update bar
        var status = ResourceLoader.LoadThreadedGetStatus(NextScenePath);

        if (status == ResourceLoader.ThreadLoadStatus.InProgress)
        {
            _progressBar.Value = 0.5;
        }
        else
        {
            _progressBar.Value = 1.0;
        }

        // switch logo
        if (ms >= 2100 && _status == 0)
        {
            _textureRect.Texture = _license;
            _status = 1;
        }
        else if (ms >= 4400 && _status == 1)
        {
            _textureRect.Texture = _godotLogo;
            _status = 2;
        }
        else if (ms >= 6600 && _status == 2)
        {
            _textureRect.Texture = _dotnetLogo;
            _status = 3;
        }
        else if (ms >= 8740)
        {
            _textureRect.Texture = _logo;
        }

        // after loaded all logos
        // load scene
        if (ms >= 12000)
        {
            if (status != ResourceLoader.ThreadLoadStatus.Loaded)
            {
                // just show the progress bar
                _progressBar.ZIndex = 1;
                _progressBar.Visible = true;
                _textureRect.Texture = _logo;
            }
            else
            {
                var scene = (PackedScene)ResourceLoader.LoadThreadedGet(NextScenePath);

                GetTree().ChangeSceneToPacked(scene);

                return; // do not execute SetSize()
                        // TODO:HANDLE ERRORS
            }
        }
        SetSize();
    }
}
