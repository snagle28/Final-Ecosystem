using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ClamGuy : MonoBehaviour
{

    public GameObject clamManager;
    //vars for fade in
    private SpriteRenderer sr;
    [SerializeField] private float fadeInDuration = 1.5f;
    private float fadeTimer = 0f;
    
    
    
    [SerializeField] private GameObject myself;
    [SerializeField] private GameObject deathParticles;

    public double oxygenLevels;
    
    private Animation anim;
    private Animator animator;
    public GameObject pearlPrefab;
    public List<Pearl> pearlList = new List<Pearl>();

    public GameObject droppedPearlObj; //allows other scripts to acess ref
    public GameObject myselfPrefab;
    
    private bubbleManager bubbleManager;
    
    
    enum ClamStates
    {
        born,
        idle,
        opening,
        closing,
        dying
    }

    //current state
    private ClamStates state = ClamStates.born;
    private Vector3 startPos;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        startPos = transform.position;
        anim = GetComponent<Animation>();
        animator = GetComponent<Animator>();
        animator.SetBool("ReadyToOpen", false);
        animator.SetBool("ReadyToClose", false);
        oxygenLevels = oxygenMin;
        
        
        //fade in shit
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f; //set alpha
        sr.color = c;
        
        bubbleManager = GameObject.Find("clamBubbles").GetComponent<bubbleManager>();
        bubbleManager.moveBubble(transform);
        //get PS
        bubbles = GameObject.Find("clamBubbles")?.GetComponent<ParticleSystem>();
        if (bubbles == null)
        {
            Debug.Log("bubbles not found");
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        print(oxygenLevels);
        updateOxlevels();
        //booleans for controlling the animation:
        //note that this has to be constantly checked so it goes in update
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        switch (state)
        {
            case ClamStates.born:
                Born();
                break;
            case ClamStates.idle:
                //be idle;
                ClamIdle();
                break;
            case ClamStates.opening:
                animator.SetBool("ReadyToOpen", true);
                if (stateInfo.IsName("clamOpen") && stateInfo.normalizedTime >= 1.5f)
                {
                    //print(supposed to spawn)
                    SpawnPearl();
                    state = ClamStates.closing;
                }
                
                break;
            case ClamStates.closing:
                //close
                if (stateInfo.IsName("clamOpen") && stateInfo.normalizedTime >= 1.5f)
                {
                    animator.SetBool("ReadyToClose", true);
                }

                if (stateInfo.IsName("clamClose") && stateInfo.normalizedTime >= 1.5f)
                {   
                    animator.SetBool("ReadyToOpen", false);
                    animator.SetBool("ReadyToClose", false);
                    state = ClamStates.dying;
                    
                }
                break;
            case ClamStates.dying:
                //die;
                die();
                break;
        }
        
        
    }

    private int shakeDir;
    
    //deal with oscillations
    [SerializeField] private float shakeAmplitude;
    [SerializeField] private float shakeAmpMax;
    [SerializeField] private float shakeSpeed;
    
    //deal with randomness and pauses
    private float distFromSP;
    private int shakeTimer = 0;
    private int shakeTimerMax;
    private int shakeCount;
    [SerializeField] int shakeCountMax;

    private bool pauseShake = false;

    private ParticleSystem bubbles;
    
    
    void ClamIdle()
    {
        //https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Mathf.Sin.html

        //first arg is speed, second is distance. we plug in time to get constantly changing sin(x)
        //remember that dist represents distance from the origin point based on speed and amp
        distFromSP = Mathf.Sin(Time.time * shakeSpeed) * shakeAmplitude;
        transform.position = startPos + new Vector3(distFromSP, 0, 0);
        
        //need to decrease shakeAmplitude. multiply by time.deltatime 
        if (!pauseShake)
        {
            //clear particles if they're there
            if ( bubbles.isPlaying == true)
            {
                bubbles.Stop();
            }
            //increment amplitude of clam shake
            if (shakeAmplitude > 0)
            {
                shakeAmplitude-= .5f *  Time.deltaTime;
            }
            //otherwise ensure variables are reset and pause movement
            else
            {
                transform.position = startPos;
                pauseShake = true;
                //normally produce bubbles
            }
        }

        if (pauseShake)
        {
            
            
            //set range of length in between shakes
            shakeTimerMax = Random.Range(1500,1900);
           
            transform.position = startPos;
            shakeAmplitude = shakeAmpMax;

            if (bubbles.isPlaying == false) 
            {
                bubbles.Play(); 
            }
            else if (!bubbles.isPlaying)
            {
                bubbles.Play();
            }

            if (bubbles.isPlaying == true)
            {
                shakeTimer++;
                if (shakeTimer >= shakeTimerMax)
                {
                    shakeCount += 1;
                    shakeTimer = 0;
                    pauseShake = false;
                }
            }
            
            //NO BREAK
            
        }

       
        
        
        
        if (shakeCount >= shakeCountMax)
        {
            bubbles.Stop();
            state = ClamStates.opening;
            shakeCount = 0;
        }
       

    }

    [SerializeField] private Vector3 pearlScale;

    void SpawnPearl()
    {
        Pearl droppedPearl = DecideType();
        if (droppedPearl != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, -3.46f, transform.position.z);
            //print(spawnPosition);
            GameObject pearlObj = Instantiate(pearlPrefab, spawnPosition, transform.rotation);
            //now render it as the sprite dictated in the prefab
            pearlObj.GetComponent<SpriteRenderer>().sprite = droppedPearl.pearlSprite;
            pearlObj.GetComponent<SpriteRenderer>().transform.localScale = pearlScale;
            pearlObj.GetComponent<SpriteRenderer>().sortingOrder = 3;
            
            pearlObj.GetComponent<Rigidbody2D>().AddForce(transform.up * 2f, ForceMode2D.Impulse);
            pearlObj.GetComponent<Rigidbody2D>().AddForce(transform.right *1.5f, ForceMode2D.Impulse);
            droppedPearlObj = pearlObj;
        }
        else
        {
            print("pearl returned null");
        }
        
    }
    
    //We want to decide which pearl to drop based on the drop chance. to do this, we can use a 
    //for each loop
    //https://www.youtube.com/watch?v=Cco7Oq0_IqU 
    //just like for i in (python), learned that last sem
    
    Pearl DecideType()
    {
        int randomNumber = Random.Range(1, 101);
        List<Pearl> viablePearList = new List<Pearl>();
        
        //coding the 'drop chance' for each pearl goes here. We will iterate through each
        //potential pearl item. If that drop chance is viable (given the randomly selected randomNumber)
        //then we can consider it.
        foreach (Pearl item in pearlList)
        {
            //comparing random number to drop chance of prefab
            if (randomNumber <= item.dropChance)
            {
                viablePearList.Add(item);
                //now we have a list of viable items but need to pick just one
            }
        }
        
        //only return pearl if there is more than one item in the list:
        if (viablePearList.Count > 0)
        {
            return viablePearList[Random.Range(0, viablePearList.Count)];
        }
        else
        {
            return pearlList[0];
        }
    }


    private bool hasDied = false;
    void die()
    {
        if (hasDied) return;

        hasDied = true;
        Instantiate(deathParticles, transform.position, transform.rotation);
        var psRenderer = deathParticles.GetComponent<ParticleSystemRenderer>();

        if (psRenderer != null)
        {
            psRenderer.sortingOrder = 6;
        }
        Destroy(gameObject);
    }

    public double oxygenMin;
    public double oxygenMax;
    public float oxygenChangeSpeed = 30f;
    public float oxygenRandomness = 5f;
    
    public double updateOxlevels()
    { 
        
        float oxChange = oxygenChangeSpeed * Time.deltaTime;
        if (pauseShake)
        {
            //supposed to update oxygen
            oxygenLevels += oxChange;
        }
        else{ oxygenLevels -= (0.1 * oxChange);
        }
        
        oxygenLevels += Random.Range(-oxygenRandomness, oxygenRandomness) * Time.deltaTime;
        //use CASTING like in intro to CS (works bc its double to float)?
        oxygenLevels = Mathf.Clamp((float)oxygenLevels, (float)oxygenMin, (float)oxygenMax);
        
        return oxygenLevels; 
    }

    void Born()
    {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeInDuration); //ensures it smoothly goes up based ON fadeinduration (at that point alpha is 1)
            Color newC = sr.color;
            newC.a = alpha;
            sr.color = newC;
            if (alpha >= 1f) //once its faded in
            {
                state = ClamStates.idle;
            }
            
    }


}

//learning how to write classes! https://www.youtube.com/watch?v=KjvvRmG7PBM 


