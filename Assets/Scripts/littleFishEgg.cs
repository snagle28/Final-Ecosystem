using UnityEngine;

public class littleFishEgg : MonoBehaviour
{
    public GameObject deathParticles;
    public GameObject fishPrefab;
    private bool canSpawn = true;
    public Sprite[] possibleSprites;

    [SerializeField] int timer = 0;
    
    // Update is called once per frame
    void Update()
    {
        timer++;
        if(timer > 500 && canSpawn)
        {
            GameObject newFish = Instantiate(fishPrefab,transform.position,transform.rotation);
            Sprite chosenSprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
            SpriteRenderer spR = newFish.GetComponent<SpriteRenderer>();
            if (spR!= null)
            {
                spR.sprite = chosenSprite;
            }
            
            canSpawn = false;
            Destroy(gameObject);
        } 
    }
}
