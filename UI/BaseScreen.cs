using Godot;
using System;

public class BaseScreen : CanvasLayer
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";
  public Tween _tween;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _tween = GetNode<Tween>("Tween");
  }

  public void Appear()
  {
    var buttons = GetTree().GetNodesInGroup("Buttons");
    foreach (BaseButton button in buttons)
    {
       button.Disabled = false;
    }
    GetTree().SetGroup("Buttons", "Disabled", true);
    GetTree().SetGroup("Buttons", "SetDisabled", false);
    _tween.InterpolateProperty(this, "offset:x", 500, 0, (float)0.5, Tween.TransitionType.Back, Tween.EaseType.InOut);
    _tween.Start();
  }

  public void Disapear()
  {
    var buttons = GetTree().GetNodesInGroup("Buttons");
    foreach (BaseButton button in buttons)
    {
       button.Disabled = true;
    }
    GetTree().SetGroup("Buttons", "Disabled", true);
    GetTree().CallGroup("Buttons", "SetDisabled", true);
    _tween.InterpolateProperty(this, "offset:x", 0, 500, (float)0.4, Tween.TransitionType.Back, Tween.EaseType.InOut);
    _tween.Start();
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //  public override void _Process(float delta)
  //  {
  //      
  //  }
}
