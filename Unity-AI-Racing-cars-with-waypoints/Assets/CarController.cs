using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 3f; // 降低基础速度
    public float maxSpeed = 3f; // 显著降低最大速度
    public float accelerationRate = 1.2f; // 降低加速度
    public float brakeRate = 6f;
    public float decelerationRate = 1.8f; // 增加自然减速力度
    public float turnSpeed = 10f; // 降低转向速度
    
    [Header("Wheel Settings")]
    public float maxSteerAngle = 5f; // 显著减小最大转向角度
    public float wheelRotationSpeed = 80f; // 降低轮子旋转速度

    [Header("UI Buttons")]
    public Button leftButton;
    public Button rightButton;
    public Button accelerateButton;
    public Button brakeButton;

    [Header("Wheel References")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    private float currentSpeed = 0f;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isAccelerating = false;
    private bool isBraking = false;
    private float currentSteerAngle = 0f;

    void Start()
    {
        // 设置按钮按下事件
        leftButton.onClick.AddListener(() => SetDirection(true)); // 左转
        rightButton.onClick.AddListener(() => SetDirection(false)); // 右转
        
        accelerateButton.onClick.AddListener(() => {
            isAccelerating = true;
            isBraking = false;
        });
        
        brakeButton.onClick.AddListener(() => {
            isBraking = true;
            isAccelerating = false;
        });

        // 设置按钮释放事件
        SetupButtonRelease(leftButton, () => { if (isMovingLeft) ResetDirection(); });
        SetupButtonRelease(rightButton, () => { if (isMovingRight) ResetDirection(); });
        SetupButtonRelease(accelerateButton, () => isAccelerating = false);
        SetupButtonRelease(brakeButton, () => isBraking = false);
    }

    void Update()
    {
        HandleSpeed();
        HandleMovement();
        UpdateWheels();
    }

    void SetDirection(bool left)
    {
        if (left)
        {
            isMovingLeft = true;
            isMovingRight = false;
        }
        else
        {
            isMovingLeft = false;
            isMovingRight = true;
        }
    }

    void ResetDirection()
    {
        isMovingLeft = false;
        isMovingRight = false;
    }

    void HandleSpeed()
    {
        if (isBraking)
        {
            // 使用更平滑的刹车曲线
            currentSpeed = Mathf.Lerp(currentSpeed, 0, brakeRate * Time.deltaTime);
        }
        else if (isAccelerating)
        {
            // 更平滑的加速曲线
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, accelerationRate * Time.deltaTime);
        }
        else if (currentSpeed > 0)
        {
            // 更快的自然减速
            currentSpeed *= Mathf.Pow(0.3f, Time.deltaTime / decelerationRate);
            if (currentSpeed < 0.05f) currentSpeed = 0;
        }
    }

    void HandleMovement()
    {
        // 更新转向角度（使用更小的转向角度）
        if (isMovingLeft)
        {
            // 左转使用负角度
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, -maxSteerAngle, turnSpeed * Time.deltaTime);
        }
        else if (isMovingRight)
        {
            // 右转使用正角度
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, maxSteerAngle, turnSpeed * Time.deltaTime);
        }
        else
        {
            // 回正速度更快
            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, 0f, turnSpeed * 3 * Time.deltaTime);
        }

        // 应用车辆旋转（转向）
        // 加入非线性曲线，低速时转向更快，高速时转向更稳
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Lerp(1.0f, 0.6f, speedFactor);
        float rotationFactor = currentSteerAngle * steerFactor * Time.deltaTime;
        
        transform.Rotate(0f, rotationFactor, 0f);

        // 应用车辆前进
        if (currentSpeed > 0)
        {
            // 使用平滑移动
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }
    }

    void UpdateWheels()
    {
        // 更新前轮转向角度
        if (frontLeftWheel)
        {
            frontLeftWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
        }
        
        if (frontRightWheel)
        {
            frontRightWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
        }

        // 旋转所有轮子
        float rotationAmount = currentSpeed * wheelRotationSpeed * Time.deltaTime;
        
        if (frontLeftWheel)
        {
            frontLeftWheel.Rotate(rotationAmount, 0f, 0f);
        }
        
        if (frontRightWheel)
        {
            frontRightWheel.Rotate(rotationAmount, 0f, 0f);
        }
        
        if (rearLeftWheel)
        {
            rearLeftWheel.Rotate(rotationAmount, 0f, 0f);
        }
        
        if (rearRightWheel)
        {
            rearRightWheel.Rotate(rotationAmount, 0f, 0f);
        }
    }

    void SetupButtonRelease(Button button, UnityEngine.Events.UnityAction releaseAction)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener((data) => releaseAction());
        
        trigger.triggers.Add(entry);
    }
}