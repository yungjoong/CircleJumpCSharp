using Godot;
using System;

public class Jumper : Area2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Signal]
    public delegate void Captured();
    [Signal]
    public delegate void Died();
    public Vector2 velocity = new Vector2(100, 0);
    public Circle target = null;

    public Sprite _Sprite;

    public Line2D _trail;
    int _TrailLength = 25;

    private int _JumpSpeed = 1500;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // spriteMaterial 삭제하였음
        // _Sprite = GetNode<Sprite>("Sprite");
        // (_Sprite.Material as ShaderMaterial).SetShaderParam("color", settings.Theme["player_body"]);
        _trail = GetNode<Line2D>("Trail/Points");
        Color TrailColor = settings.Theme["player_trail"];
        _trail.Gradient.SetColor(1, TrailColor);
        TrailColor.a = 0;
        _trail.Gradient.SetColor(0, TrailColor);
    }

    public override void _UnhandledInput(InputEvent _event)
    {
        if (target != null && _event is InputEventScreenTouch eventKey)
        {
            if (eventKey.Pressed)
            {
                jump();
            }
        }
    }

    private void jump()
    {
        target.Implode();
        target = null;
        velocity = Transform.x * _JumpSpeed;
        if (settings.EnableSound == true)
        {
            GetNode<AudioStreamPlayer>("Jump").Play();
        }
    }

    public void OnJumperAreaEntered(Circle area)
    {
        target = area;
        // target.GetNode<Node2D>("Pivot").Rotation = (Position - target.Position).Angle();
        velocity = new Vector2(0, 0);
        EmitSignal("Captured", area);
        if (settings.EnableSound == true)
        {
            GetNode<AudioStreamPlayer>("Capture").Play();
        }
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_trail.Points.Length > _TrailLength)
        {
            _trail.RemovePoint(0);
        }
        _trail.AddPoint(Position);

        if (target != null)
        {
            Transform = target._OrbitPosition.GlobalTransform;
        }
        else
        {
            Position += velocity * delta;
        }
    }

    public void die()
    {
        target = null;
        QueueFree();
    }

    public void OnVisibilityNotifier2DScreenExited()
    {
        if (target == null)
        {
            EmitSignal("Died");
            die();
        }
    }
}
