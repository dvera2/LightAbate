using UnityEngine;
using System.Collections;

public abstract class Pickup : MonoBehaviour {

	void OnTriggerEnter( Collider collider )
	{
		if (!collider.CompareTag ("Player")) {
			return;
		}

		GameEvents.TriggerPickupReceived (this);

		Destroy (gameObject);
	}
}
