using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float camSensitivity = 200.0f;
    public float maxLookAngle = 30.0f;
    public float movementSpeed = 10.0f;
    public float jumpHeight = 12.0f;
    public float doubleJumpHeight = 15.0f;
    public float tripleJumpHeight = 20.0f;
    public float superJumpHeight = 18.0f;

    private float fVel = 0.0f;
    private float velX = 0.0f;
    private float velZ = 0.0f;
    private float velY = 0.0f;
    private float intendedMag = 0.0f;
    private float intendedYaw = 0.0f;

    private float doubleJumpTimer = 0.0f;
    private float sideFlipTimer = 0.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private bool onGround = false;
    private bool animOver = false;

    private PlayerController controller;
    private PlayerAction action;
    private PlayerAction prevAction;

    public PointOfInterest playerPOI;
    public Transform meshTrans;
    private Transform POITrans;
    private CharacterController cc;
    public Animator anim;

    //Code to prevent snapback
    private Vector2 persitentInput;
    private const float snapBackCont = 50.0f;
    private const float terminalVel = -24.0f;

    private const float jumpMultiplier = .25f;
    private const float movementDeadZone = .1f * .1f;
    private const float doubleJumpTimerDecay = 1.0f;

    private void Awake()
    {
        controller = new PlayerController();
        cc = GetComponent<CharacterController>();
        
    }

    private void Start()
    {
        POITrans = playerPOI.transform;
    }

    private void OnEnable()
    {
        prevAction = PlayerAction.Idle;
        doubleJumpTimer = 0.0f;
        sideFlipTimer = 0.0f;
        controller.Enable();
    }

    private void OnDisable()
    {
        controller.Disable();
    }

    private void Update()
    {
        onGround = cc.isGrounded;
        doubleJumpTimer -= doubleJumpTimerDecay * Time.deltaTime;
        doubleJumpTimer = Mathf.Max(doubleJumpTimer, 0.0f);

        sideFlipTimer -= doubleJumpTimerDecay * Time.deltaTime;
        sideFlipTimer = Mathf.Max(sideFlipTimer, 0.0f);

        Vector2 look = controller.GamePlayer.Look.ReadValue<Vector2>();
        Vector2 move = controller.GamePlayer.Move.ReadValue<Vector2>();
        //Debug.Log(move);
        bool jumpButton = controller.GamePlayer.Jump.triggered;
        persitentInput = Vector2.Lerp(persitentInput, move, snapBackCont * Time.deltaTime);

        if (onGround && velY < 0.0f)
        {
            velY = 0.0f;
        }

        #region Camera region
        //temporarily inverted x-axis for my own preference
        yaw -= look.x * camSensitivity * Time.deltaTime;
        pitch -= look.y * camSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        POITrans.eulerAngles = new Vector3(pitch, yaw, 0);
        #endregion

        UpdateJoystickInput(persitentInput);
        switch (action)
        {
            case PlayerAction.Idle:
                IdleAction();
                break;
            case PlayerAction.Walking:
                WalkingAction();
                break;
            case PlayerAction.Decelerating:
                DecelerationAction();
                break;
            case PlayerAction.Crouching:
                CrouchingAction();
                break;
            case PlayerAction.TurningAround:
                TurningAction();
                break;

            case PlayerAction.Jump:
                JumpAction();
                break;
            case PlayerAction.DoubleJump:
                DoubleJumpAction();
                break;
            case PlayerAction.TripleJump:
                TripleJumpAction();
                break;
            case PlayerAction.SuperJump:
                SuperJumpAction();
                break;
            case PlayerAction.SideFlip:
                SideFlipAction();
                break;

            case PlayerAction.JumpLanding:
                JumpLandingAction();
                break;
            case PlayerAction.DoubleJumpLanding:
                DoubleJumpLandingAction();
                break;
            case PlayerAction.TripleJumpLanding:
                TripleJumpLandingAction();
                break;
            case PlayerAction.SuperJumpLanding:
                SuperJumpLandingAction();
                break;
            case PlayerAction.SideFlipLanding:
                SideFlipLandingAction();
                break;

            case PlayerAction.Dive:
                DiveAction();
                break;
            default:
                break;
        }
        //Debug.Log($"{action},{prevAction}");
        //Debug.Log(fVel);
        cc.Move(new Vector3(velX, 0, velZ) * Time.deltaTime);
        if (controller.GamePlayer.Jump.triggered && onGround)
        {
            SetPlayerYVelBasedOnSpeed(velY, jumpMultiplier);
        }
        velY += GamePhysics.gravity * Time.deltaTime;
        velY = Mathf.Max(velY, terminalVel);
        cc.Move(Time.deltaTime * velY * Vector3.up);

    }

    private void SetForwardVelocity(float fVel)
    {

        float rad = Mathf.Deg2Rad * meshTrans.eulerAngles.y;

        this.fVel = fVel;

        velX = Mathf.Sin(rad) * this.fVel;
        velZ = Mathf.Cos(rad) * this.fVel;

    }

    private void UpdateJoystickInput(Vector2 move)
    {
        float mag = move.sqrMagnitude;
        intendedMag = mag * movementSpeed;
        if (intendedMag < .1f)
        {
            intendedMag = 0;
        }


        if (mag > movementDeadZone)
        {
            intendedYaw = Mathf.Atan2(move.x, move.y) + (Mathf.Deg2Rad * POITrans.eulerAngles.y);
            intendedYaw *= Mathf.Rad2Deg;
        }
        else
        {
            intendedYaw = meshTrans.eulerAngles.y;
        }
    }

    private void SetPlayerYVelBasedOnSpeed(float intitialY, float multiplier)
    {
        velY = intitialY + fVel * multiplier;

    }

    private void SetAnimOver()
    {
        animOver = true;
    }

    private void SetPlayerAction(PlayerAction action)
    {
        animOver = false;
        switch (action)
        {
            case PlayerAction.TurningAround:
                break;

            case PlayerAction.Jump:
                SetPlayerYVelBasedOnSpeed(jumpHeight, .25f);
                fVel *= .8f;
                break;
            case PlayerAction.DoubleJump:
                SetPlayerYVelBasedOnSpeed(doubleJumpHeight, .25f);
                fVel *= .8f;
                break;
            case PlayerAction.TripleJump:
                SetPlayerYVelBasedOnSpeed(tripleJumpHeight, .25f);
                fVel *= .8f;
                break;
            case PlayerAction.SuperJump:
                SetPlayerYVelBasedOnSpeed(superJumpHeight, 0);
                break;
            case PlayerAction.SideFlip:
                SetPlayerYVelBasedOnSpeed(superJumpHeight, 0);
                fVel = movementSpeed / 4;
                meshTrans.eulerAngles = Vector3.up * intendedYaw;
                break;
            case PlayerAction.LongJump:
                SetPlayerYVelBasedOnSpeed(9.0f, 0);
                fVel *= 1.5f;
                fVel = Mathf.Clamp(fVel, -13.0f, 13.0f);
                break;
            case PlayerAction.Dive:
                SetForwardVelocity(12.0f);
                anim.SetTrigger("Dive");
                break;
            default:
                break;

        }

        prevAction = this.action;
        this.action = action;
    }

    private bool AnalogStickFlickedBack()
    {
        Quaternion intendedRotation = Quaternion.Euler(0, intendedYaw, 0);
        return Quaternion.Angle(intendedRotation, meshTrans.rotation) > 60.0f;
    }

    #region ground based

    private void UpdateWalkingSpeed()
    {
        float maxTargetSpeed;
        float target;
        Quaternion currentRotaion = meshTrans.rotation;
        Quaternion intendedRotation = Quaternion.Euler(0, intendedYaw, 0);
        maxTargetSpeed = movementSpeed;
        target = intendedMag < maxTargetSpeed ? intendedMag : maxTargetSpeed;

        if (fVel <= 0.0f)
        {
            fVel += 14.0f * Time.deltaTime;
        }
        else if (fVel <= target)
        {
            fVel += (14.0f - fVel / 43.0f) * Time.deltaTime;
        }
        else
        {
            fVel -= 12.0f * Time.deltaTime;
        }

        if (fVel > 48.0f)
        {
            fVel = 48.0f;
        }
        //meshTrans.eulerAngles = Vector3.up * (intendedYaw - GameMath.ApproachDelta(intendedYaw - (meshTrans.eulerAngles.y), 0, 377.5f, 377.5f));
        meshTrans.rotation = Quaternion.Slerp(currentRotaion, intendedRotation, 8f * Time.deltaTime);
        //Debug.Log($"Intended Yaw: {intendedYaw} Actual Yaw: {meshTrans.eulerAngles.y}");
        SetForwardVelocity(fVel);
    }

    //Leaving Currently unused because I don't have the brain power to focus on this at the moment.
    private void UpdateSliding()
    {
        float lossFactor;
        float accel;
        float oldSpeed;
        float newSpeed;

        bool stopped = false;
        Quaternion intendedRot = Quaternion.Euler(0, intendedYaw, 0);

        float intendedDYaw = intendedRot.eulerAngles.y - meshTrans.eulerAngles.y;
        float intDYawRad = intendedDYaw * Mathf.Deg2Rad;
        float forward = Mathf.Cos(intDYawRad);
        float sideway = Mathf.Sin(intDYawRad);

        if(forward < 0.0f && fVel >= 0.0f)
        {
            forward *= .5f + .5f * fVel / 100.0f;
        }

        accel = 5.0f;
        lossFactor = intendedMag / movementSpeed * forward * .02f + .92f;

        oldSpeed = Mathf.Sqrt(velX * velX + velZ * velZ);
        velX += velZ * (intendedMag / movementSpeed) * sideway * .05f;
        velZ -= velX * (intendedMag / movementSpeed) * sideway * .05f;
        newSpeed = Mathf.Sqrt(velX * velX + velZ * velZ);

        if(oldSpeed > 0.0f && newSpeed > 0.0f)
        {
            velX = velX * oldSpeed / newSpeed;
            velZ = velZ * oldSpeed / newSpeed;
        }
    }


    private void WalkingAction()
    {
        if (AnalogStickFlickedBack())
        {
            sideFlipTimer = 0.5f;
            anim.SetTrigger("TurnAround");
            meshTrans.eulerAngles = Vector3.up * intendedYaw;
            fVel = -fVel;
            SetPlayerAction(PlayerAction.TurningAround);
        }
        if (intendedMag == 0)
        {
            SetPlayerAction(PlayerAction.Decelerating);
            return;
        }
        if (controller.GamePlayer.Jump.triggered)
        {
            SetJumpAction();
            return;
        }
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerYVelBasedOnSpeed(4.0f, .5f);
            SetPlayerAction(PlayerAction.Dive);
        }
        UpdateWalkingSpeed();
    }

    private void IdleAction()
    {
        if (AnalogStickFlickedBack())
        {
            sideFlipTimer = 0.5f;
            anim.SetTrigger("TurnAround");
            meshTrans.eulerAngles = Vector3.up * intendedYaw;
            SetPlayerAction(PlayerAction.TurningAround);
            return;
        }
        if (intendedMag != 0)
        {
            meshTrans.eulerAngles = Vector3.up * intendedYaw;
            SetPlayerAction(PlayerAction.Walking);
            return;
        }
        if (controller.GamePlayer.Jump.triggered)
        {
            SetJumpAction();
            return;
        }
        if (controller.GamePlayer.Crouch.IsPressed())
        {
            anim.SetTrigger("Crouch");
            SetPlayerAction(PlayerAction.Crouching);
            return;
        }
    }

    private void DecelerationAction()
    {
        bool stopped = false;

        if (intendedMag != 0)
        {
            SetPlayerAction(PlayerAction.Walking);
            return;
        }
        if (controller.GamePlayer.Jump.triggered)
        {
            SetJumpAction();
            return;
        }

        fVel = GameMath.ApproachDelta(fVel, 0.0f, 28.0f, 28.0f);
        if (fVel == 0.0f)
        {
            stopped = true;
        }
        if (stopped)
        {
            SetPlayerAction(PlayerAction.Idle);
        }
        SetForwardVelocity(fVel);
    }

    private void CrouchingAction()
    {
        if (controller.GamePlayer.Jump.triggered)
        {
            anim.SetTrigger("StandUp");
            SetPlayerAction(PlayerAction.SuperJump);
            return;
        }
        if (!controller.GamePlayer.Crouch.IsPressed())
        {
            anim.SetTrigger("StandUp");
            SetPlayerAction(PlayerAction.Idle);
            return;
        }
    }

    private void CrouchingSlidingAction()
    {
        if (controller.GamePlayer.Jump.triggered)
        {
            if(fVel > 2.5f && !AnalogStickFlickedBack())
            {
                SetPlayerAction(PlayerAction.LongJump);
                return;
            }
            SetPlayerAction(PlayerAction.SuperJump);
            return;
        }
    }

    private void TurningAction()
    {
        if (sideFlipTimer == 0)
        {
            anim.SetTrigger("StandUp");
            if(intendedMag != 0)
            {
                SetPlayerAction(PlayerAction.Walking);
            }
            else
            {
                SetPlayerAction(PlayerAction.Decelerating);
            }
            return;
        }
        if (controller.GamePlayer.Jump.triggered)
        {
            anim.SetTrigger("StandUp");
            SetJumpAction();
            return;
        }

        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerYVelBasedOnSpeed(4.0f, 0);
            SetPlayerAction(PlayerAction.Dive);
        }

        if (AnalogStickFlickedBack())
        {
            sideFlipTimer = .5f;
            meshTrans.eulerAngles = Vector3.up * intendedYaw;
            anim.SetTrigger("StandUp");
            fVel = -fVel;
            SetPlayerAction(PlayerAction.Walking);
        }
        UpdateWalkingSpeed();
    }
    #endregion

    #region Airborne
    private void UpdateVelAir()
    {
        float sidewaysSpeed;
        float intendedDYaw;
        float intendedDMag;
        float drag = (action == PlayerAction.Dive || action == PlayerAction.LongJump) ? 12.0f : 8.0f;
        fVel = GameMath.ApproachDelta(fVel, 0.0f, .07f, .07f);

        intendedDYaw = intendedYaw - meshTrans.eulerAngles.y;
        intendedDMag = intendedMag;
        
        if (fVel < movementSpeed && fVel > -movementSpeed)
        {
            fVel += intendedDMag * Mathf.Cos(intendedDYaw * Mathf.Deg2Rad) * 1.0f * Time.deltaTime;
        }
        sidewaysSpeed = intendedDMag * Mathf.Sin(intendedDYaw * Mathf.Deg2Rad) * 100.0f * Time.deltaTime;

        if (fVel > drag)
        {
            fVel -= 14.0f * Time.deltaTime;
        }
        if (fVel < -16.0f)
        {
            fVel += 28.0f * Time.deltaTime;
        }
        velX = fVel * Mathf.Sin(meshTrans.eulerAngles.y * Mathf.Deg2Rad);
        velZ = fVel * Mathf.Cos(meshTrans.eulerAngles.y * Mathf.Deg2Rad);
        velX += sidewaysSpeed * Mathf.Sin((meshTrans.eulerAngles.y + 90) * Mathf.Deg2Rad);
        velZ += sidewaysSpeed * Mathf.Cos((meshTrans.eulerAngles.y + 90) * Mathf.Deg2Rad);
    }

    private void JumpAction()
    {
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerAction(PlayerAction.Dive);
            return;
        }
        if (onGround)
        {
            
            SetPlayerAction(PlayerAction.JumpLanding);
            return;
        }
        UpdateVelAir();
    }

    private void DoubleJumpAction()
    {
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerAction(PlayerAction.Dive);
            return;
        }
        if (onGround)
        {
            
            SetPlayerAction(PlayerAction.DoubleJumpLanding);
            return;
        }
        UpdateVelAir();
    }

    private void TripleJumpAction()
    {
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerAction(PlayerAction.Dive);
            return;
        }
        if (onGround)
        {
            
            SetPlayerAction(PlayerAction.TripleJumpLanding);
            return;
        }
        UpdateVelAir();
    }

    private void SuperJumpAction()
    {
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerAction(PlayerAction.Dive);
            return;
        }
        if (onGround)
        {
            SetPlayerAction(PlayerAction.SuperJumpLanding);
            return;
        }
        UpdateVelAir();
    }

    private void SideFlipAction()
    {
        if (controller.GamePlayer.Attack.triggered)
        {
            SetPlayerAction(PlayerAction.Dive);
            return;
        }
        if (onGround)
        {
            SetPlayerAction(PlayerAction.SideFlipLanding);
            return;
        }
        UpdateVelAir();
    }

    private void DiveAction()
    {
        if (onGround)
        {
            anim.SetTrigger("StandUp");
            SetPlayerAction(PlayerAction.Decelerating);
            return;
        }
        UpdateVelAir();
    }

    #endregion

    #region Landing Actions
    private void CommonLandingAction()
    {
        doubleJumpTimer = .25f;
    }

    private void CommonLandingCancel()
    {
        if (intendedMag != 0 && fVel > 0.0f)
        {
            SetPlayerAction(PlayerAction.Walking);
            return;
        }
        SetForwardVelocity(0.0f);
        SetPlayerAction(PlayerAction.Idle);
    }

    private void JumpLandingAction()
    {
        CommonLandingAction();
        CommonLandingCancel();
    }

    private void DoubleJumpLandingAction()
    {
        CommonLandingAction();
        CommonLandingCancel();
    }

    private void TripleJumpLandingAction()
    {
        CommonLandingAction();
        CommonLandingCancel();
    }

    private void SuperJumpLandingAction()
    {
        CommonLandingAction();
        CommonLandingCancel();
    }

    private void SideFlipLandingAction()
    {
        CommonLandingAction();
        CommonLandingCancel();
    }

    private void SetJumpAction()
    {
        if(sideFlipTimer > 0)
        {
            SetPlayerAction(PlayerAction.SideFlip);
            sideFlipTimer = 0.0f;
            return;
        }

        if(doubleJumpTimer <= 0.0f)
        {
            SetPlayerAction(PlayerAction.Jump);
        }
        else
        {
            switch (prevAction)
            {
                case PlayerAction.JumpLanding:
                    SetPlayerAction(PlayerAction.DoubleJump);
                    break;
                case PlayerAction.DoubleJumpLanding:
                    if (fVel > movementSpeed * .8f)
                    {
                        SetPlayerAction(PlayerAction.TripleJump);
                        break;
                    }
                    goto default;
                default:
                    SetPlayerAction(PlayerAction.Jump);
                    break;
            }
            
        }
        doubleJumpTimer = 0.0f;
       
    }
    #endregion
}

public enum PlayerAction : byte
{
    Idle,
    Walking,
    Decelerating,
    Crouching,
    TurningAround,

    Jump,
    DoubleJump,
    TripleJump,
    SpinJump,
    LongJump,
    SuperJump,
    SideFlip,
    Freefall,
    Dive,

    JumpLanding,
    DoubleJumpLanding,
    TripleJumpLanding,
    LongJumpLanding,
    SuperJumpLanding,
    SideFlipLanding,
    FreeFallLanding,

    ButtSliding,
    BellySliding,
}