using Godot;
using System;

public class Screens : Node
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  [Signal]
  public delegate void StartGame();


  [Signal]
  public delegate void a();

  public BaseScreen CurrentScreen = null;

  public static Godot.Collections.Dictionary SoundButtons = new Godot.Collections.Dictionary();
  public static Godot.Collections.Dictionary MusicButtons = new Godot.Collections.Dictionary();

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    SoundButtons.Add(true, ResourceLoader.Load("res://assets/images/buttons/audioOn.png") as Texture);
    SoundButtons.Add(false, ResourceLoader.Load("res://assets/images/buttons/audioOff.png") as Texture);
    MusicButtons.Add(true, ResourceLoader.Load("res://assets/images/buttons/musicOn.png") as Texture);
    MusicButtons.Add(false, ResourceLoader.Load("res://assets/images/buttons/musicOff.png") as Texture);
       
    RegisterButtons();
    ChangeScreen(GetNode<BaseScreen>("TitleScreen"));
  }

  public void RegisterButtons()
  {
    var Buttons = GetTree().GetNodesInGroup("Buttons");
    GD.Print(Buttons);
    foreach (var Button in Buttons)
    {
      (Button as Node).Connect("pressed", this, "OnButtonPressed", new Godot.Collections.Array(){Button});
      switch ((Button as Node).Name) {
        case "Ads":
        {
        break;
        }
        case "Sound": {
          (Button as TextureButton).TextureNormal = (Texture)SoundButtons[settings.EnableSound];
          break;
        }
        case "Music": {
          (Button as TextureButton).TextureNormal = (Texture)MusicButtons[settings.EnableMusic];
          break;
        }
      }
    }
  }

  public async void OnButtonPressed(Node button)
  {
    if (settings.EnableSound == true) {
      GetNode<AudioStreamPlayer>("Click").Play();
    }

    switch (button.Name)
    {
      case "About":
        ChangeScreen(GetNode<BaseScreen>("AboutScreen"));
        break;
      case "Home":
          ChangeScreen(GetNode<BaseScreen>("TitleScreen"));
          break;
      case "Play":
          ChangeScreen(null);
          await ToSignal(GetTree().CreateTimer((float)0.5), "timeout");
          EmitSignal("StartGame");
          break;
      case "Settings":
          ChangeScreen(GetNode<BaseScreen>("SettingScreen"));
          break;
      case "Sound":
        settings.EnableSound = !settings.EnableSound;
        var b = (TextureButton)button;
        b.TextureNormal = (Texture)SoundButtons[settings.EnableSound];
        settings.SaveSettings();
        break;

      case "Music":
        settings.EnableMusic = !settings.EnableMusic;
        var c = (TextureButton)button;
        c.TextureNormal = (Texture)MusicButtons[settings.EnableMusic];
        settings.SaveSettings();
        break;
      default:
      {
        GD.Print(button.Name);
        break;
      }
    }

  }

  public async void ChangeScreen(BaseScreen NewScreen)
  {
    if (CurrentScreen != null)
    {
      CurrentScreen.Disapear();
      await ToSignal(CurrentScreen._tween, "tween_completed");
    }
    CurrentScreen = NewScreen;
    if (CurrentScreen != null)
    {
      CurrentScreen.Appear();
      await ToSignal(CurrentScreen._tween, "tween_completed");
    }
  }

  public void GameOver(int Score, int HighScore)
  {
    var ScoreBox = GetNode("GameOverScreen/MarginContainer/VBoxContainer/Scores");
    ScoreBox.GetNode<Label>("Score").Text = $"Score: {Score}";
    ScoreBox.GetNode<Label>("Best").Text = $"Best: {HighScore}";
    ChangeScreen(GetNode<BaseScreen>("GameOverScreen"));
  }
}
