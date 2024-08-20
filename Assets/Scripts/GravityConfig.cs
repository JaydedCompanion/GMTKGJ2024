using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityConfig : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {
        ParticleSystem.ExternalForcesModule extForce = GetComponent<ParticleSystem> ().externalForces;
        foreach (ParticleSystemForceField field in FindObjectsOfType<ParticleSystemForceField> ())
            if (field.gameObject != gameObject)
                extForce.AddInfluence (field);
    }

    // Update is called once per frame
    void Update () {

    }

}