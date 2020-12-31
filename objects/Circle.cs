using Godot;
using System;

public class Circle : Area2D
{
    [Signal]
    public delegate void FullOrbit();

    public enum MODES { STATIC, LIMITED };

    public float radius = 100;
    public float RotationSpeed = Mathf.Pi;
    MODES Mode = MODES.STATIC;
    private int MoveRange = 0;
    private float MoveSpeed = 2.0F;
    int NumOrbit = 3;
    int CurrentOrbit = 0;
    float OrbitStart;
    public Node2D _OrbitPosition;

    public Jumper _Jumper = null;

    public Tween _MoveTween;

    public Sprite _Sprite;
    public Sprite _SpriteEffect;

    public Node2D _Pivot;

    public AnimationPlayer _Player;


    public void init(Vector2 _position, int level = 1)
    {
        MODES _mode = (MODES)settings.RandWeighted(new int[] { 10, level - 1 });
        SetMode(_mode);
        Position = _position;
        var MoveChance = Mathf.Clamp(level - 10, 0, 9) / 10.0;
        if (GD.Randf() < MoveChance)
        {
            MoveRange = Mathf.Max(25, (int)(100 * GD.RandRange(0.75, 1.25) * MoveChance * Mathf.Pow(-1, GD.Randi() % 2)));
            MoveSpeed = Mathf.Max(2.5f - Mathf.Ceil(level / 5) * 0.25f, 0.75f);
        }
        var SmallChance = Mathf.Min(0.9f, Mathf.Max(0f, (level - 10f) / 20.0f));
        if (GD.Randf() < SmallChance)
        {
            radius = (int)Mathf.Max(50, (int)(radius - level * GD.RandRange(0.75, 1.25)));
        }
        _SpriteEffect.Material = _Sprite.Material;

        var CollisionShape2D = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
        CollisionShape2D.Radius = radius - 25;

        var imgSize = _Sprite.Texture.GetSize().x / 2;
        _Sprite.Scale = new Vector2(1, 1) * radius / imgSize;
        _OrbitPosition.Position = new Vector2(
          x: radius,
          y: _OrbitPosition.Position.y
        );
        RotationSpeed *= Mathf.Pow(-1, GD.Randi() % 2);
        SetTween();
    }

    public void SetMode(MODES _mode)
    {
        Color _Color;
        Mode = _mode;
        switch (Mode)
        {
            case MODES.STATIC:
                GetNode<Label>("Label").Hide();
                _Color = settings.Theme["circle_static"];
                break;
            case MODES.LIMITED:
                CurrentOrbit = NumOrbit;
                GetNode<Label>("Label").Text = NumOrbit.ToString();
                GetNode<Label>("Label").Show();
                _Color = settings.Theme["circle_limited"];
                break;
            default:
                _Color = settings.Theme["circle_static"];
                break;
        }
        // 형광효과 제거
        // (_Sprite.Material as ShaderMaterial).SetShaderParam("color", _Color);
    }

    public override void _Process(float delta)
    {
        _Pivot.Rotation += RotationSpeed * delta;
        if (_Jumper != null)
        {
            CheckOrbits();
            Update();
        }
    }

    void CheckOrbits()
    {
        if (Mathf.Abs(_Pivot.Rotation - OrbitStart) > 2 * Mathf.Pi)
        {
            CurrentOrbit += 1;
            EmitSignal("FullOrbit");
            if (Mode == MODES.LIMITED)
            {
                if (settings.EnableSound == true)
                {
                    GetNode<AudioStreamPlayer>("Beep").Play();
                }
                GetNode<Label>("Label").Text = (NumOrbit - CurrentOrbit).ToString();
                if (CurrentOrbit >= NumOrbit)
                {
                    _Jumper.die();
                    _Jumper = null;
                    Implode();
                }
            }
            OrbitStart = _Pivot.Rotation;
        }
    }

    public async void Implode()
    {
        _Jumper = null;
        _Player.Play("implode");
        await ToSignal(_Player, "animation_finished");
        QueueFree();
    }

    public void Capture(Jumper target)
    {
        CurrentOrbit = 0;
        _Jumper = target;
        _Player.Play("capture");
        _Pivot.Rotation = (_Jumper.Position - Position).Angle();
        OrbitStart = _Pivot.Rotation;
    }

    public override void _Ready()
    {
        _OrbitPosition = GetNode<Position2D>("Pivot/OrbitPosition");
        _MoveTween = GetNode<Tween>("MoveTween");
        _Sprite = GetNode<Sprite>("Sprite");
        _SpriteEffect = GetNode<Sprite>("SpriteEffect");
        _Pivot = GetNode<Node2D>("Pivot");
        _Player = GetNode<AnimationPlayer>("AnimationPlayer");
    }


    public void SetTween(Godot.Object obj = null, string key = null)
    {
        if (MoveRange == 0)
        {
            return;
        }

        MoveRange *= -1;
        _MoveTween.InterpolateProperty(this, "position:x", Position.x, Position.x + MoveRange,
        MoveSpeed, Tween.TransitionType.Quad, Tween.EaseType.InOut);
        _MoveTween.Start();
    }

    public override void _Draw()
    {
        if (Mode != MODES.LIMITED)
        {
            return;
        }
        if (_Jumper != null)
        {
            var r = ((radius - 50) / NumOrbit) * (1 + NumOrbit - CurrentOrbit);
            DrawCircleArcPoly(new Vector2(0, 0), r, OrbitStart + Mathf.Pi / 2, _Pivot.Rotation + Mathf.Pi / 2, new Color(1, 0, 0));
        }
    }

    public void DrawCircleArcPoly(Vector2 center, float radius, float angleFrom, float angleTo, Color color)
    {
        int nbPoints = 32;
        var pointsArc = new Vector2[nbPoints + 1];
        pointsArc[0] = center;
        var colors = new Color[] { color };

        for (int i = 0; i < nbPoints; ++i)
        {
            float anglePoint = Mathf.Deg2Rad(angleFrom + i * (angleTo - angleFrom) / nbPoints - 90);
            pointsArc[i + 1] = center + new Vector2(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint)) * radius;
        }

        DrawPolygon(pointsArc, colors);
    }
}
