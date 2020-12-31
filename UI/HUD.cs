using Godot;
using System;

public class HUD : CanvasLayer
{
    int _score = 0;
    Label _Message;
    MarginContainer _ScoreBox;
    Label _Score;
    MarginContainer _BonusBox;
    Label _StartTip;

    Tween _Tween; 
    public override void _Ready()
    {
       _Message = GetNode<Label>("Message");
       _Message.RectPivotOffset = _Message.RectSize / 2;

       _ScoreBox = GetNode<MarginContainer>("ScoreBox");
       _Score = GetNode<Label>("ScoreBox/HBoxContainer/Score");
       _BonusBox = GetNode<MarginContainer>("BonusBox");
       _StartTip = GetNode<Label>("StartTip");

        _Tween = GetNode<Tween>("Tween");
    }

    public void ShowMessage(string _Text) {
        _Message.Text = _Text;
        GetNode<AnimationPlayer>("MessageAnimation").Play("show_message");
    }

    public void Hide() {
        _ScoreBox.Hide();
        _BonusBox.Hide();
        _StartTip.Hide();
    }
    public void Show() {
        _ScoreBox.Show();
        _BonusBox.Show();
        _StartTip.Show();
    }
    
    public void UpdateScore(int Score, int Value) {
        if (Value > 0 && _StartTip.Visible)
            _StartTip.Hide();
        _Tween.InterpolateProperty(this, "_score", Score, Value, 0.25f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        _Tween.Start();
        GetNode<AnimationPlayer>("ScoreAnimation").Play("score");
        
    }

    public void UpdateBonus(int Value) {
        GetNode<Label>("BonusBox/Bonus").Text = $"{Value} x";
        if (Value > 1) {
            GetNode<AnimationPlayer>("BonusAnimation").Play("bonus");
        }
    }

    public void OnTweenTweenStep(Godot.Object obj, NodePath key, float elapsed, int value) {
        _Score.Text = value.ToString(); 
    }
}
