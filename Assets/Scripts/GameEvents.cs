using UnityEngine;
using System.Collections;


public class GameEvents
{
	public delegate void LightDepletedHandler( GameObject character );
	public static event LightDepletedHandler LightDepleted;
	public static void TriggerLightDepleted( GameObject character ) {
		if (LightDepleted != null)
			LightDepleted ( character );
	} 

	public delegate void PickupReceivedHandler( Pickup pickup );
	public static event PickupReceivedHandler PickupReceived;
	public static void TriggerPickupReceived( Pickup pickup ) {
		if (PickupReceived != null)
			PickupReceived (pickup);
	}
}