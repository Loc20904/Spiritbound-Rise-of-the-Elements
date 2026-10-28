// See Tools/Unity/README.md for CLI usage, snapshots and rollback.
// One-time, project-specific scene grouping migration (Unity 6000.3.3f1).
// Editor-only maintenance utility. This file deliberately lives outside Assets.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHierarchyMigration
{
    static string Project => Directory.GetParent(Application.dataPath).FullName;
    static string Work => Path.Combine(Project, "Library", "SceneHierarchyMigration", "2026-10-28");
    static readonly HashSet<string> Persistent = new HashSet<string> {
        "SceneController", "GameManager", "GameProgressManager", "PlayerStatManager", "SettingsManager", "SFXPool"
    };
    static readonly HashSet<string> Groups = new HashSet<string> {
        "_Systems", "_Cameras", "_World", "_Actors", "_Encounter", "_UI", "_Audio", "_Sequences"
    };
    static readonly string[] Order = {
        "_Systems", "_Cameras", "_World", "_Actors", "_Encounter", "_UI", "_Audio", "_Sequences"
    };
    static string[] Paths => EditorBuildSettings.globalScenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    static readonly Dictionary<int,string> ObjectIds = new Dictionary<int,string>();
    static string Remember(UnityEngine.Object value, string id) { ObjectIds[value.GetInstanceID()] = id; return id; }
    static string Id(UnityEngine.Object value)
    {
        if (value == null) return "null";
        if (ObjectIds.TryGetValue(value.GetInstanceID(), out var cached)) return cached;
        var global = GlobalObjectId.GetGlobalObjectIdSlow(value);
        if (global.identifierType != 0) return Remember(value, global.ToString());
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string guid, out long local))
            return Remember(value, "asset:" + guid + ":" + local);
        return Remember(value, value.GetType().FullName + ":" + value.name);
    }
    static string PathOf(Transform value) => value.parent == null ? value.name : PathOf(value.parent) + "/" + value.name;
    static bool IsPersistentRoot(GameObject root) => root.GetComponentsInChildren<MonoBehaviour>(true)
        .Any(b => b != null && Persistent.Contains(b.GetType().Name));
    static bool Has(GameObject root, string type) => root.GetComponents<Component>().Any(c => c != null && c.GetType().Name == type);

    public static string Destination(GameObject root, string sceneName)
    {
        if (IsPersistentRoot(root)) return "@persistent";
        if (Groups.Contains(root.name)) return "@group";
        string name = root.name.ToLowerInvariant();
        if (Has(root, "PlayerHealth")) return "_Actors/Player";
        if (root.GetComponents<MonoBehaviour>().Any(b => b != null && (b is EnemyBase || b is EnemyHealth)))
            return "_Actors/Enemies";
        if (Has(root, "BossHealth")) return "_Encounter/Boss";
        if (Has(root, "BossPillar")) return "_Encounter/Mechanics";
        if (Has(root, "FortuneTellerNPC") || Has(root, "NPCInteractable")) return "_Actors/NPCs";
        if (Has(root, "Camera") || name == "camera" || name == "cameraholder" || name.Contains("cinemachine"))
            return "_Cameras";
        if (name == "cameralimit" || name == "camerallimit" || name == "limitcamera") return "_Cameras/Bounds";
        if (Has(root, "AudioSource") || Has(root, "AudioManager")) return "_Audio";
        if (Has(root, "PlayableDirector")) return "_Sequences/Timelines";
        if (Has(root, "IntroChangeScene") || Has(root, "BossCutsceneManager")) return "_Sequences/Controllers";
        if (Has(root, "SubtitleController")) return "_Sequences/Subtitles";
        if (name == "cutscene") return "_Sequences/Cinematics";
        if (name.Contains("framename")) return "_Sequences/Titles";
        if (name == "dragon" || name.StartsWith("finalboss_ulti") || name == "groundvfx_0" ||
            name == "water_skill_0" || name == "s1_0" || name == "s2_0" || name == "s3_0") return "_Encounter/Effects";
        if (name == "bossskillsholder") return "_Encounter/Effects";
        if (name == "ultipoint" || name == "spawnulti" || name == "telepoint") return "_Encounter/Anchors";
        if (name == "holdboss") return "_Encounter/Triggers";
        if (Has(root, "Canvas"))
        {
            if (sceneName == "IntroScene" || (sceneName == "FinalBossScene" && Has(root, "Animator")))
                return "_Sequences/Presentation";
            if (name.Contains("dialogue") || name.Contains("choice")) return "_UI/DialogueAndChoices";
            return "_UI";
        }
        if (name == "gameuicomponent" || name == "eventsystem" || name == "tutorial") return "_UI";
        if (name == "mainmenulogo_0") return "_UI/Branding";
        if (Has(root, "DialogueUIManager")) return "_Systems";
        if (Has(root, "Grid") || name == "map") return "_World/Terrain";
        if (Has(root, "ParallaxController") || name.StartsWith("background")) return "_World/Backgrounds";
        if (Has(root, "FinishPoint")) return "_World/Transitions";
        if (name.StartsWith("checkpoint") || name == "startrespawn" || name == "startpoint") return "_World/Checkpoints";
        if (Has(root, "Lava") || name.StartsWith("lava") || name.Contains("spike") ||
            name.StartsWith("falltrap") || name.StartsWith("trap")) return "_World/Hazards";
        if (name.StartsWith("platform") || name.StartsWith("bêjump") || name == "g_fireground") return "_World/Platforms";
        if (name == "bounary" || name == "boundary") return "_World/Boundaries";
        if (name == "triangle") return "_World/Decorations";
        throw new InvalidOperationException("Unclassified root in " + sceneName + ": " + root.name + " [" +
            string.Join(",", root.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name)) + "]");
    }

    public sealed class Node
    {
        public string id, name, tag, parent, prefab;
        public int layer, staticFlags;
        public bool activeSelf, activeHierarchy;
        public float[] matrix;
        public string[] children, componentTypes;
        public SortedDictionary<string, string> values = new SortedDictionary<string, string>(StringComparer.Ordinal);
    }
    public sealed class Snapshot
    {
        public string scene;
        public int roots, missing;
        public List<Node> nodes = new List<Node>();
    }
    static string PropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.ObjectReference: return Id(property.objectReferenceValue);
            case SerializedPropertyType.Integer: return property.longValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.Boolean: return property.boolValue ? "true" : "false";
            case SerializedPropertyType.Float: return property.doubleValue.ToString("R", CultureInfo.InvariantCulture);
            case SerializedPropertyType.String: return property.stringValue;
            case SerializedPropertyType.Enum: return property.intValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.Color: return JsonUtility.ToJson(property.colorValue);
            case SerializedPropertyType.Vector2: return JsonUtility.ToJson(property.vector2Value);
            case SerializedPropertyType.Vector3: return JsonUtility.ToJson(property.vector3Value);
            case SerializedPropertyType.Vector4: return JsonUtility.ToJson(property.vector4Value);
            case SerializedPropertyType.Quaternion: return JsonUtility.ToJson(property.quaternionValue);
            case SerializedPropertyType.Rect: return JsonUtility.ToJson(property.rectValue);
            case SerializedPropertyType.Bounds: return JsonUtility.ToJson(property.boundsValue);
            case SerializedPropertyType.ArraySize: return property.intValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.Character: return property.intValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.LayerMask: return property.intValue.ToString(CultureInfo.InvariantCulture);
            default: return null;
        }
    }
    static string TilemapHash(Component component)
    {
        string json = EditorJsonUtility.ToJson(component);
        // JSON uses legacy instance IDs; resolve them only to produce stable cross-reload hashes.
#pragma warning disable CS0618
        json = Regex.Replace(json, @"""instanceID""\s*:\s*(-?\d+)", match =>
            "\"reference\":" + JsonConvert.SerializeObject(Id(EditorUtility.InstanceIDToObject(int.Parse(match.Groups[1].Value)))));
#pragma warning restore CS0618
        using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(json))).Replace("-", "");
    }
    static bool Bookkeeping(string key) => key.EndsWith(".m_FileID") || key.EndsWith(".m_PathID");
    public static Snapshot Capture(Scene scene)
    {
        Canvas.ForceUpdateCanvases();
        ObjectIds.Clear();
        var result = new Snapshot { scene = scene.path, roots = scene.rootCount };
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            var go = t.gameObject;
            var components = go.GetComponents<Component>();
            var state = new Node {
                id = Id(go), name = go.name, tag = go.tag, layer = go.layer,
                staticFlags = (int)GameObjectUtility.GetStaticEditorFlags(go), activeSelf = go.activeSelf,
                activeHierarchy = go.activeInHierarchy, parent = t.parent == null ? "null" : Id(t.parent.gameObject),
                children = t.Cast<Transform>().Select(c => Id(c.gameObject)).ToArray(),
                componentTypes = components.Select(c => c == null ? "MISSING" : c.GetType().FullName).ToArray(),
                prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go),
                matrix = Enumerable.Range(0,16).Select(i => t.localToWorldMatrix[i]).ToArray()
            };
            result.missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            for (int index = 0; index < components.Length; index++)
            {
                var component = components[index];
                if (component == null || component is Transform) continue;
                if (component is UnityEngine.Tilemaps.Tilemap)
                {
                    state.values[index + ":tilemapHash"] = TilemapHash(component);
                    continue;
                }
                using var serialized = new SerializedObject(component);
                var property = serialized.GetIterator();
                bool enter = true;
                while (property.Next(enter))
                {
                    enter = property.propertyType != SerializedPropertyType.ObjectReference;
                    if (property.name == "m_EditorClassIdentifier") continue;
                    var value = PropertyValue(property);
                    if (value != null) state.values[index + ":" + property.propertyPath] = value;
                }
                if (component is Collider2D collider)
                    state.values[index + ":attachedRigidbody"] = Id(collider.attachedRigidbody);
            }
            result.nodes.Add(state);
        }
        if (result.nodes.GroupBy(n => n.id).Any(g => g.Count() > 1)) throw new Exception("Non-unique object IDs in " + scene.path);
        return result;
    }
    public static List<string> Compare(Snapshot before, Snapshot after, HashSet<string> moved)
    {
        var errors = new List<string>();
        var byId = after.nodes.ToDictionary(n => n.id);
        foreach (var old in before.nodes)
        {
            if (!byId.TryGetValue(old.id, out var current)) { errors.Add("Missing object: " + old.name + " " + old.id); continue; }
            if (old.name != current.name || old.tag != current.tag || old.layer != current.layer ||
                old.staticFlags != current.staticFlags || old.activeSelf != current.activeSelf || old.activeHierarchy != current.activeHierarchy)
                errors.Add("Object attributes changed: " + old.name);
            if (old.prefab != current.prefab) errors.Add("Prefab link changed: " + old.name);
            if (!old.componentTypes.SequenceEqual(current.componentTypes)) errors.Add("Components changed: " + old.name);
            if (!old.children.SequenceEqual(current.children)) errors.Add("Internal child order changed: " + old.name);
            if (!moved.Contains(old.id) && old.parent != current.parent) errors.Add("Internal parent changed: " + old.name);
            for (int i = 0; i < 16; i++)
                if (Math.Abs(old.matrix[i] - current.matrix[i]) > 0.0005f)
                { errors.Add("World matrix changed: " + old.name + " element " + i + " " + old.matrix[i] + " -> " + current.matrix[i]); break; }
            foreach (var value in old.values.Where(v => !Bookkeeping(v.Key)))
                if (!current.values.TryGetValue(value.Key, out var now) || value.Value != now)
                    errors.Add("Serialized value/reference changed: " + old.name + " " + value.Key + " " + value.Value + " -> " + now);
            foreach (var key in current.values.Keys.Except(old.values.Keys).Where(k => !Bookkeeping(k))) errors.Add("Serialized property added: " + old.name + " " + key);
        }
        if (before.missing != after.missing) errors.Add("Missing script count changed");
        return errors;
    }
    static Transform EnsureGroup(Scene scene, string path, Dictionary<string, Transform> groups, List<string> created)
    {
        if (groups.TryGetValue(path, out var found)) return found;
        int split = path.LastIndexOf('/');
        Transform parent = split < 0 ? null : EnsureGroup(scene, path.Substring(0,split), groups, created);
        string name = split < 0 ? path : path.Substring(split+1);
        var existing = parent == null ? scene.GetRootGameObjects().Select(g => g.transform).FirstOrDefault(t => t.name == name)
            : parent.Cast<Transform>().FirstOrDefault(t => t.name == name);
        if (existing != null)
        {
            if (existing.localPosition != Vector3.zero || existing.localRotation != Quaternion.identity || existing.localScale != Vector3.one)
                throw new Exception("Existing container is not identity: " + path);
            groups[path] = existing; return existing;
        }
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        Undo.RegisterCreatedObjectUndo(go, "Create scene hierarchy group");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        groups[path] = go.transform; created.Add(path);
        return go.transform;
    }
    static Scene Acquire(string path, out bool wasLoaded)
    {
        var scene = SceneManager.GetSceneByPath(path);
        wasLoaded = scene.IsValid() && scene.isLoaded;
        if (wasLoaded && scene.isDirty) throw new Exception("Scene has unsaved edits; migration will not overwrite them: " + path);
        return wasLoaded ? scene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }
    public static object Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play Mode before migration");
        Directory.CreateDirectory(Work);
        var scenes = new JArray();
        foreach (var path in Paths)
        {
            var scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                ObjectIds.Clear();
                var roots = scene.GetRootGameObjects();
                scenes.Add(new JObject { ["scene"] = scene.name, ["path"] = path, ["roots"] = new JArray(roots.Select(r =>
                    new JObject { ["id"] = Id(r), ["name"] = r.name, ["destination"] = Destination(r, scene.name) })) });
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
        File.WriteAllText(Path.Combine(Work,"manifest.json"), scenes.ToString());
        return scenes.Select(s => new { scene = (string)s["scene"], roots = s["roots"].Count(), retainedServices = s["roots"].Count(r => (string)r["destination"] == "@persistent") }).ToArray();
    }
    public static object ApplyScene(string path)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play Mode before migration");
        var active = SceneManager.GetActiveScene();
        bool wasLoaded;
        var scene = Acquire(path, out wasLoaded);
        int undoGroup = -1;
        bool saved = false;
        try
        {
            var roots = scene.GetRootGameObjects();
            var assignments = roots.Select(r => new { root = r, destination = Destination(r, scene.name) }).ToArray();
            var before = Capture(scene);
            string folder = Path.Combine(Work,"snapshots",scene.name);Directory.CreateDirectory(folder);
            if (File.Exists(Path.Combine(folder,"before.json"))) throw new Exception("A migration snapshot already exists for " + scene.name);
            File.WriteAllText(Path.Combine(folder,"before.json"),JsonConvert.SerializeObject(before,Formatting.Indented));
            File.Copy(Path.Combine(Project,path),Path.Combine(folder,"before.unity"),false);
            Undo.IncrementCurrentGroup();undoGroup = Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Organize " + scene.name + " hierarchy");
            var groups = new Dictionary<string,Transform>();var created = new List<string>();var moved = new HashSet<string>();var mapping = new JArray();
            foreach (var assignment in assignments)
            {
                if (assignment.destination.StartsWith("@")) continue;
                var target = EnsureGroup(scene,assignment.destination,groups,created);
                string id = Id(assignment.root);
                Undo.SetTransformParent(assignment.root.transform,target,"Organize scene root");
                moved.Add(id);mapping.Add(new JObject { ["id"] = id, ["name"] = assignment.root.name, ["destination"] = assignment.destination });
            }
            foreach (string name in Order)
                if (groups.TryGetValue(name,out var group)) group.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            var after = Capture(scene);
            var errors = Compare(before,after,moved);
            if (errors.Count > 0) throw new Exception(string.Join("\n",errors.Take(40)));
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene,path)) throw new Exception("Save failed: " + path);
            saved = true;
            after = Capture(scene);errors = Compare(before,after,moved);
            if (errors.Count > 0) throw new Exception("Post-save validation failed:\n" + string.Join("\n",errors.Take(40)));
            File.WriteAllText(Path.Combine(folder,"after.json"),JsonConvert.SerializeObject(after,Formatting.Indented));
            var report = new { scene = scene.name, path, rootsBefore = before.roots, rootsAfter = after.roots,
                originalObjects = before.nodes.Count, totalObjectsAfter = after.nodes.Count,
                groupsCreated = created, movedRoots = mapping, preservedObjects = before.nodes.Count, errors };
            File.WriteAllText(Path.Combine(folder,"result.json"),JsonConvert.SerializeObject(report,Formatting.Indented));
            return report;
        }
        catch
        {
            if (!saved && undoGroup >= 0) Undo.RevertAllDownToGroup(undoGroup);
            throw;
        }
        finally
        {
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
            if (!wasLoaded && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene,true);
        }
    }
    public static object ValidatePilot()
    {
        string path="Assets/_Project/Scenes/MapForest.unity", folder=Path.Combine(Work,"snapshots","MapForest");
        var before=JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(Path.Combine(folder,"before.json")));
        var migration=JObject.Parse(File.ReadAllText(Path.Combine(folder,"result.json")));
        var moved=new HashSet<string>(migration["movedRoots"].Select(r=>(string)r["id"]));
        bool loaded;var scene=Acquire(path,out loaded);
        try { var after=Capture(scene);var errors=Compare(before,after,moved);return new {scene=scene.name,roots=scene.rootCount,objects=after.nodes.Count,errors}; }
        finally {if(!loaded)EditorSceneManager.CloseScene(scene,true);}
    }
    public static object ApplyPresentation() => new[]{ ApplyScene("Assets/_Project/Scenes/CreditsScene.unity"),ApplyScene("Assets/_Project/Scenes/MainMenuScene.unity") };
    public static object ApplyPilot() => ApplyScene("Assets/_Project/Scenes/MapForest.unity");
    public static object ApplyRemaining()
    {
        var results = new List<object>();
        foreach (string name in new[]{"MapVolcano","MapIce","BossFireScene","BossGroundScene","BossWaterScene","FinalBossScene","FortuneScene","IntroScene","CreditsScene","MainMenuScene"})
            results.Add(ApplyScene("Assets/_Project/Scenes/"+name+".unity"));
        return results;
    }
    public static object ValidateSaved()
    {
        var results = new List<object>();
        foreach (string path in Paths)
        {
            string name = Path.GetFileNameWithoutExtension(path), folder = Path.Combine(Work,"snapshots",name);
            var before = JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(Path.Combine(folder,"before.json")));
            var migration = JObject.Parse(File.ReadAllText(Path.Combine(folder,"result.json")));
            var moved = new HashSet<string>(migration["movedRoots"].Select(r => (string)r["id"]));
            bool loaded;var scene = Acquire(path,out loaded);
            try
            {
                var current = Capture(scene);
                var errors = Compare(before,current,moved);
                results.Add(new { scene=name, roots=current.roots, objects=current.nodes.Count, errors });
            }
            finally { if (!loaded) EditorSceneManager.CloseScene(scene,true); }
        }
        File.WriteAllText(Path.Combine(Work,"saved-validation.json"),JsonConvert.SerializeObject(results,Formatting.Indented));
        return results;
    }
}