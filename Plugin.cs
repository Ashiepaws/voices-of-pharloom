using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using TeamCherry.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace VoicesOfPharloom;

[BepInAutoPlugin(id: "dev.ashiepaws.voices_of_pharloom")]
[HarmonyPatch]
public partial class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static List<PackData> PluginPacks = new();

    internal static string CurrentDialogueKey = "";
    internal static int CurrentDialogueIndex = 0;
    internal static AudioSource DialogueAudioSource;

    private void Awake()
    {
        Logger = base.Logger;

        var harmony = new Harmony(Id);
        StartCoroutine(AwakeDelayed(harmony));

        Directory.GetDirectories(Paths.PluginPath, "VoA", SearchOption.AllDirectories).ToList().ForEach(dir =>
        {
            var packData = ParsePackData(dir);
            PluginPacks.Add(packData);
        });

        DialogueAudioSource = new GameObject("VoA_DialogueAudioSource").AddComponent<AudioSource>();
        DialogueAudioSource.spatialBlend = 0f;
        DontDestroyOnLoad(DialogueAudioSource.gameObject);

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private PackData ParsePackData(string path)
    {
        var packName = path.Split(Path.DirectorySeparatorChar)[^2];
        
        var validLanguages = Enum.GetValues(typeof(GlobalEnums.SupportedLanguages)).Cast<GlobalEnums.SupportedLanguages>().Select(lang => lang.ToString()).ToHashSet();
        var languages = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)
            .Select(dir => dir.Split(Path.DirectorySeparatorChar)[^1])
            .Where(validLanguages.Contains)
            .ToHashSet();

        return new PackData
        {
            PackName = packName,
            Directory = path,
            Languages = languages,
            Enabled = true
        };
    }

    private IEnumerator<object> AwakeDelayed(Harmony harmony)
    {
        yield return null;
        harmony.PatchAll();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.PrintText))]
    public static void PrintTextPatch(TMP_TextInfo textInfo, int pageIndex)
    {
        PlayDialogueAudio($"{CurrentDialogueKey}_{CurrentDialogueIndex++}");
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunDialogueBase), nameof(PlayMakerNPC.StartDialogue), [ typeof(PlayMakerNPC) ])]
    public static void StartDialoguePatch(PlayMakerNPC component, RunDialogueBase __instance)
    {
        if (__instance is RunDialogue rd)
            CurrentDialogueKey = $"{rd.Sheet}_{rd.Key}";
        else if (__instance is RunDialogueV2 rd2)
            CurrentDialogueKey = rd2.UsesCustomText() ? "CUSTOM" : $"{rd2.Sheet}_{rd2.Key}";
        else if (__instance is RunDialogueV3 rd3)
            CurrentDialogueKey = rd3.UsesCustomText() ? "CUSTOM" : $"{rd3.Sheet}_{rd3.Key}";
        else if (__instance is RunDialogueV4 rd4)
            CurrentDialogueKey = rd4.UsesCustomText() ? "CUSTOM" : $"{rd4.Sheet}_{rd4.Key}";
        else if (__instance is RunDialogueV5 rd5)
            CurrentDialogueKey = rd5.UsesCustomText() ? "CUSTOM" : $"{rd5.Sheet}_{rd5.Key}";
        CurrentDialogueIndex = 0;
    }

    private static void PlayDialogueAudio(string dialogueKey)
    {
        if (string.IsNullOrEmpty(dialogueKey))
            return;

        foreach (var pack in PluginPacks.Where(p => p.Enabled && p.Languages.Contains(Language.CurrentLanguage().ToString())))
        {
            var audioPath = GetAudioPath(pack, dialogueKey);
            if (audioPath != null)
            {
                var clip = LoadAudioClip(audioPath);
                if (clip != null)
                {
                    DialogueAudioSource.clip = clip;
                    DialogueAudioSource.Play();
                    return;
                }
            }
        }
    }

    private static AudioClip? LoadAudioClip(string path)
    {
        string url = "file:///" + Uri.EscapeUriString(path.Replace("\\", "/"));
        var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
        var operation = request.SendWebRequest();
        while (!operation.isDone) { }
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError($"Failed to load audio clip from {path}: {request.error}");
            return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        clip.name = $"VoA_{Path.GetFileNameWithoutExtension(path)}";
        return clip;
    }

    private static string GetAudioPath(PackData pack, string filename)
    {
        var allFiles = Directory.GetFiles(Path.Combine(pack.Directory, Language.CurrentLanguage().ToString()), $"{filename}.*", SearchOption.TopDirectoryOnly);
        return allFiles.FirstOrDefault();
    }

    internal class PackData
    {
        public string PackName { get; set; }
        public string Directory { get; set; }
        public HashSet<string> Languages { get; set; }
        public bool Enabled { get; set; }
    }
}
