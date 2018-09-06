using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class Test : MonoBehaviour {
	enum ResourceType
	{
		material      = 0,
		texture2D     = 1,
		audioClip     = 2,
		sprite        = 3,
		font          = 4,
		mesh          = 5,
		animationClip = 6,
	}
	// Use this for initialization
	void Start () {
		Object[] roots = new Object[]{ transform };
		Object[] dependObjs = EditorUtility.CollectDependencies (roots);
		for (int k = 0; k < dependObjs.Length; k++) {
			if (dependObjs [k].GetType () == typeof(Material)) {
				AddResource (dependObjs [k], dependObjs [k].name, ResourceType.material);
			}
			if (dependObjs [k].GetType () == typeof(Font)) {
				AddResource (dependObjs [k], dependObjs [k].name, ResourceType.font);
			}
			if (dependObjs [k].GetType () == typeof(AnimationClip)) {
				AddResource (dependObjs [k], dependObjs [k].name, ResourceType.animationClip);
			}
			if (dependObjs [k].GetType () == typeof(AudioClip)) {
				AddResource (dependObjs [k], dependObjs [k].name, ResourceType.audioClip);
			}
			if (dependObjs [k].GetType () == typeof(Texture2D)) {
				AddResource (dependObjs [k], dependObjs [k].name, ResourceType.texture2D);
			}
		}
	}

	private void AddResource (Object resource, string name, ResourceType type)
	{
		//TODO
		Debug.Log ("Object " + resource + " Name " + name + " Type " + type); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}


}
