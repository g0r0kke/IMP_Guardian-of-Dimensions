using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleParticles : MonoBehaviour {
    void Update () {
        var ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = transform.lossyScale.magnitude;
    }
}
