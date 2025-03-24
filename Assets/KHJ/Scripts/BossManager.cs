using Azmodan.Phase1;
using Azmodan.Phase2;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private BossPhase1 phase1Prefab;
    [SerializeField] private BossPhase2 phase2Prefab;
    
    private Boss currentBoss;
    
    public void StartPhase1()
    {
        if (currentBoss != null) Destroy(currentBoss.gameObject);
        currentBoss = Instantiate(phase1Prefab, transform.position, Quaternion.identity);
    }
    
    public void TransitionToPhase2()
    {
        Vector3 position = currentBoss.transform.position;
        Destroy(currentBoss.gameObject);
        currentBoss = Instantiate(phase2Prefab, position, Quaternion.identity);
    }
}
