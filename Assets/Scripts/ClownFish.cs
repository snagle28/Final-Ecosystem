using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ClownFish: MonoBehaviour
{
    private int daysAlive = 0;

    private int Lifetime;
    //ROTATION STUFF
    private float rotationSpeed = 180f;
    private bool autoRotate = false;
    

    Transform target = null; //current spot we're moving towards

    //count of current lerp progress
    float lerpTime;
    
    
    public float degrees;
    private float hideTimer = 0;
    private float stareTimer = 0;
    private float maxHideTime = 900;
    private float maxStareTime = 100;
    private bool atTurnAngle = false;
    private bool atSwimAngle = true;
    private bool savedRotation = false;
    private bool readyToStare = false;
    private Quaternion beginningRotation;

    [SerializeField] private ParticleSystem deathBurstPrefab;

    //enum is like a custom variable type
    //we're using it to make states for our spider's behavior
    enum FishStates
    {
        swimming,
        turning,
        looking,
        returning,
        dying
    }

    //current state
    FishStates state = FishStates.swimming;

    //timer that'll count down for hunger
    float hungerTime;
    //hunger stat
    float hungerVal = 5;

    //list for food currently in the scene
    List<GameObject> allFood = new List<GameObject>();

    //holds which game object the spider has touched
    GameObject touchingObj;

    private Vector3 startPos;

    void Start()
    {
        FindAllFood(); //find all food objs in the scene
        startPos = transform.position;
        Lifetime = Random.Range(3000, 3500);

    }

    void Update()
    {
        print(state.ToString());
        daysAlive++;
        if (daysAlive >= Lifetime)
        {
            state = FishStates.dying;
        }
        hideTimer++;
       
        switch (state)
        {
            case FishStates.swimming: //if we're in the idle state
                Swim(); //run idle code
                break;
            case FishStates.turning:
                hideTimer = 0;//if we're in the eating state
                TurnToFood(); //run eating code
                break;
            case FishStates.looking:
                Look();
                break;
            case FishStates.returning:
                TurnBack();
                break;
            case FishStates.dying:
                Die();
                break;
            default:
                break;
        }
    }

    
    [SerializeField] private float range;
    [SerializeField] private float swimSpeed;
    private bool movingRight = true;
    
    void Swim()
    {
        bool facingRight = transform.localScale.x > 0;
        Vector3 targetPos;
        hideTimer += Time.deltaTime;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        
        Vector3 scale = transform.localScale;
        scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        //set target based on whether or not the fish is moving right
        if (movingRight)
        {
            targetPos = startPos + Vector3.right * range;
            renderer.sortingOrder = 6;
        }
        else
        {
            targetPos = startPos + Vector3.left * range;
            renderer.sortingOrder = -1;
        }

        // Vector3 velocity = Vector3.zero;
        //
        // transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.01f);
        
        float step = swimSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            movingRight = !movingRight; //instead of setting to false or true, just switch
        }
        
       

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        
        if (hideTimer >= maxHideTime)
        {
            beginningRotation = transform.rotation;
            if (GameObject.FindGameObjectWithTag("food"))
            {
                state = FishStates.turning;
            }
        }
    }

    void TurnToFood()
    {
        bool facingRight = transform.localScale.x > 0;
        //bool facingLeft = transform.localScale.x < 0;
        Vector3 targetFood = FindNearest(allFood).transform.position;
        //Vector3 velocity = Vector3.zero;
        
        // //get direction of nearest fish
        Vector3 direction = targetFood - transform.position;

        //flip based on direction of food
        if (targetFood.x < transform.position.x && facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x); 
            transform.localScale = scale;
            
        }
        else if (targetFood.x > transform.position.x && !facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
            
        }
        
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        if (transform.localScale.x < 0)
        {
            targetRotation *= Quaternion.Euler(0, 0, 180);
        }
        
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            180f * Time.deltaTime); //180f is speed, just did one frame per degree



        // transform.rotation = Quaternion.Euler(0, 0, angle-180);
        // // //stop rotating once you get close enough
        //
        if (Quaternion.Angle(transform.rotation, targetRotation) <= 1f)
        {
            transform.localRotation = Quaternion.Euler(0f,0f,targetRotation.eulerAngles.z);
            stareTimer = 0;
            
            //turn BACK 
            state = FishStates.looking;
        }


    }

    void Look()
    {
        //INCREMENT STARE TIMER/////
        stareTimer++;
        if (stareTimer >= maxStareTime)
        { ;
           state = FishStates.returning;
        }
    }

    void TurnBack()
    {
        
        //turn back if needed//
        bool facingRight = transform.localScale.x > 0;
        Vector3 targetFood = FindNearest(allFood).transform.position;
        
        //reset stare timer for next circuit
        stareTimer = 0;
        
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            beginningRotation,
            rotationSpeed * Time.deltaTime);
        
        if (Quaternion.Angle(transform.rotation, beginningRotation) <= 1f)
        {
            transform.rotation = beginningRotation;
            state = FishStates.swimming;
            hideTimer = 0;
        }
        
    }

    //vars for fade out
    private SpriteRenderer sr;
    [SerializeField] private float fadeInDuration = 1.5f;
    private float fadeTimer = 0f;
    void Die()
    {
        if (deathBurstPrefab != null)
        {
            Instantiate(deathBurstPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    

    void FindAllFood(){
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food")); //find all objs tagged food and put them in a list
    }

    Transform FindNearest(List<GameObject> objsToFind){
        float minDist = Mathf.Infinity; //setting the min dist to a big number
        Transform nearest = null; //tracks the obj closest to us
        for(int i = 0; i < objsToFind.Count; i++){ //loop through the objects we're checking
            float dist = Vector3.Distance(transform.position, objsToFind[i].transform.position); //check the dist b/t the spider and the current obj
            if(dist < minDist){ //if the dist is less than our currently tracked min dist
                minDist = dist; //set the min dist to the new dist
                nearest = objsToFind[i].transform; //set the nearest obj var to this obj
            }
        }
        return nearest; //return the closest obj
    }

    

    private bool touchingTrigger;
    void OnTriggerEnter2D(Collider2D col){
        if(col != null) touchingObj = col.gameObject; //if we touch something, set the var to whatever that thing is
        if (col.CompareTag("hideTrigger"))
        {
            touchingTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col != null) { //if we stop touching something 
            if(col.gameObject == touchingObj) touchingObj = null; //AND that thing is being tracked, clear the touching tracking var
        }
        if (col.CompareTag("hideTrigger"))
        {
            touchingTrigger = false;
        }
    }
    
}
