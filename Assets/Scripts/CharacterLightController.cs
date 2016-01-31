using UnityEngine;
using System.Collections;

public class CharacterLightController : MonoBehaviour {
	
	[SerializeField]
	private Light _lightSource;
	
	[SerializeField]
	private float _startingLightTime = 30.0f;

    [SerializeField]
    private float _maximumLightTime = 30.0f;

	[SerializeField]
	private Renderer _characterRenderer;

	[SerializeField]
	private int _materialIndex = 0;

	[Header("Spot Light SEtting")]
	[SerializeField]
	private float _lightAngle = 90.0f;
	private float _minAngle = 15.0f;

	[Header("Point Light Setting")]
	[SerializeField]
	private float _lightRange = 15.0f;
	
	private float _lightTimer = 0;
	private Material _characterLightMaterialInstance;
	private Color _originalColor;
	
	void Start()
	{
		GameEvents.PickupReceived += HandlePickupReceived;
        GameEvents.PlayerDamaged += OnDamage;

		_lightTimer = _startingLightTime;

		if (_characterRenderer) {
			var material = _characterRenderer.materials[_materialIndex];
			_characterLightMaterialInstance = new Material (material);
			_originalColor = _characterLightMaterialInstance.color;
			_characterRenderer.materials[_materialIndex] = _characterLightMaterialInstance;
		}
	}
        
	void OnDestroy()
	{
        GameEvents.PlayerDamaged -= OnDamage;
		GameEvents.PickupReceived -= HandlePickupReceived;
	}

	void HandlePickupReceived (Pickup pickup)
	{
		if (pickup is LightPickup) {
			AddLight( ((LightPickup)pickup).LightTime );
		}
	}

	public void AddLight( float lightTime )
	{
        _lightTimer = Mathf.Min (lightTime + _lightTimer, _maximumLightTime);
	}
	
	public void OnDamage( float lightTime )
	{
		_lightTimer = Mathf.Max (0, _lightTimer - lightTime);
	}
	
	void Update()
	{
		_lightTimer -= Time.deltaTime;
        _lightTimer = Mathf.Clamp(_lightTimer, 0, _maximumLightTime);

		if (_lightTimer <= 0) {
			GameEvents.TriggerLightDepleted( this.gameObject );
		}
	}
	
	void LateUpdate()
	{
        float lightT = Mathf.Max(0, _lightTimer / _startingLightTime);

		if (_lightSource) {
			if( _lightSource.type == LightType.Spot )
			{
				float diff = _lightAngle - _minAngle;
				_lightSource.spotAngle = _minAngle + diff * lightT;
			}
			else if(_lightSource.type == LightType.Point)
				_lightSource.range = _lightRange * lightT;
		}

		if (_characterLightMaterialInstance) {

			var c = _originalColor;
			c.r = lightT * c.r;
			c.g = lightT * c.g;
			c.b = lightT * c.b;
	
			_characterLightMaterialInstance.color = c;
			_characterRenderer.materials[_materialIndex] = _characterLightMaterialInstance;
		
		}
	}

    void OnGUI()
    {
        //GUILayout.Label( string.Format("Light Time: {0:0.00}", _lightTimer) );
    }
}

