using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PrepararActividad24
{
    const string Root = "Assets/Animaciones24";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    static readonly string[] Names = { "Orbita", "Pulso", "Cometa" };
    static PrepararActividad24()
    {
        if (SessionState.GetBool("Prueba24", false))
            EditorApplication.playModeStateChanged += CheckPlay;
    }

    [MenuItem("Tools/Actividad 2.4/Integrar y probar")]
    public static void Build()
    {
        Directory.CreateDirectory(Root);
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var panel = new GameObject("Galeria de animaciones", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(GameObject.Find("Canvas").transform, false);
        Place(panel, new Vector2(.07f,.25f), new Vector2(.93f,.76f));
        panel.GetComponent<Image>().color = new Color(.025f,.055f,.1f,.98f);

        for (int i = 0; i < Names.Length; i++)
        {
            string name = Names[i];
            var tile = new GameObject(name, typeof(RectTransform), typeof(Image));
            tile.transform.SetParent(panel.transform, false);
            Place(tile, new Vector2(.025f+i*.325f,.08f), new Vector2(.325f+i*.325f,.92f));
            tile.GetComponent<Image>().color = new Color(.08f,.14f,.23f);
            var viewport = new GameObject("Ventana", typeof(RectTransform));
            viewport.transform.SetParent(tile.transform, false);
            Place(viewport, new Vector2(.12f,.12f), new Vector2(.88f,.88f));
            var fitter = viewport.AddComponent<AspectRatioFitter>();
            fitter.aspectRatio = 1;
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            if (i == 2)
            {
                viewport.AddComponent<Image>().sprite = SpriteFile("MascaraCircular", 3, 0);
                viewport.AddComponent<Mask>().showMaskGraphic = false;
            }
            var item = new GameObject("Sprite animado", typeof(RectTransform), typeof(Image), typeof(Animator));
            item.transform.SetParent(viewport.transform, false);
            Place(item, Vector2.zero, Vector2.one);
            var image = item.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            var clip = new AnimationClip { frameRate = 8 };
            var keys = new ObjectReferenceKeyframe[9];
            for (int frame = 0; frame < 8; frame++)
                keys[frame] = new ObjectReferenceKeyframe { time = frame / 8f, value = SpriteFile(name + "_" + (frame + 1), i, frame) };
            image.sprite = (Sprite)keys[0].value;
            keys[8] = new ObjectReferenceKeyframe { time = 1f, value = keys[0].value };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(Image), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, Root + "/" + name + ".anim");
            var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(Root + "/" + name + ".controller", clip);
            item.GetComponent<Animator>().runtimeAnimatorController = controller;
        }
        var button = GameObject.Find("BotonIniciar").GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(button.onClick, panel.SetActive, true);
        GameObject.Find("Titulo").GetComponent<TMPro.TMP_Text>().text = "2.4. Animaciones de sprites";
        panel.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        SessionState.SetBool("Prueba24", true);
        EditorApplication.playModeStateChanged += CheckPlay;
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }

    static void Place(GameObject obj, Vector2 min, Vector2 max)
    {
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static Sprite SpriteFile(string name, int type, int frame)
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float angle = frame * Mathf.PI / 4;
        var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x - 31.5f, y - 31.5f);
            float r = p.magnitude;
            Color c = Color.clear;
            if (type == 0)
            {
                if (Mathf.Abs(r - 19) < 1) c = new Color(.18f,.36f,.47f);
                if (r < 8) c = new Color(.33f,.88f,1);
                if ((p - direction * 19).magnitude < 5) c = new Color(.82f,1,1);
            }
            if (type == 1)
            {
                float radius = 14 + 6 * Mathf.Sin(angle);
                if (Mathf.Abs(r - radius - 6) < 1.5f) c = new Color(.55f,.32f,.78f);
                if (Mathf.Abs(p.x) + Mathf.Abs(p.y) < radius) c = new Color(.85f,.61f,1);
                if (Mathf.Abs(p.x) + Mathf.Abs(p.y) < radius * .45f) c = new Color(1,.86f,1);
                if ((p - direction * 23).magnitude < 2) c = Color.white;
            }
            if (type == 2)
            {
                c = new Color(.03f,.07f,.14f);
                if ((x * 31 + y * 47) % 131 == 0) c = new Color(.4f,.57f,.75f);
                Vector2 head = direction * 20;
                Vector2 tangent = new Vector2(-direction.y,direction.x);
                for (int j = 14; j >= 0; j--)
                {
                    if ((p - head + tangent * j).magnitude < 7 - j * .34f)
                        c = Color.Lerp(new Color(1,.91f,.5f), new Color(1,.29f,.12f), j / 14f);
                }
            }
            if (type == 3 && r < 23) c = Color.white;
            tex.SetPixel(x,y,c);
        }
        tex.Apply();
        string path = Root + "/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void CheckPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            GameObject.Find("BotonIniciar").GetComponent<Button>().onClick.Invoke();
            var panel = GameObject.Find("Galeria de animaciones");
            var animators = panel.GetComponentsInChildren<Animator>();
            if (animators.Length != 3 || panel.GetComponentsInChildren<Mask>().Length != 1)
                throw new Exception("Galeria incompleta");
            var seen = animators.Select(a => new HashSet<Sprite>()).ToArray();
            double started = EditorApplication.timeSinceStartup;
            EditorApplication.CallbackFunction update = null;
            update = () =>
            {
                for (int i = 0; i < animators.Length; i++)
                    seen[i].Add(animators[i].GetComponent<Image>().sprite);
                if (EditorApplication.timeSinceStartup - started < 3) return;
                EditorApplication.update -= update;
                bool passed = true;
                for (int i = 0; i < animators.Length; i++)
                {
                    var current = animators[i].GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"PRUEBA24 {animators[i].transform.parent.parent.name}: sprites={seen[i].Count}, loop={current.loop}, ciclos={current.normalizedTime:F2}");
                    passed &= seen[i].Count == 8 && current.loop && current.normalizedTime > 2;
                }
                SessionState.SetBool("Prueba24", false);
                Debug.Log("PRUEBA24 " + (passed ? "OK" : "FALLO"));
            };
            EditorApplication.update += update;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SessionState.SetBool("Prueba24", false);
        }
    }
}


