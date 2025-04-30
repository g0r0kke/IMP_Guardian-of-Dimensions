using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Animator animator;

    private bool isPlayed = false;
    private float anmationTime = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(isPlayed)
        {
            if(anmationTime > 0.6f)
            {
                animator.SetBool("isBasicAttack", false);
                animator.SetBool("isUltimateAttack", false);
                animator.SetBool("isHealing", false);
                animator.SetBool("isDefense", false);
                animator.SetBool("isVictory", false);
                animator.SetBool("isDie", false);
                animator.SetBool("isRetry", false);
                isPlayed = false;
                anmationTime = 0;
            }
            else
            {
                anmationTime += Time.deltaTime;
            }
        }
    }

    // Basic attack animation
    public void BasicAttackAnimation()
    {
        animator.SetBool("isBasicAttack", true);
        isPlayed = true;
    }

    // Ultimate attack animation
    public void UltimateAttackAnimation()
    {
        animator.SetBool("isUltimateAttack", true);
        isPlayed = true;
    }

    //Healing animation
    public void HealingAnimation()
    {
        animator.SetBool("isHealing", true);
        isPlayed = true;
    }

    //Defense animation
    public void DefenseAnimation()
    {
        animator.SetBool("isDefense", true);
        isPlayed = true;
    }

    //victory animation
    public void VictoryAnimation()
    {
        animator.SetBool("isBasicAttack", false);
        animator.SetBool("isUltimateAttack", false);
        animator.SetBool("isHealing", false);
        animator.SetBool("isDefense", false);
        animator.SetBool("isVictory", false);
        animator.SetBool("isVictory", true);
        animator.SetBool("isDie", false);
        animator.SetBool("isRetry", false);
        isPlayed = true;
    }

    //die animation
    public void DieAnimation()
    {
        animator.SetBool("isBasicAttack", false);
        animator.SetBool("isUltimateAttack", false);
        animator.SetBool("isHealing", false);
        animator.SetBool("isDefense", false);
        animator.SetBool("isVictory", false);
        animator.SetBool("isDie", true);
        animator.SetBool("isRetry", false);
        isPlayed = true;
    }

    //retry animation
    public void RetryAnimation()
    {
        animator.SetBool("isBasicAttack", false);
        animator.SetBool("isUltimateAttack", false);
        animator.SetBool("isHealing", false);
        animator.SetBool("isDefense", false);
        animator.SetBool("isVictory", false);
        animator.SetBool("isDie", false);
        animator.SetBool("isRetry", true);
        isPlayed = true;
    }
}
