using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {

    private List<DemonController> _demons = new List<DemonController>();

    void Awake()
    {
    }

    void OnDestroy()
    {
        _demons.Clear();
    }

    void Start()
    {
        var demonsInScene = GameObject.FindObjectsOfType<DemonController>();
        _demons.Clear();

        _demons.AddRange(demonsInScene);
    }

    public void AddDemon(DemonController demon)
    {
        if(_demons.Contains(demon) == false)
            _demons.Add(demon);
    }

    public void RemoveDemon(DemonController demon)
    {
        if (_demons.Contains(demon) == true)
            _demons.Remove(demon);
    }

    public List<DemonController> FindDemonsInCone( Vector3 position, Vector3 direction, float angleInDegrees, float distance )
    {
        var demons = new List<DemonController>();
        for (int i = 0; i < _demons.Count; i++)
        {
            var toDemon = _demons [i].transform.position - position;
            if (toDemon.magnitude > distance)
                continue;

            if (Vector3.Dot(toDemon, direction) <= Mathf.Cos(0.5f * Mathf.Deg2Rad * angleInDegrees))
                continue;

            demons.Add(_demons [i]);
        }

        return demons;
    }
}
