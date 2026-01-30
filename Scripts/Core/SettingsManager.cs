using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class SettingsManager : Node
{
	private const string SettingsFilePath = "user://settings.tres";
	
	private const int MasterBus = 0;
	private const int EffectsBus = 1;
	private const int MusicBus = 2;

	public override void _Ready()
	{
		LoadSettings();
	}

	public static double MainVolume
	{
		get => AudioServer.Singleton.GetBusVolumeLinear(MasterBus);
		set => AudioServer.Singleton.SetBusVolumeLinear(MasterBus, (float)value);
	}

	public static double EffectsVolume
	{
		get => AudioServer.Singleton.GetBusVolumeLinear(EffectsBus);
		set => AudioServer.Singleton.SetBusVolumeLinear(EffectsBus, (float)value);
	}
	
	public static double MusicVolume
	{
		get => AudioServer.Singleton.GetBusVolumeLinear(MusicBus);
		set => AudioServer.Singleton.SetBusVolumeLinear(MusicBus, (float)value);
	}

	private static void LoadSettings()
	{
		if (!FileAccess.FileExists(SettingsFilePath))
		{
			SaveSettings();
			return;
		}
		
		UserData.Settings? settings = ResourceLoader.Load<UserData.Settings>(SettingsFilePath);
		if (settings == null)
		{
			SaveSettings();
			return;
		}
		
		MainVolume = settings.MainVolume;
		EffectsVolume = settings.EffectsVolume;
		MusicVolume = settings.MusicVolume;
	}
	
	private static void SaveSettings()
	{
		UserData.Settings settings = new()
		{
			MainVolume = MainVolume,
			EffectsVolume = EffectsVolume,
			MusicVolume = MusicVolume
		};
		
		ResourceSaver.Save(settings, SettingsFilePath);
	}
}