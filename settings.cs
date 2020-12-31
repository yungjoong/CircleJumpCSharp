using Godot;
using System;

public class settings : Node
{
    public static string ScoreFile = "user://highscore.save";
    public static string SettingsFile = "user://settings.save";
    public static bool EnableSound = true;
    public static bool EnableMusic = true;

    public static int CirclePerLevel = 5;

    public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Color>> ColorSchemes =
    new Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Color>>
    {
        {
            "NEON1",
            new Godot.Collections.Dictionary<string, Color>
            {
                {"background", Color.Color8(0,0,0)},
                {"player_body", Color.Color8(203, 255, 0)},
                {"player_trail", Color.Color8(205, 0,255)},
                {"circle_fill", Color.Color8(255, 0, 110)},
                {"circle_static", Color.Color8(0, 255,102)},
                {"circle_limited", Color.Color8(204, 0, 255)}
            }
        },
        {
            "NEON2",
            new Godot.Collections.Dictionary<string, Color>
            {
                {"background", Color.Color8(0, 0, 0)},
                {"player_body", Color.Color8(246, 255, 0)},
                {"player_trail", Color.Color8(255, 255, 255)},
                {"circle_fill", Color.Color8(255, 0, 110)},
                {"circle_static", Color.Color8(151, 255, 48)},
                {"circle_limited", Color.Color8(127, 0, 255)}
            }
        },
        {
            "NEON3",
            new Godot.Collections.Dictionary<string, Color>
            {
                {"background", Color.Color8(0, 0, 0)},
                {"player_body", Color.Color8(255, 0, 187)},
                {"player_trail", Color.Color8(255, 148, 0)},
                {"circle_fill", Color.Color8(255, 148, 0)},
                {"circle_static", Color.Color8(170, 255, 0)},
                {"circle_limited", Color.Color8(204, 0, 255)}
            }
        }
    };

    public static Godot.Collections.Dictionary<string, Color> Theme;
    // Called when the node enters the scene tree for the first time.

    public static Godot.Object Admob;

    bool RealAds = false;
    bool BannerTop = false;


    string AdBannerId = "ca-app-pub-6485759996905115/4596999216";
    string AdInterstialId = "ca-app-pub-6485759996905115/8153100840";
    
    public static bool EnableAds = true;
    public static float InterstialRate = 0.2f;

    public override void _Ready()
    {
        Theme = ColorSchemes["NEON1"];
        LoadSettings();
        // 전역변수 방식으로 광고 호출시 사용할 수 있음
        // GodotAdMob 은 android/plugins/GodotAdMob.gdap 를 확인하면 나오는 값임
        // if(Engine.HasSingleton("GodotAdMob") == true) {
        //     Admob = Engine.GetSingleton("GodotAdMob");
        //     Admob.Call("init", RealAds, GetInstanceId());
        //     Admob.Call("loadBanner", AdBannerId, BannerTop);
        //     GD.Print("SigneTone");
        // } else {
        //     GD.Print("No SingleTone");
        // }
    }

    public void ShowAdBanner() {
        GD.Print("ShowAdBanner Called\n");
        if (Admob != null && EnableAds) {
            Admob.Call("showBanner");
        }
    }

    public void HideAdBanner() {
        if (Admob != null) {
            Admob.Call("hideBanner");
        }
    }

    public void ShowAdInterstitial() {
        if (Admob != null && EnableAds) {
            Admob.Call("showInterstitial");
        }
    }

    // public void _OnInterstitialClose() {
    //     if (Admob != null && EnableAds) {
    //         ShowAdBanner();
    //     }
    // }

    public static int RandWeighted(int[] weights)
    {
        int sum = 0;
        foreach (var weight in weights)
            sum += weight;
        double num = GD.RandRange(0, sum);
        for (int i = 0; i < weights.Length; i++)
        {
            if (num < weights[i])
                return i;
            num -= weights[i];
        }
        return 0;
    }


    public static void SaveSettings()
    {
        File f = new File();
        f.Open(SettingsFile, File.ModeFlags.Write);
        f.StoreVar(EnableSound);
        f.StoreVar(EnableMusic);
        f.StoreVar(EnableAds);
        f.Close();
    }

    public static void LoadSettings()
    {
        var f = new File();
        if (f.FileExists(SettingsFile))
        {
            f.Open(SettingsFile, File.ModeFlags.Read);

            // GetVar는 file에서 순차적으로 Var에 접근하는 듯
            EnableSound = (bool)f.GetVar();
            EnableMusic = (bool)f.GetVar();
            EnableAds = (bool)f.GetVar();
            f.Close();
        }
    }
}
