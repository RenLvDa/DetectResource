using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FindInActiveGameObject : EditorWindow {

	[MenuItem("UI/FindInActiveGameObject")]
	static void Init() {
		FindInActiveGameObject window = (FindInActiveGameObject)EditorWindow.GetWindow(typeof(FindInActiveGameObject));
		window.Show();
	}

	//保存的资源列表
	private Dictionary<string, ResourceHolder> mResourceDict = new Dictionary<string, ResourceHolder>();
	private List<ResourceHolder> mResourceList = new List<ResourceHolder>();
	public class ResourceHolder {
		//资源引用
		public Object ResourceObject;
		public string ResourcePath;
		public string ResourceName;
		public string ResourceType;
		public int ResourceUseCount;
	}
	//场景的默认文件路径
	private string mSceneFolderPath = "02.scene/";
	//保存需要扫描场景名字的列表文件
	private string mSceneNameListFilePath = "D:/workspace/sceneList.txt";
	//用于保存的文件地址
	private string mSaveFilePath = "D:/workspace/saceFile.txt";
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

	void OnGUI(){
		if (GUILayout.Button ("Scan")) {
			Scene nowScene = EditorSceneManager.GetActiveScene();
			BuildInActiveResourceList(nowScene);
		}

		//展示扫描结果
		if (mResourceList.Count > 0 && mResourceList.Count < 50) {
			GUILayout.Label("扫描结果（最多显示50条）");
			//表头
			GUILayout.BeginHorizontal();
			GUILayout.Label("资源名称", GUILayout.Width(WIDTH));
			if(GUILayout.Button(mTypeSortStr, GUILayout.Width(WIDTH))) {
				SortWithType(mTypeSortOrder);
				mTypeSortStr = "资源类型" + (mTypeSortOrder ? UP : DOWN);
				mTypeSortOrder = !mTypeSortOrder;
			}
			if(GUILayout.Button(mUseSortStr, GUILayout.Width(WIDTH))) {
				SortWithUseCount(mUseSortOrder);
				mUseSortStr = "资源使用次数" + (mUseSortOrder ? UP : DOWN);
				mUseSortOrder = !mUseSortOrder;
			}
			GUILayout.EndHorizontal();
			mScrollPos = GUILayout.BeginScrollView(mScrollPos, GUILayout.Width(WIDTH * 3 + 30));
			//列表显示内容
			for (int i = 0; i < mResourceList.Count; i++) {
				GUILayout.BeginHorizontal();
				GUILayout.Label(mResourceList[i].ResourceName,GUILayout.Width(WIDTH));
				GUILayout.Label(mResourceList[i].ResourceType, GUILayout.Width(WIDTH));
				GUILayout.Label(mResourceList[i].ResourceUseCount.ToString(), GUILayout.Width(WIDTH));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			//提供清理方法
			if (GUILayout.Button("Clear")) {
				Clear();
			}
		}
	}

	private void BuildInActiveResourceList(Scene targetScene){
		GameObject[] allSceneGameObject = (GameObject[]) targetScene.GetRootGameObjects ();
		List<GameObject> inActiveGameObject = new List<GameObject> ();
		for (int i = 0; i < allSceneGameObject.Length; i++) {
			FindInActive (allSceneGameObject [i]);
		}
	}

	private void AddResource(Object resource, string name){
		if (resource == null)
			return;
		//获取资源地址作为key值
		string path = AssetDatabase.GetAssetPath(resource);
		if (mResourceDict.ContainsKey(path)) {
			mResourceDict[path].ResourceUseCount++;
		} else {
			ResourceHolder resourceHolder = new ResourceHolder {
				ResourceObject = resource,
				ResourcePath = path,
				ResourceName = name,
				ResourceUseCount = 1
			};
			mResourceDict.Add(path, resourceHolder);
			mResourceList.Add(resourceHolder);
		}
	}

	private void FindInActive(GameObject root){
		if (root.activeSelf == false) {
			
		} else if (root.transform.childCount != 0) {
			foreach (Transform child in root.transform) {
				FindInActive (child.gameObject);
			}
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

	//清理
	private void Clear() {
		mResourceDict.Clear();
		mResourceList.Clear();
	}
}
