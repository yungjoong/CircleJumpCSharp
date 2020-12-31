using Godot;
using System;

public class Main : Node2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  // Called when the node enters the scene tree for the first time.

  private Jumper Player;

  private PackedScene _Circle;
  private PackedScene _Jumper;
  private Camera2D _Camera;

  private Position2D _StartPosition;

  private HUD _HUD;

  private AudioStreamPlayer _Music;

  private Godot.Object AdmobPlugin;

  private int _Score;
  private int _NumCircles = 0;
  private int _Bonus;
  public int _Level;
  [Export]
  public int Score {
    get{return _Score;}
    set{
      _Score = value;
      _HUD.UpdateScore(_Score, value);
      if (_Score > Highscore && !NewHighScore) {
        _HUD.ShowMessage("New\nRecord!");
        NewHighScore = true;
      }
    }
  }
  private int Highscore = 0;
  private bool NewHighScore = false;

  public int Bonus {
    get{return _Bonus;}
    set{
      _Bonus = value;
      _HUD.UpdateBonus(_Bonus);
    }
  }

  public override void _Ready()
  {
    AdmobPlugin = (Godot.Object) GetNode("AdMob");
    // `load_banner` 는 admob.gd에 있는 함수임
    AdmobPlugin.Call("load_banner");
    _Circle = ResourceLoader.Load("objects/Circle.tscn") as PackedScene;
    _Jumper = ResourceLoader.Load("objects/Jumper.tscn") as PackedScene;

    _Camera = GetNode<Camera2D>("Camera2D");
    _StartPosition = GetNode<Position2D>("StartPosition");

    _Music = GetNode<AudioStreamPlayer>("Music");

    GD.Randomize();
    LoadScore();
    _HUD = GetNode<HUD>("HUD");
    _HUD.Hide();
    GetNode<ColorRect>("Background/ColorRect").Color = settings.Theme["background"];
  }

  public void NewGame()
  {
    NewHighScore = false;
    AdmobPlugin.Call("hide_banner");
    // 직접 호출해보고자 하였으나 동작하지 않음
    // settings.Admob.Call("HideAdBanner");
    Score = 0;
    Bonus = 0;
    _NumCircles = 0;
    _Level = 1;
    _HUD.UpdateScore(Score, 0);
    _Camera.Position = _StartPosition.Position;
    Player = _Jumper.Instance() as Jumper;
    Player.Position = _StartPosition.Position;
    AddChild(Player);
    Player.Connect("Captured", this, "OnJumperCaptured");
    Player.Connect("Died", this, "OnJumperDied");
    SpawnCircle(_StartPosition.Position);
    _HUD.Show();
    _HUD.ShowMessage("Go!");
    if (settings.EnableMusic == true) {
      _Music.VolumeDb = 0;
      _Music.Play();
    }
  }

  public void SpawnCircle(Vector2 _Position)
  {
    var c = _Circle.Instance() as Circle;
    AddChild(c);
    c.Connect("FullOrbit", this, "SetBonus", new Godot.Collections.Array() {1});
    c.init(_Position, _Level);
  }

  public void SetBonus(int value) {
    _Bonus = value;
    _HUD.UpdateBonus(_Bonus);
  }

  public void SpawnCircle() {
    var c = _Circle.Instance() as Circle;
    Vector2 _Position;
    float x = (float)GD.RandRange(-150, 150);
    float y = (float)GD.RandRange(-500, -400);
    _Position = Player.target.Position + new Vector2(x, y);

    AddChild(c);
    c.Connect("FullOrbit", this, "SetBonus", new Godot.Collections.Array() {1});
    c.init(_Position, _Level);
  }

  public async void OnJumperDied() {
    if (Score > Highscore) {
      Highscore = Score;
      SaveScore();
    }
    GetTree().CallGroup("Circles", "Implode");
    GetNode<Screens>("Screens").GameOver(Score, Highscore);
    _HUD.Hide();
    if (settings.EnableMusic == true) {
      // GetNode<AudioStreamPlayer>("Music").Play();
      FadeMusic();
    }

    await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
    if (settings.EnableAds) {
      if (GD.Randf() < settings.InterstialRate) {
        AdmobPlugin.Call("show_interstitial");
      } else {
        AdmobPlugin.Call("show_banner");
      }
    }
  }



  public void OnJumperCaptured(Circle _Object)
  {
    _Camera.Position = _Object.Position;
    _Object.Capture(Player);

    CallDeferred("SpawnCircle");
    Score += 1 * Bonus;
    Bonus += 1;
    _NumCircles += 1;
    if (_NumCircles > 0 && _NumCircles % settings.CirclePerLevel == 0) {
        _Level += 1;
        _HUD.ShowMessage($"Level {_Level}");
      }
  }

  public void LoadScore() {
    var f = new File();
    if (f.FileExists(settings.ScoreFile)) {
      f.Open(settings.ScoreFile, File.ModeFlags.Read);
      Highscore = (int)f.GetVar();
      f.Close();
    }
  }

  public void SaveScore() {
    var f = new File();
    f.Open(settings.ScoreFile, File.ModeFlags.Write);
    f.StoreVar(Highscore);
    f.Close();
  }
  public async void FadeMusic() {

    var MusicFade = GetNode<Tween>("MusicFade");
    MusicFade.InterpolateProperty(_Music, "volume_db", 0, -50, 1.0f, Tween.TransitionType.Sine, Tween.EaseType.In);
    MusicFade.Start();
    await ToSignal(MusicFade, "tween_all_completed");
    _Music.Stop();
  }

  public void _OnAdmobBannerFailedToLoad(int ErrorCode) {
    GD.Print($"Banner failed to load: Error Code {ErrorCode}\n");
  }

  public void _OnAdmobBannerLoaded() {
    GD.Print("Banner loaded\n");
  }

  public void _OnAdmobInterstitialClosed() {
    GD.Print("Interstitial closed\n");
  }

  public void _OnAdmobInterstitialFailedToLoad(int ErrorCode) {
    GD.Print($"Interstitial failed to load: Error code {ErrorCode}\n");
  }

  public void _OnAdmobInterstitialLoaded() {
    GD.Print("Interstitial loaded\n");
  }
}
