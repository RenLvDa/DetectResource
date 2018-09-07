using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResourceScanner : EditorWindow {

    //菜单入口
    [MenuItem("UI/SimplePlugDemo")]
    static void Init() {
		ResourceScanner window = (ResourceScanner)EditorWindow.GetWindow(typeof(ResourceScanner));
        window.Show();
    }

	//资源类型
	const string MATERIAL = "material";
	const string TEXTURE2D = "texture2D";
	const string AUDIO_CLIP = "audioClip";
	const string SPRITE = "sprite";
	const string FONT = "font";
	const string MESH = "mesh";
	const string ANIMATION_CLIP = "animationClip";
	const string RUNTIME_ANIMATOR_CONTROLLER = "runtimeAnimatorController";
	const string AVATOR = "avator";

    //保存的资源列表
    private Dictionary<string, ResourceHolder> mResourceDict = new Dictionary<string, ResourceHolder>();
    private List<ResourceHolder> mResourceList = new List<ResourceHolder>();
	private List<InActiveHolder> mInActiveList = new List<InActiveHolder>();

    //场景的默认文件路径
    private string mSceneFolderPath = "Assets/02.scene/";
    //保存需要扫描场景名字的列表文件
    private string mSceneNameListFilePath = "D:/workspace/sceneList.txt";
    //用于保存的文件地址
    private string mSaveFolderPath = "D:/workspace/test/";
    //用于保存记录的文件
    private string mSaveFileName = "default.csv";
    //显示宽度（每列）
    const float WIDTH = 100f;
    //用于保存滚动内容位置
    private Vector2 mScrollPos = new Vector2(0, 0);
    //用于显示资源排序方向
    private string mTypeSortStr = "资源类型";
    private string mUseSortStr = "资源使用次数";
    private const string UP = "↑";
    private const string DOWN = "↓";
    private bool mTypeSortOrder = true;
    private bool mUseSortOrder = true;
    //绘制
    void OnGUI() {
        //设置保存文件夹位置
        GUILayout.BeginHorizontal();
        GUILayout.Label("保存文件夹路径");
        GUILayout.Label(mSaveFolderPath);
        if (GUILayout.Button("浏览")) {
            //选择文件夹位置
            Debug.Log("还没弄。");
        }
        GUILayout.EndHorizontal();

        //设置场景文件夹位置
        GUILayout.BeginHorizontal();
        GUILayout.Label("场景文件夹路径");
        GUILayout.Label(mSceneFolderPath);
        if (GUILayout.Button("浏览")) {
            //选择文件夹位置
            Debug.Log("还没弄。");
        }
        GUILayout.EndHorizontal();

        //扫描功能按键
        GUILayout.BeginHorizontal();
        bool scanBtn = GUILayout.Button("扫描当前场景");
        bool scanSaveBtn = GUILayout.Button("扫描当前场景并保存");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool scanListBtn = GUILayout.Button("扫描列表中所有场景（整合统计）");
        bool scanListSaveBtn = GUILayout.Button("扫描列表中所有场景（分别保存）");
        GUILayout.EndHorizontal();
        //核心功能，扫描当前场景，统计资源使用情况
        if (scanBtn) {
            Scene nowScene = EditorSceneManager.GetActiveScene();
            BuildResourceList(nowScene);
        }

		//扫描当前场景，统计未激活的GameObject
		if (GUILayout.Button("ScanInActive")) {
			Scene nowScene = EditorSceneManager.GetActiveScene();
			ClearResourceList ();
			BuildInActiveResourceList (nowScene);
		}

        if (scanSaveBtn) {
            Scene nowScene = EditorSceneManager.GetActiveScene();
            BuildResourceList(nowScene);
            if (SaveToFile(mSaveFolderPath, nowScene.name)) {
                Debug.Log("保存成功");
            }
        }

        //扫描提供名字的场景中的所有的资源引用
        if (scanListBtn) {
            string[] nameOfScenes = GetNameOfFilesToScan(mSceneNameListFilePath);
            float progress = 0f;
            float progressStep = 1.0f / nameOfScenes.Length;
            for (int i = 0; i < nameOfScenes.Length; i++) {
                EditorUtility.DisplayProgressBar("扫描中", nameOfScenes[i], progress += progressStep);
                Scene scene = EditorSceneManager.GetSceneByPath(mSceneFolderPath + nameOfScenes[i] + ".unity");
                BuildResourceList(scene);
            }
            EditorUtility.ClearProgressBar();
        }

        //对每个提供名字的场景依次扫描并按场景名保存为文件
        if (scanListSaveBtn) {
            string[] nameOfScenes = GetNameOfFilesToScan(mSceneNameListFilePath);
            float progress = 0f;
            float progressStep = 1.0f / nameOfScenes.Length;
            for (int i = 0; i < nameOfScenes.Length; i++) {
                EditorUtility.DisplayProgressBar("扫描中", nameOfScenes[i], progress += progressStep);
                Scene scene = EditorSceneManager.GetSceneByPath(mSceneFolderPath + nameOfScenes[i] + ".unity");
                BuildResourceList(scene);
                if (SaveToFile(mSaveFolderPath, scene.name)) {
                    Debug.Log("保存成功");
                }
                ClearResourceList();
            }
            EditorUtility.ClearProgressBar();
        }


        //保存到文件
        if (GUILayout.Button("保存到文件(default.csv)")) {
            if (SaveToFile(mSaveFileName)) {
                Debug.Log("保存成功");
            }
        }

        //从文件中读取
        if (GUILayout.Button("从文件中加载(default.csv)")) {
            if (LoadFromFile(mSaveFileName)) {
                Debug.Log("加载成功");
            }
        }

        //展示扫描结果
        if (mResourceList.Count > 0 && mResourceList.Count < 50) {
            GUILayout.Label("扫描结果（最多显示50条）");
            //表头
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源名称", GUILayout.Width(WIDTH));
            if (GUILayout.Button(mTypeSortStr, GUILayout.Width(WIDTH))) {
                SortWithType(mTypeSortOrder);
                mTypeSortStr = "资源类型" + (mTypeSortOrder ? UP : DOWN);
                mUseSortStr = "资源使用次数";
                mTypeSortOrder = !mTypeSortOrder;
            }
            if (GUILayout.Button(mUseSortStr, GUILayout.Width(WIDTH))) {
                SortWithUseCount(mUseSortOrder);
                mUseSortStr = "资源使用次数" + (mUseSortOrder ? UP : DOWN);
                mTypeSortStr = "资源类型";
                mUseSortOrder = !mUseSortOrder;
            }
            GUILayout.EndHorizontal();
            mScrollPos = GUILayout.BeginScrollView(mScrollPos, GUILayout.Width(WIDTH * 3 + 30));
            //列表显示内容
            for (int i = 0; i < mResourceList.Count; i++) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(mResourceList[i].ResourceName, GUILayout.Width(WIDTH));
                GUILayout.Label(mResourceList[i].ResourceType, GUILayout.Width(WIDTH));
                GUILayout.Label(mResourceList[i].ResourceUseCount.ToString(), GUILayout.Width(WIDTH));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            //提供清理方法
            if (GUILayout.Button("清空扫描记录")) {
                ClearResourceList();
            }
        }

		//展示扫描结果
		if (mInActiveList.Count > 0 && mInActiveList.Count < 50) {
			GUILayout.Label("扫描结果（最多显示50条）");
			GUILayout.BeginHorizontal();
			GUILayout.Label("对象名称", GUILayout.Width(WIDTH));
			GUILayout.Label("对象路径", GUILayout.Width(WIDTH));
			GUILayout.EndHorizontal();
			//列表显示内容
			mScrollPos = GUILayout.BeginScrollView(mScrollPos, GUILayout.Width(WIDTH * 3 + 30));
			//列表显示内容
			for (int i = 0; i < mInActiveList.Count; i++) {
				GUILayout.BeginHorizontal();
				GUILayout.Label(mInActiveList[i].inActiveObject.name,GUILayout.Width(WIDTH));
				GUILayout.Label(mInActiveList[i].path);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			//提供清理方法
			if (GUILayout.Button("清空扫描记录")) {
				ClearResourceList();
			}
		}


    }

    //从文件中获取所有需要扫描的场景名字
    private string[] GetNameOfFilesToScan(string filePath) {
        string[] result = null;
        using (TextReader textReader = File.OpenText(filePath)) {
            result = textReader.ReadToEnd().Split('\n');
        }
        return result;
    }

    //清理
    private void ClearResourceList() {
        mResourceDict.Clear();
        mResourceList.Clear();
        mTypeSortStr = "资源类型";
        mUseSortStr = "资源使用次数";
        mUseSortOrder = true;
        mTypeSortOrder = true;
		mInActiveList.Clear ();
    }
		
    //添加资源
    private void AddResource(Object resource, string name, string type, string sceneName) {
        if (resource == null)
            return;
        //获取资源地址作为key值
        string path = AssetDatabase.GetAssetPath(resource);
        if (mResourceDict.ContainsKey(path)) {
            ResourceHolder resourceHolder = mResourceDict[path];
            resourceHolder.ResourceUseCount++;
            //对于场景名，目前看来似乎没有需要全部都显示的必要，所以先不做了
        } else {
            ResourceHolder resourceHolder = new ResourceHolder {
                ResourceObject = resource,
                ResourcePath = path,
                ResourceName = name,
                ResourceType = type,
                ResourceUseCount = 1,
                UseSceneName = sceneName
            };
            mResourceDict.Add(path, resourceHolder);
            mResourceList.Add(resourceHolder);
        }
    }


    //根据资源类型排序
    private void SortWithType(bool sortOrder = true) {
        if(sortOrder)
            mResourceList.Sort((ResourceHolder resource1, ResourceHolder resource2) =>
                resource1.ResourceType.CompareTo(resource2.ResourceType)
            );
        else
            mResourceList.Sort((ResourceHolder resource1, ResourceHolder resource2) =>
                resource2.ResourceType.CompareTo(resource1.ResourceType)
            );
    }

    //根据资源使用次数排序
    private void SortWithUseCount(bool sortOrder = true) {
        if(sortOrder)
            mResourceList.Sort((ResourceHolder resource1, ResourceHolder resource2) =>
                resource1.ResourceUseCount - resource2.ResourceUseCount
            );
        else
            mResourceList.Sort((ResourceHolder resource1, ResourceHolder resource2) =>
                resource2.ResourceUseCount - resource1.ResourceUseCount
            );
    }
		
	//构建资源列表
	private void BuildResourceList (Scene targetScene)
	{
		//拿到当前scene的所有GameObject
		GameObject[] sceneObjects = (GameObject[])targetScene.GetRootGameObjects ();

		//遍历当前场景所有GameObject
		for (int i = 0; i < sceneObjects.Length; i++) {

			//Renderer
			Renderer[] renderers = sceneObjects [i].GetComponentsInChildren<Renderer> (true);
			for (int j = 0; j < renderers.Length; j++) {
				//material
				if (renderers [j].materials != null) {
					Material[] rendererMaterials = renderers [j].materials;
					for (int k = 0; k < rendererMaterials.Length; k++) {
						AddResource (rendererMaterials [k], rendererMaterials [k].name, MATERIAL, targetScene.name);
						AddMaterialTextureResource (rendererMaterials [k], targetScene);
					}
				}
			}


			//AudioSource
			AudioSource[] audioSources = sceneObjects [i].GetComponentsInChildren<AudioSource> (true);
			for (int j = 0; j < audioSources.Length; j++) {
				//audioclip
				if (audioSources [j].clip != null) {
					AddResource (audioSources [j].clip, audioSources [j].clip.name, AUDIO_CLIP, targetScene.name);
				}
			}

			//Image
			Image[] images = sceneObjects [i].GetComponentsInChildren<Image> (true);
			for (int j = 0; j < images.Length; j++) {
				//sprite
				if (images [j].sprite != null) {
					AddResource (images [j].sprite, images [j].sprite.name, SPRITE, targetScene.name);
				}
				//material
				if (images [j].material != null) {
					AddResource (images [j].material, images [j].material.name, MATERIAL, targetScene.name);
					AddMaterialTextureResource (images [j].material, targetScene);
				}
			}

			//Text
			Text[] texts = sceneObjects [i].GetComponentsInChildren<Text> (true);
			for (int j = 0; j < texts.Length; j++) {
				//font
				if (texts [j].font != null) {
					AddResource (texts [j].font, texts [j].font.name, FONT, targetScene.name);
				}
				//material
				if (texts [j].material != null) {
					AddResource (texts [j].material, texts [j].material.name, MATERIAL, targetScene.name);
					AddMaterialTextureResource (texts [j].material, targetScene);
				}
			}

			//Mesh Collider
			MeshCollider[] meshColliders = sceneObjects [i].GetComponentsInChildren<MeshCollider> (true);
			for (int j = 0; j < meshColliders.Length; j++) {
				//mesh
				if (meshColliders [j].sharedMesh != null) {
					AddResource (meshColliders [j].sharedMesh, meshColliders [j].sharedMesh.name, MESH, targetScene.name);
				}
			}

			//Mesh Filter
			MeshFilter[] meshFilters = sceneObjects [i].GetComponentsInChildren<MeshFilter> (true);
			for (int j = 0; j < meshFilters.Length; j++) {
				//mesh
				if (meshFilters [j].mesh != null) {
					AddResource (meshFilters [j].mesh, meshFilters [j].mesh.name, MESH, targetScene.name);
				}
			}

			//Animation
			Animation[] animations = sceneObjects[i].GetComponentsInChildren<Animation>(true);
			for (int j = 0; j < animations.Length; j++) {
				//Animation Clip
				AddAnimationClipResource(animations[j], targetScene);
			}

			//Animator
			Animator[] animators = sceneObjects[i].GetComponentsInChildren<Animator>(true);
			for (int j = 0; j < animators.Length; j++) {
				//Runtime Animator Controller
				if (animators [j].runtimeAnimatorController != null) {
					AddResource (animators [j].runtimeAnimatorController, animators [j].runtimeAnimatorController.name,
						RUNTIME_ANIMATOR_CONTROLLER, targetScene.name);
				}
				//Avator
				if (animators [j].avatar != null) {
					AddResource (animators [j].avatar, animators [j].avatar.name, AVATOR, targetScene.name);
				}
			}

		}

	}

	//构建资源列表 场景中未激活的gameobject
	private void BuildInActiveResourceList(Scene targetScene){
		GameObject[] allSceneGameObject = (GameObject[]) targetScene.GetRootGameObjects ();
		List<GameObject> inActiveGameObject = new List<GameObject> ();
		for (int i = 0; i < allSceneGameObject.Length; i++) {
			FindInActive (allSceneGameObject [i],allSceneGameObject [i].name);
		}
	}

    //文件保存和加载
    //保存文件到目标名字，会对文件进行编号，对于已经存在的文件，会迭代编号创建新文件
    private bool SaveToFile(string folderPath, string fileName) {
        int count = 1;
        string partPath = folderPath + fileName + "_";
        string path = partPath;
        do {
            path = partPath + count.ToString() + ".csv";
        } while (File.Exists(path));
        using (TextWriter textWriter = File.CreateText(path)) {
            string text = EncodeResourceList();
            textWriter.Write(text);
        }
        return true;
    }
    //直接保存到指定文件
    private bool SaveToFile(string filePath) {
        using (TextWriter textWriter = File.CreateText(filePath)) {
                string text = EncodeResourceList();
                textWriter.Write(text);
            }
        return true;
    }
    //从指定文件中加载内容
    private bool LoadFromFile(string filePath) {
        ClearResourceList();
        using (TextReader textReader = File.OpenText(filePath)) {
            DecodeResourceList(textReader.ReadToEnd());
        }
        return true;
    }

    //数据编码解码
    private string EncodeResourceList() {
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        for(int i = 0; i < mResourceList.Count; i++) {
            stringBuilder.Append(mResourceList[i].ResourcePath).Append(',')
                .Append(mResourceList[i].ResourceName).Append(',')
                .Append(mResourceList[i].ResourceType).Append(',')
                .Append(mResourceList[i].ResourceUseCount).Append('\n');
        }
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
    }

    private void DecodeResourceList(string text) {
        string[] allResourceStrs = text.Split('\n');
        for(int i = 0; i < allResourceStrs.Length; i++) {
            string[] resourceStr = allResourceStrs[i].Split(',');
            if(resourceStr.Length == 4) {
                ResourceHolder resourceHolder = new ResourceHolder() {
                    ResourcePath = resourceStr[0],
                    ResourceName = resourceStr[1],
                    ResourceType = resourceStr[2],
                    ResourceUseCount = int.Parse(resourceStr[3])
                };
                mResourceDict.Add(resourceHolder.ResourcePath, resourceHolder);
                mResourceList.Add(resourceHolder);
            }
        }
    }

    //保存资源相关信息
    public class ResourceHolder {
        //资源引用
        public Object ResourceObject;
        public string ResourcePath;
        public string ResourceName;
        public string ResourceType;
        public int ResourceUseCount;
        public string UseSceneName;
    }

	//保存未激活的gameobject信息
	public class InActiveHolder
	{
		public GameObject inActiveObject;
		public string path;
	}


	private T1[] GetResourceFromComponent<T1>(Object mat) where T1 : Object{
		Object[] roots = new Object[]{ mat };
		Object[] dependObjs = EditorUtility.CollectDependencies (roots);
		List<T1> results = new List<T1> ();
		for (int i = 0; i < dependObjs.Length; i++) {
			if (dependObjs [i].GetType () == typeof(T1)) {
				results.Add ((T1)dependObjs [i]);
			}
		}
		return results.ToArray ();
	}

	private void AddMaterialTextureResource(Material mat, Scene targetScene){
		Texture2D[] textures = GetResourceFromComponent<Texture2D> (mat);
		if (textures != null) {
			for (int i = 0; i < textures.Length; i++) {
				AddResource (textures [i], textures [i].name, TEXTURE2D, targetScene.name);
			}
		}
	}

	private void AddAnimationClipResource(Animation anim, Scene targetScene){
		AnimationClip[] clips = GetResourceFromComponent<AnimationClip> (anim);
		if (clips != null) {
			for (int i = 0; i < clips.Length; i++) {
				AddResource (clips [i], clips [i].name, ANIMATION_CLIP, targetScene.name);
			}
		}
	}

	private void FindInActive(GameObject root, string gameobjectPath){
		if (root.activeSelf == false) {
			mInActiveList.Add (new InActiveHolder (){ inActiveObject = root, path = gameobjectPath });
		} else if (root.transform.childCount != 0) {
			foreach (Transform child in root.transform) {
				FindInActive (child.gameObject, gameobjectPath + '/' + child.name);
			}
		}
	}
}
