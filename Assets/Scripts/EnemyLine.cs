﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLine : MonoBehaviour {

	[Header("Enemy Line Parameters")]
	[SerializeField] float startingZ = 33f;
	[SerializeField] float endZ = -1f;
	[SerializeField] float moveSpeed = 1;

	[Header("Enemy Army Composition")]
	[SerializeField] float enemyAxePercentage = 0.33f;
	[SerializeField] float enemySwordPercentage = 0.33f;
	[SerializeField] float enemyShieldPercentage = 0.33f;

	[Header("Ally Army Composition")]
	[SerializeField] float allyAxePercentage = 0.33f;
	[SerializeField] float allySwordPercentage = 0.33f;
	[SerializeField] float allyShieldPercentage = 0.33f;

	[Header("Spawn Parameters")]
	[SerializeField] float spawnCooldown = 2;

	[Header("References")]
	[SerializeField] GameObject brokenMetal;
	[SerializeField] GameObject brokenLeather;


	Vector3 endPosition;

	void Awake () {

		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, startingZ);
		endPosition = new Vector3(this.transform.position.x, this.transform.position.y, endZ);
		StartCoroutine (SpawnItem ());
		StartCoroutine(DecreaseAllyStrength());
	}

	void Update () {
		
		this.transform.position = Vector3.MoveTowards(this.transform.position, endPosition, moveSpeed * Time.deltaTime);
	}

	IEnumerator SpawnItem () {

		Bounds bounds = this.GetComponent<Collider> ().bounds;
		Vector3 spawnLocation = new Vector3 (Random.Range (bounds.min.x, bounds.max.x), this.transform.position.y, this.transform.position.z - 6);
		Instantiate (brokenMetal, spawnLocation, Quaternion.identity);

		yield return new WaitForSeconds (spawnCooldown);

		StartCoroutine (SpawnItem ());
	}

	void OnCollisionEnter (Collision other) {

		if (other.transform.CompareTag ("Pickup")) {

			Destroy (other.gameObject);
		}
	}

	void UpdateMoveSpeed () {

		float advantageAmount = 0;

		if(enemyShieldPercentage != 0) { advantageAmount += allyAxePercentage - enemyShieldPercentage; }
		if(enemyAxePercentage != 0) { advantageAmount += allySwordPercentage - enemyAxePercentage; }
		if(enemySwordPercentage != 0) { advantageAmount += allyShieldPercentage - enemySwordPercentage; }

		float disadvantageAmount = 0;

		if(enemyAxePercentage - allyShieldPercentage > 0) { disadvantageAmount += enemyAxePercentage - allyShieldPercentage; }
		if(enemySwordPercentage - allyAxePercentage > 0) { disadvantageAmount += enemySwordPercentage - allyAxePercentage; }
		if(enemyShieldPercentage - allySwordPercentage > 0) { disadvantageAmount += enemyShieldPercentage - allySwordPercentage; }

		float balanceFactor = 0.1f;
		moveSpeed += balanceFactor * (disadvantageAmount - advantageAmount);
	}

	public void IncrementAxeCount () {

		allyAxePercentage += 0.05f;
		UpdateMoveSpeed();
	}

	public void IncrementSwordCount () {

		allySwordPercentage += 0.05f;
		UpdateMoveSpeed();
	}

	public void IncrementShieldCount () {

		allyShieldPercentage += 0.05f;
		UpdateMoveSpeed();
	}

	IEnumerator DecreaseAllyStrength () {

		float decreaseAmount = Random.Range(0.01f, 0.03f);

		float weaponToDecrease = Random.value;

		if(weaponToDecrease < 0.33f && allyAxePercentage >= decreaseAmount) {

			allyAxePercentage -= decreaseAmount;
		}
		else if(weaponToDecrease < 0.66f && allySwordPercentage >= decreaseAmount) {

			allySwordPercentage -= decreaseAmount;
		}
		else if(allyShieldPercentage >= decreaseAmount) {

			allyShieldPercentage -= decreaseAmount;
		}

		yield return new WaitForSeconds(5);
		StartCoroutine(DecreaseAllyStrength());
	}
}
