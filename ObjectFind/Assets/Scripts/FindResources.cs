using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class FindResources : MonoBehaviour
{

	enum ResourceType
	{
		material      			  = 0,
		texture2D     			  = 1,
		audioClip     			  = 2,
		sprite        			  = 3,
		font          			  = 4,
		mesh          			  = 5,
		animationClip 			  = 6,
		runtimeAnimatorController = 7,
		avator					  = 8,
	}


	void Awake ()
	{
		Scene scene = SceneManager.GetActiveScene ();
		GameObject[] objs = (GameObject[])scene.GetRootGameObjects ();

		BuildResourceList (SceneManager.GetActiveScene ());
			
	}


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
						AddResource (rendererMaterials [k], rendererMaterials [j].name, ResourceType.material);
						AddMaterialTextureResource (rendererMaterials [k]);
					}
				}
			}


			//AudioSource
			AudioSource[] audioSources = sceneObjects [i].GetComponentsInChildren<AudioSource> (true);
			for (int j = 0; j < audioSources.Length; j++) {
				//audioclip
				if (audioSources [j].clip != null) {
					AddResource (audioSources [j].clip, audioSources [j].clip.name, ResourceType.audioClip);
				}
			}

			//Image
			Image[] images = sceneObjects [i].GetComponentsInChildren<Image> (true);
			for (int j = 0; j < images.Length; j++) {
				//sprite
				if (images [j].sprite != null) {
					AddResource (images [j].sprite, images [j].sprite.name, ResourceType.sprite);
				}
				//material
				if (images [j].material != null) {
					AddResource (images [j].material, images [j].material.name, ResourceType.material);
					AddMaterialTextureResource (images [j].material);
				}
			}

			//Text
			Text[] texts = sceneObjects [i].GetComponentsInChildren<Text> (true);
			for (int j = 0; j < texts.Length; j++) {
				//font
				if (texts [j].font != null) {
					AddResource (texts [j].font, texts [j].font.name, ResourceType.font);
				}
				//material
				if (texts [j].material != null) {
					AddResource (texts [j].material, texts [j].material.name, ResourceType.material);
					AddMaterialTextureResource (texts [j].material);
				}
			}

			//Mesh Collider
			MeshCollider[] meshColliders = sceneObjects [i].GetComponentsInChildren<MeshCollider> (true);
			for (int j = 0; j < meshColliders.Length; j++) {
				//mesh
				if (meshColliders [j].sharedMesh != null) {
					AddResource (meshColliders [j].sharedMesh, meshColliders [j].sharedMesh.name, ResourceType.mesh);
				}
			}

			//Mesh Filter
			MeshFilter[] meshFilters = sceneObjects [i].GetComponentsInChildren<MeshFilter> (true);
			for (int j = 0; j < meshFilters.Length; j++) {
				//mesh
				if (meshFilters [j].mesh != null) {
					AddResource (meshFilters [j].mesh, meshFilters [j].mesh.name, ResourceType.mesh);
				}
			}

			//Animation
			Animation[] animations = sceneObjects[i].GetComponentsInChildren<Animation>(true);
			for (int j = 0; j < animations.Length; j++) {
				//Animation Clip
				AddAnimationClipResource(animations[j]);
			}
				
			//Animator
			Animator[] animators = sceneObjects[i].GetComponentsInChildren<Animator>(true);
			for (int j = 0; j < animators.Length; j++) {
				//Runtime Animator Controller
				if (animators [j].runtimeAnimatorController != null) {
					AddResource (animators [j].runtimeAnimatorController, animators [j].runtimeAnimatorController.name,
						ResourceType.runtimeAnimatorController);
				}
				//Avator
				if (animators [j].avatar != null) {
					AddResource (animators [j].avatar, animators [j].avatar.name, ResourceType.avator);
				}
			}

		}
			
	}

//	private Texture2D[] GetMaterialTexture(Object mat){
//		Object[] roots = new Object[]{ mat };
//		Object[] dependObjs = EditorUtility.CollectDependencies (roots);
//		List<Texture2D> results = new List<Texture2D> ();
//		for (int i = 0; i < dependObjs.Length; i++) {
//			if (dependObjs [i].GetType () == typeof(Texture2D)) {
//				results.Add ((Texture2D)dependObjs [i]);
//			}
//		}
//		return results.ToArray ();
//	}

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

	private void AddMaterialTextureResource(Material mat){
		Texture2D[] textures = GetResourceFromComponent<Texture2D> (mat);
		if (textures != null) {
			for (int i = 0; i < textures.Length; i++) {
				AddResource (textures [i], textures [i].name, ResourceType.texture2D);
			}
		}
	}

	private void AddAnimationClipResource(Animation anim){
		AnimationClip[] clips = GetResourceFromComponent<AnimationClip> (anim);
		if (clips != null) {
			for (int i = 0; i < clips.Length; i++) {
				AddResource (clips [i], clips [i].name, ResourceType.animationClip);
			}
		}
	}

	private void AddResource (Object resource, string name, ResourceType type)
	{
		//TODO
		Debug.Log ("Object " + resource + " Name " + name + " Type " + type); 
	}

}
