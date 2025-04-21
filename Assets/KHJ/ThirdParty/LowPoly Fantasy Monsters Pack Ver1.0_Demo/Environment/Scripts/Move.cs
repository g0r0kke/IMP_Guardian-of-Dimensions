using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour {

    private float move = 20;
    private bool stop = false;    
    private float blend;
    private float delay = 0;
    public float AddRunSpeed = 1;
    public float AddWalkSpeed = 1;
    private bool hasAniComp = false;


    // Use this for initialization
    void Start () 
    {
        if ( null != GetComponent<Animation>() )
        {
            hasAniComp = true;
        }
    }

    void MoveActive ()
    { 
        float speed = 0.0f;
        float add = 0.0f;

        if ( hasAniComp == true )
        {    
            if ( Keyboard.current.upArrowKey.isPressed )
            {      
                move *= 1.015F;

                if ( move > 250 && CheckAniClip( "run" ) == true )
                {
                    {
                        GetComponent<Animation>().CrossFade("run");
                        add = 20 * AddRunSpeed;
                    }
                }
                else
                {
                    GetComponent<Animation>().Play("walk");
                    add = 5 * AddWalkSpeed;
                }

                speed = Time.deltaTime * add;

                transform.Translate( 0, 0, speed );
            }

            if ( Keyboard.current.upArrowKey.wasReleasedThisFrame )
            {
                if ( GetComponent<Animation>().IsPlaying("walk") )
                {    GetComponent<Animation>().CrossFade("idle01", 0.3f); }
                if ( GetComponent<Animation>().IsPlaying("run") )
                {    
                    GetComponent<Animation>().CrossFade("idle01", 0.5f);
                    stop = true;
                }    
                move = 20;
            }

            if (stop == true)
            {    
                float max = Time.deltaTime * 20 * AddRunSpeed;
                blend = Mathf.Lerp(max, 0, delay);

                if ( blend > 0 )
                {    
                    transform.Translate( 0, 0, blend );
                    delay += 0.025f; 
                }    
                else 
                {    
                    stop = false;
                    delay = 0.0f;
                }
            }
        }
        else
        {
            if ( Keyboard.current.upArrowKey.isPressed )
            {      
                add = 5 * AddWalkSpeed;
                speed = Time.deltaTime * add;
                transform.Translate( 0, 0, speed );
            }
        }
    }

    bool CheckAniClip ( string clipname )
    {    
        if( this.GetComponent<Animation>().GetClip(clipname) == null ) 
            return false;
        else if ( this.GetComponent<Animation>().GetClip(clipname) != null ) 
            return true;

        return false;
    }

    // Update is called once per frame
    void Update () 
    {
        MoveActive();

        if ( hasAniComp == true )
        {    
            if (Keyboard.current.vKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "damage away" ) == false ) return;

                GetComponent<Animation>().CrossFade("damage away", 0.2f);
                GetComponent<Animation>().CrossFadeQueued("idle01");
            } 

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "dead away" ) == false ) return;

                GetComponent<Animation>().CrossFade("dead away", 0.2f);
            } 

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "attack03" ) == false ) return;

                GetComponent<Animation>().CrossFade("attack03", 0.2f);
                GetComponent<Animation>().CrossFadeQueued("idle01");
            } 

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "attack01" ) == false ) return;

                GetComponent<Animation>().CrossFade("attack01", 0.2f);
                GetComponent<Animation>().CrossFadeQueued("idle01");
            }

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "attack02" ) == false ) return;

                GetComponent<Animation>().CrossFade("attack02", 0.2f);
                GetComponent<Animation>().CrossFadeQueued("idle01");
            }

            if (Keyboard.current.aKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "drop down" ) == false ) return;

                GetComponent<Animation>().CrossFade("drop down", 0.2f);
            }

            if (Keyboard.current.zKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "sit up" ) == false ) return;

                GetComponent<Animation>().CrossFade("sit up", 0.2f);
                GetComponent<Animation>().CrossFadeQueued("idle01");
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "damage" ) == false ) return;

                GetComponent<Animation>().CrossFade("damage", 0.1f);
                GetComponent<Animation>().CrossFadeQueued("idle01");            
            }

            if (Keyboard.current.xKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "dead" ) == false ) return;

                GetComponent<Animation>().CrossFade("dead", 0.1f);
            }            

            if (Keyboard.current.dKey.wasPressedThisFrame)
            {    
                if ( CheckAniClip( "idle02" ) == false ) return;

                GetComponent<Animation>().CrossFade("idle02", 0.1f);
                GetComponent<Animation>().CrossFadeQueued("idle01");            
            }    
                                
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            transform.Rotate(0.0f, Time.deltaTime * -100.0f, 0.0f);
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            transform.Rotate(0.0f, Time.deltaTime * 100.0f, 0.0f);
        }
    }
}