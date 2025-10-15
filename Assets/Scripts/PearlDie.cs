using UnityEngine;

public class PearlDie : MonoBehaviour
{
    //reference to scriptable object
    public GameObject deathParticles;
    public GameObject clamPrefab;

    
    private int timer = 0;
    // Update is called once per frame
    void Update()
    {
        timer++;
        if (timer > 1000)
        {
                Instantiate(deathParticles, transform.position, transform.rotation);
                var psRenderer = deathParticles.GetComponent<ParticleSystemRenderer>();

                if (psRenderer != null)
                {
                    psRenderer.sortingOrder = 6;
                }
                
                if (clamPrefab != null)
                {
                    Vector3 spawnPos = new Vector3(Random.Range(-5, 1),(float)-4.22,0);
                    Instantiate(clamPrefab, spawnPos, Quaternion.identity);
                }
                Destroy(gameObject);
            
        }

        
    }
}

