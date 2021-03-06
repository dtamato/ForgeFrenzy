﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Materials { AxeMetal, SwordMetal, Wood, Leather }

[DisallowMultipleComponent]
public class Workbench : MonoBehaviour {

	[Header("Crafting")]
	[SerializeField] float maxCraftingTime = 5;
	[SerializeField] GameObject craftFillBar;
	[SerializeField] Image craftFill;
	[SerializeField] AudioClip craftingAudioClip;
	[SerializeField] AudioClip craftSuccessAudioClip;
	[SerializeField] AudioClip craftFailAudioClip;

	[Header("References")]
	[SerializeField] GameObject finishedSwordPrefab;
	[SerializeField] GameObject finishedAxePrefab;
	[SerializeField] GameObject finishedShieldPrefab;
	[SerializeField] GameObject failedCraft;
	[SerializeField] AudioClip placeAudioClip;
	[SerializeField] GameObject xButtonGameobject;
	[SerializeField] GameObject aButtonGameobject;
	[SerializeField] ParticleSystem particleSystem;

	AudioSource audioSource;
	List<GameObject> placedGameObjects = new List<GameObject>();
	List<Materials> placedMaterials = new List<Materials>();
	float craftingTimer;
	GameObject playerCrafting;

	void Awake () {

		audioSource = this.GetComponent<AudioSource>();
		ResetCraftingBar();
	}

	void OnTriggerEnter (Collider other) {

		if (other.CompareTag ("Player")) {
			
			if(other.GetComponent<PlayerController>().HasObject()) {

				aButtonGameobject.SetActive(true);
			}
		}
	}

	void OnTriggerExit(Collider other) {

		if (other.CompareTag ("Player")) {

			aButtonGameobject.SetActive(false);
			xButtonGameobject.SetActive(false);
		}
	}

	public void SetPlayerCrafting(GameObject player) {

		playerCrafting = player;
	}

	public void RemovePlayerCrafting () {

		playerCrafting = null;
	}

	public void AddObject (GameObject newObject) {
		//Debug.Log("Added " + newObject.name + " to workbench");
		// Orient object on workbench
		newObject.transform.SetParent (this.transform);
		newObject.transform.localPosition = new Vector3(0.1f, 0.6f, 0);
		newObject.transform.localRotation = Quaternion.Euler(76, -90, 0);
		//newObject.GetComponent<Collider> ().enabled = false;

		// Add to list of objects on workbench
		placedGameObjects.Add(newObject);
		if(newObject.name.Contains("Axe Metal")) {

			placedMaterials.Add(Materials.AxeMetal);
		}
		else if(newObject.name.Contains("Sword Metal")) {

			placedMaterials.Add(Materials.SwordMetal);
		}
		else if(newObject.name.Contains("Chopped Wood")) {

			placedMaterials.Add(Materials.Wood);
		}
		else if(newObject.name.Contains("Tanned Leather")) {

			placedMaterials.Add(Materials.Leather);
		}

		aButtonGameobject.SetActive(false);
		if(placedGameObjects.Count > 1) { xButtonGameobject.SetActive(true); }
		audioSource.clip = placeAudioClip;
		audioSource.Play();
	}

	public void RemoveObject (PlayerController player) {

		if (placedGameObjects.Count > 0) {

			player.ReceiveItem (placedGameObjects [placedGameObjects.Count - 1]);
			placedGameObjects.RemoveAt (placedGameObjects.Count - 1);
			if(placedMaterials.Count > placedGameObjects.Count) { placedMaterials.RemoveAt(placedMaterials.Count - 1); }
		}
	}

	public void CraftWeapon () {

		if (placedGameObjects.Count > 1) {

			if (craftingTimer == 0) {

				for(int i = 0; i < placedGameObjects.Count; i++) { placedGameObjects[i].GetComponent<SpriteRenderer>().enabled = false; }
				craftFillBar.SetActive (true);
				particleSystem.Play();
			}

			if (craftingTimer < maxCraftingTime) {

				craftingTimer += Time.deltaTime;
				craftFill.fillAmount = (craftingTimer / maxCraftingTime);
				if(audioSource.isPlaying == false) { audioSource.PlayOneShot(craftingAudioClip); }
			}
			else if (craftingTimer >= maxCraftingTime) {

				ResetCraftingBar ();
				GameObject weaponPrefab = DetermineWeaponCreated ();
				foreach (GameObject placedObject in placedGameObjects) { Destroy (placedObject.gameObject); }
				GameObject newWeapon = Instantiate (weaponPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				//playerCrafting.GetComponent<PlayerController> ().ReceiveItem (newWeapon);
				newWeapon.transform.SetParent(this.transform);
				newWeapon.transform.localPosition = new Vector3(0.22f, 0.67f, 0);
				newWeapon.transform.localRotation = Quaternion.Euler(15, -90, 0);

				placedGameObjects = new List<GameObject> ();
				placedMaterials = new List<Materials> ();
				placedGameObjects.Add (newWeapon);

				playerCrafting.GetComponentInChildren<Animator>().SetBool("Work", false);
			}
		}
	}

	GameObject DetermineWeaponCreated () {

		GameObject newWeapon = null;

		if(placedMaterials.Count == 2) {

			if(placedMaterials.Contains(Materials.AxeMetal) && placedMaterials.Contains(Materials.Wood)) {
				
				newWeapon = finishedAxePrefab;
				audioSource.PlayOneShot(craftSuccessAudioClip);
			}
			else if(placedMaterials.Contains(Materials.SwordMetal) && placedMaterials.Contains(Materials.Leather)) {
				
				newWeapon = finishedSwordPrefab;
				audioSource.PlayOneShot(craftSuccessAudioClip);
			}
			else if(placedMaterials.Contains(Materials.Leather) && placedMaterials.Contains(Materials.Wood)) {
				
				newWeapon = finishedShieldPrefab;
				audioSource.PlayOneShot(craftSuccessAudioClip);
			}
			else {

				newWeapon = failedCraft;
				audioSource.PlayOneShot(craftFailAudioClip);
			}
		}
		else {

			newWeapon = failedCraft;
			audioSource.PlayOneShot(craftFailAudioClip);
		}

		return newWeapon;
	}

	public void ResetCraftingBar () {

		craftingTimer = 0;
		craftFill.fillAmount = 0;
		craftFillBar.SetActive(false);
		xButtonGameobject.SetActive(false);
		aButtonGameobject.SetActive(false);
		particleSystem.Stop();
	}

	public int GetPlacedObjectsCount () {

		return placedGameObjects.Count;
	}
}
