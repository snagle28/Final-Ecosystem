using UnityEngine;

public class ClamTop : MonoBehaviour
{
    private Animation anim;
    private Animator animator;
    
    
    void Start()
    {
        anim = GetComponent<Animation>();
        animator = GetComponent<Animator>();
        // animator.SetBool("ReadyToOpen", false);
        // animator.SetBool("ReadyToClose", false);
    }
   
}
