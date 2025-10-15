using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

public class LittleFish : MonoBehaviour
{
    
    public Sprite[] possibleSprites;
    public List<Transform> Waypoints = new List<Transform>();
    private Vector3 targetPos;
    private SpriteRenderer sr;
    private bool canSpawn = true;
    private float spawnOffset;

    private float spawnDelay;
    private float spawnTimer = 0f;
    
    //vars for moving and turning:
    private bool has_target = false;
    private bool isTurning;
    
    //variable for current waypoint
    private Vector3 wayPoint;
    private Vector3 lastWayPoint = new Vector3 (0f,0f,0f);
    private ClamGuy clamScript;
    private GameObject foundClam;

    private int lookTimer = 0;
    
    public List<littleEgg> eggList = new List<littleEgg>();


    private int Lifetime;
    private int daysAlive = 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (possibleSprites.Length > 0 && sr != null)
        {
            Sprite chosenSprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
            sr.sprite = chosenSprite;
        }
        
    }
    
    enum FishyStates
    {
        birth,
        swimming,
        looking,
        dying
    }
    FishyStates state = FishyStates.birth;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Lifetime = Random.Range(5000, 7000);
        targetPos = getTargetPos();
        
        foundClam = GameObject.FindGameObjectWithTag("clam");
        if(foundClam != null)
        {
            clamScript = foundClam.GetComponent<ClamGuy>();
        }

        if (Waypoints.Count == 0 || Waypoints == null)
        {
            Waypoints = new List<Transform>();
            GameObject[] waypoints = GameObject.FindGameObjectsWithTag("waypoint");
            foreach (GameObject waypoint in waypoints)
            {
                Waypoints.Add(waypoint.transform);
            }
        }

        spawnOffset = Random.Range(-50f, 50f);
        spawnDelay = Random.Range(0.5f, 3f);
        spawnTimer = Random.Range(0f, spawnDelay);

    }

    // Update is called once per frame
    void Update()
    {
        ClamGuy cScript = GetClamScript();
        // print(state);
        if (daysAlive >= Lifetime)
        {
            state = FishyStates.dying;
        }
        daysAlive++;
        
        switch(state)
        {
            
            case FishyStates.birth: 
                SwimUp();
                break;
            case FishyStates.swimming:
                SwimAround();
                if (cScript != null && daysAlive > Random.Range(300,500) && cScript.oxygenLevels > 200 && canSpawn)
                {
                    spawnTimer += Time.deltaTime;
                    if (spawnTimer >= spawnDelay)
                    {
                        print("trying to give birth");
                        canSpawn = false;
                        SpawnEgg();
                    }
                    
                }
                break;
            case FishyStates.looking:
                if (cScript != null && cScript.droppedPearlObj != null)
                {
                    lookTowardsPearl(cScript.droppedPearlObj.transform);
                }
                else
                {
                    // keep counting so the fish eventually resets
                    lookTimer++;

                    if (lookTimer >= 500)
                    {
                        lookTimer = 0;
                        state = FishyStates.swimming;
                    }
                }
                break;
            case FishyStates.dying:
                Destroy(gameObject);
                break;
        }
    }

    private bool movingRight = true;

    [SerializeField]
    private float swimSpeed;
    void SwimUp()
    {
        /****************/
        //FACE THE RIGHT POSITION//
        
        bool facingRight = transform.localScale.x > 0;
        
        Vector3 direction = targetPos - transform.position;
        
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, sr.flipX?angle-180:angle); //might need to shift
        if (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180 * Time.deltaTime);
        }
        if (angle > 90&&!sr.flipX){
             sr.flipX=true;
             transform.rotation = transform.rotation * Quaternion.Euler(0, 0, 180);
        }
        
        float step = swimSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        
        
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            state = FishyStates.swimming;
        }

    }

    void SwimAround()
    {
        if (!has_target)
        {
            has_target = FindTarget();
        }
        else
        {
            rotateFish(wayPoint);
            transform.position = Vector3.MoveTowards(transform.position, wayPoint, swimSpeed * Time.deltaTime);
        }

        if (transform.position == wayPoint)
        {
            has_target = false;
        }
        
        if (clamScript != null && clamScript.droppedPearlObj !=null)
        {
            print("sees a pearl");
            
            // if (!hasLooked && foundClam)
            // {
            //     state = FishyStates.looking; //do i need to reset this
            // }
            state = FishyStates.looking;
        }
        else
        {
            hasLooked = false; 
        }
        
    }
    
    
    //set range of locations for the baby fish
    [SerializeField]
    Transform topleftLimit, bottomrightLimit;

    Vector3 getTargetPos()
    {
        Vector3 targetPos = new Vector3(Random.Range(topleftLimit.position.x, bottomrightLimit.position.x),
            Random.Range(bottomrightLimit.position.y, bottomrightLimit.position.y));
        return targetPos;
    }

    public Vector3 getRandomWaypoint()
    {
        int randomIndex = Random.Range(0, Waypoints.Count-1);
        Vector3 randomWaypoint = Waypoints[randomIndex].transform.position;
        return randomWaypoint;
    }

    //private float m_speed;
    bool FindTarget(float start = 1f, float end = 7f)
    {
        wayPoint = getRandomWaypoint();
        if (lastWayPoint == wayPoint)
        {
            wayPoint = getRandomWaypoint();
            return false;
        }
        else
        {
            lastWayPoint = wayPoint;
            //m_speed = Random.Range(start, end);
            return true;
        }
    }
    
    bool rotateFish(Vector3 targetLocation) //come back to this, needs to be able to rotate to the right if needed
    {
        bool facingRight = transform.localScale.x > 0;
        
        Vector3 direction = targetLocation- transform.position;
        
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, sr.flipX?angle-180:angle); //might need to shift
        if (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180 * Time.deltaTime);
            sr.flipX = (targetLocation.x < transform.position.x);
            return false;
        }
        else
        {
            return true;
        }
    
        //cheking whether or not the x sprite needs to flip. 
        //this matters because if it does we need to change the angle (thanks Daniel lol)
        
        
    }

    private Quaternion savedRotation;
    private bool hasLooked = false;
    
    
    //NEW VERSION:
    /*
     * originally i saved rotation after the fish had already begun to rotate
     * when the fish tried to rotate back it pretty much just jiggled
     * now rotation gets saved at the right time (ONLY ONCE)
     * two clear mini-states/phases can't overlap 
     */
    void lookTowardsPearl(Transform pearlTransform)
    {
        //first look at pearl
        lookTimer++;
        if (lookTimer < 250)
        {
            rotateFish(pearlTransform.position);
            if (lookTimer == 1)
            {
                savedRotation = transform.rotation; //save rotation once
            }
        }
        else if (lookTimer < 500)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, savedRotation, 180 * Time.deltaTime);
        }
        else
        {
            lookTimer = 0;
            sr.flipX = savedRotation.eulerAngles.z > 90 && savedRotation.eulerAngles.z < 270;

            
            hasLooked = true;
            state = FishyStates.swimming;
        }
        
        
    }
    
    [SerializeField] private Vector3 eggScale;
    public GameObject eggPrefab;
        
    littleEgg DecideType()
    {
        int randomNumber = Random.Range(1, 101);
        List<littleEgg> viableEggs = new List<littleEgg>();

        foreach (littleEgg egg in eggList)
        {
            if (randomNumber <= egg.dropChance)
            {
                viableEggs.Add(egg);
            }
        }

        if (viableEggs.Count > 0)
        {
            return viableEggs[Random.Range(0, viableEggs.Count)];
        }
        else
        {
            return null;
        }
    }

    void SpawnEgg()
    {
        littleEgg spawnedEgg = DecideType();
        if (spawnedEgg != null)
        {
            GameObject eggObj = Instantiate(eggPrefab, transform.position, transform.rotation);
            eggObj.GetComponent<SpriteRenderer>().sprite = spawnedEgg.eggSprite;
            eggObj.GetComponent<SpriteRenderer>().transform.localScale = eggScale;
            eggObj.GetComponent<SpriteRenderer>().sortingOrder = 7;
            
        }
    }
    
    ClamGuy GetClamScript(){
        if (clamScript == null)
        {
            // Find the clam by tag
            GameObject c = GameObject.FindWithTag("clam");
            if (c != null)
            {
                clamScript = c.GetComponent<ClamGuy>();
                if (clamScript == null)
                    print("Found clam object but no ClamGuy component attached!");
            }
            else
            {
                // print("Can't find any clam object with tag 'clam'");
            }
        }

        // Reset if clam is inactive
        if (clamScript != null && !clamScript.gameObject.activeInHierarchy)
        {
            clamScript = null;
        }

        return clamScript;
    }
    
    
    
    
}
