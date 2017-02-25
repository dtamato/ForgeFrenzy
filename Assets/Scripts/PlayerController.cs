﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	[Header("Parameters")]
	[SerializeField] int inputID;
	[SerializeField] float moveSpeed = 0.1f;

	[Header("References")]
	[SerializeField] Transform carryPosition;

	Player player;
	Rigidbody rb;
	GameObject pickedUpObject;
	GameObject stationFound;


	void Awake () {

		player = ReInput.players.GetPlayer(inputID);
		rb = this.GetComponent<Rigidbody>();
	}

	void Update () {

		ProcessMovement();
		ProcessActions ();
	}

	void ProcessMovement () {

		float horizontalAxis = player.GetAxis("Horizontal");
		float verticalAxis = player.GetAxis("Vertical");

		if(horizontalAxis != 0 || verticalAxis != 0) {

			float angleFacing = Mathf.Atan2 (player.GetAxis ("Horizontal"), player.GetAxis ("Vertical")) * Mathf.Rad2Deg;
			this.transform.rotation = Quaternion.Euler (0, angleFacing, 0);

			rb.MovePosition(this.transform.position + moveSpeed * new Vector3(horizontalAxis, 0, verticalAxis) * Time.deltaTime);
		}
	}

	void ProcessActions () {

		if (player.GetButtonDown ("Pickup")) {
			
			if (pickedUpObject == null) {

				FindPickup ();
			}
			else if (pickedUpObject != null) {
				
				FindStationToPlace ();
			}
		}
		else if (player.GetButtonDown ("Action")) {

			if (pickedUpObject == null && stationFound == null) {

				FindStationToActOn ();
			}
		}
		else if (player.GetButton ("Action")) {

			if (stationFound && pickedUpObject == null) {

				if (stationFound.CompareTag ("Forge")) {

					stationFound.GetComponent<Forge> ().ForgeWeapon ();
				}
				else if (stationFound.CompareTag ("Anvil")) {

					stationFound.GetComponent<Anvil> ().Hammer ();
				}
				else if (stationFound.CompareTag ("Woodworks")) {

					stationFound.GetComponent<Woodworks> ().ChopWood ();
				}
			}
		}
		else if (player.GetButtonUp ("Action")) {

			if (stationFound) {

				if (stationFound.CompareTag ("Forge")) {

					stationFound.GetComponent<Forge> ().ResetForgingBar ();
				}
				else if (stationFound.CompareTag ("Woodworks")) {

					stationFound.GetComponent<Woodworks> ().ResetWoodCuttingBar ();
					stationFound.GetComponent<Woodworks> ().RemovePlayerCutting ();
				}
			}
		}
	}

	void FindStationToPlace () {

		Collider[] stationsFound = GetCollidersInFront ();
		bool stationFound = false;

		if(stationsFound.Length > 0) {

			foreach (Collider collider in stationsFound) {
				
				if (collider.CompareTag ("Forge")) {

					stationFound = true;
					collider.GetComponent<Forge> ().UpdateMetalAmount (1);
					Destroy (pickedUpObject.gameObject);
					pickedUpObject = null;
				}
				else if(collider.CompareTag("Anvil") && collider.GetComponent<Anvil>().GetPlacedObject() == null) {

					stationFound = true;
					collider.GetComponent<Anvil> ().PlaceObject (pickedUpObject);
					pickedUpObject = null;
				}
			}
		}

		if(!stationFound || stationsFound.Length == 0) {

			DropItem();
		}
	}

	void DropItem () {
		
		pickedUpObject.transform.parent = null;
		pickedUpObject.GetComponent<Collider>().enabled = true;
		pickedUpObject.GetComponent<Rigidbody>().isKinematic = false;
		pickedUpObject.transform.rotation = Quaternion.Euler(0, 180, 0);
		pickedUpObject = null;
	}

	void FindPickup () {

		Collider[] collidersFound = GetCollidersInFront ();
		//Debug.Log ("Pickups Found: " + collidersFound.Length);
		foreach (Collider collider in collidersFound) {
			//Debug.Log("Pickup found: " + collider.name);
			if(collider.transform.CompareTag("Pickup")) {
				
				pickedUpObject = collider.gameObject;
				pickedUpObject.transform.SetParent(carryPosition);
				pickedUpObject.transform.localPosition = Vector3.zero;
				pickedUpObject.transform.localRotation = Quaternion.identity;
				pickedUpObject.GetComponent<Collider>().enabled = false;
				pickedUpObject.GetComponent<Rigidbody>().isKinematic = true;
				if (pickedUpObject.GetComponent<BrokenWeapon> ()) { pickedUpObject.GetComponent<BrokenWeapon>().enabled = false; }
				break;
			}
		}
	}

	void FindStationToActOn () {

		Collider[] stationsFound = GetCollidersInFront ();

		if(stationsFound.Length > 0) {

			foreach (Collider collider in stationsFound) {

				if (collider.CompareTag ("Forge") || collider.CompareTag ("Anvil")) {

					stationFound = collider.gameObject;
				}
				else if (collider.CompareTag ("Woodworks")) {

					stationFound = collider.gameObject;
					stationFound.GetComponent<Woodworks> ().SetPlayerCutting (this.gameObject);
				}
				else if (collider.CompareTag ("Tannery")) {

					stationFound = collider.gameObject;
					stationFound.GetComponent<Tannery> ().PlaceLeather ();
				}
			}
		}
	}

	Collider[] GetCollidersInFront () {

		Collider[] collidersFound = Physics.OverlapSphere(this.transform.position + 0.5f * this.transform.forward, 0.5f);
		//Debug.Log("stationsFound: " + collidersFound.Length);
		return collidersFound;
	}

	public void ReceiveItem (GameObject item) {

		pickedUpObject = item;
		pickedUpObject.transform.SetParent (carryPosition);
		pickedUpObject.transform.localPosition = Vector3.zero;
		pickedUpObject.transform.localRotation = Quaternion.identity;
		pickedUpObject.GetComponent<Collider> ().enabled = false;
	}
}