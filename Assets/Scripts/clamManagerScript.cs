using UnityEngine;

public class clamManagerScript : MonoBehaviour
{
    public bool firstClam = true;
    public GameObject clamPrefab;
    public GameObject clownFishPrefab;
    private Transform leftLimit;
    private Transform rightLimit;
    private int clownTimer = 0;
    
    

    void Update()
    {
       
        if (GameObject.FindGameObjectWithTag("clownFish") == null)
        {
            clownTimer++;
            if(clownTimer >= 300)
            {
                Vector3 clownFishPos = new Vector3(Random.Range((float)5.99, (float)7.46), (float)-1.81, 0);
                GameObject newFish = Instantiate(clownFishPrefab, clownFishPos, Quaternion.identity);
                newFish.transform.localScale = new Vector3(Mathf.Abs(newFish.transform.localScale.x), newFish.transform.localScale.y, 1f);
                clownTimer = 0;
            }
            
        }
    }

    public void spawnClm()
    {
        Vector3 spawnPos = new Vector3(Random.Range(leftLimit.position.x, rightLimit.position.x), 0, 0);
        Instantiate(clamPrefab, spawnPos, Quaternion.identity);
    }
}
