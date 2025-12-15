using UnityEngine;

public class WeaponPositioner : MonoBehaviour
{
    public Animator animator;
    public GameObject[] weapons; 
    public GameObject weaponHolder;
    public float transitionSpeed = 10f;

    public WeaponState idleState;
    public WeaponState walkState;
    public WeaponState runState;
    public WeaponState crouchState;
    public WeaponState crouchWalkState;
    public WeaponState slideState;
    public WeaponState jumpState;

    private WeaponState currentState;
    private WeaponState targetState;

    public bool hideOnSlide = true;
    public bool hideOnJump = true;

    [System.Serializable]
    public class WeaponState
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentState = idleState;
        targetState = idleState;
    }

    void Update()
    {
        if (animator == null) return;

        targetState = GetCurrentState();
        ApplyState();
    }

    WeaponState GetCurrentState()
    {
        bool isGrounded = animator.GetBool("IsGrounded");
        bool isRunning = animator.GetBool("IsRunning");
        bool isCrouching = animator.GetBool("IsCrouching");
        bool isSliding = animator.GetBool("IsSliding");
        bool crouchWalk = animator.GetBool("CrouchWalk");
        float speed = animator.GetFloat("Speed");

        if (isSliding)
            return slideState;

        if (!isGrounded)
            return jumpState;

        if (crouchWalk)
            return crouchWalkState;

        if (isCrouching)
            return crouchState;

        if (isRunning && speed > 0.1f)
            return runState;

        if (speed > 0.1f)
            return walkState;

        return idleState;
    }

    void ApplyState()
    {
        if (targetState == null) return;

        bool shouldHide = (targetState == slideState && hideOnSlide) ||
                          (targetState == jumpState && hideOnJump);

        if (weaponHolder != null)
        {
            weaponHolder.SetActive(!shouldHide);
        }

        if (!shouldHide && weapons != null)
        {
            foreach (GameObject weapon in weapons)
            {
                if (weapon == null) continue;

                weapon.transform.localPosition = Vector3.Lerp(
                    weapon.transform.localPosition,
                    targetState.position,
                    Time.deltaTime * transitionSpeed
                );

                Quaternion targetRot = Quaternion.Euler(targetState.rotation);
                weapon.transform.localRotation = Quaternion.Lerp(
                    weapon.transform.localRotation,
                    targetRot,
                    Time.deltaTime * transitionSpeed
                );
            }
        }
    }
}