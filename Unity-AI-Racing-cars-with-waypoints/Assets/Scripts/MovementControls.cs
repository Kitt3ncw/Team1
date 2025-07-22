using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class MovementControls : MonoBehaviour
{
    //Private variables//
    Rigidbody rbody;

    //Public Variables
    public WheelCollider[] wc;
    public Transform[] tires;
    public float mTorque = 2500f;
    public float mSteer = 25f;
    public float mBrake = 10000f;
    public float mDecelerationSpeed = 1000f;
    public int wcTorqueLength;
    public int wcDecelerationSpeedLength;
    public Vector3 cOfMass = new Vector3(0, 0.7f, 0);
    public bool brakeAllowed;
    public bool turning;
    public float currentSpeed;
    public float mSpeed;
    public float mMagnitude;
    public Light leftSpotLight;
    public Light leftPointLight;
    public Light rightSpotLight;
    public Light rightPointLight;

    AudioSource audioSource;
    public int[] gearRatio;

    // 在类变量区域添加加速相关变量
    [Header("Boost Settings")] public float boostMultiplier = 1.8f; // 加速时的扭矩倍数
    public float boostDuration = 3.0f; // 加速持续时间(秒)
    public float boostCooldown = 5.0f; // 加速冷却时间(秒)
    private float currentBoostTime = 0f; // 当前加速剩余时间
    private float currentCooldown = 0f; // 当前冷却剩余时间

    public ParticleSystem BoostStateParticleSystem;
    public bool isBoosting = false; // 是否正在加速

    private cameraController cameraController;

    // Use this for initialization
    void Start()
    {
        rbody = gameObject.GetComponent<Rigidbody>();
        audioSource = gameObject.GetComponent<AudioSource>();
        cameraController = FindObjectOfType<cameraController>();
        audioSource.Play();
        rbody.centerOfMass = cOfMass;
    }

    void Update()
    {
        LapCompleteHuman.currentSpeed = currentSpeed;
        EngineSound();
        HandBrake();
        RotatingTires();

        // 更新加速状态
        UpdateBoostState();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CarMovement();
        DecelerationSpeed();
    }

    // 新增加速状态更新方法
    private void UpdateBoostState()
    {
        if (isBoosting)
        {
            currentBoostTime -= Time.deltaTime;
            if (currentBoostTime <= 0)
            {
                isBoosting = false;
                currentCooldown = boostCooldown;
                // 可选：添加加速结束特效或声音
                cameraController.ResetDistance();
                BoostStateParticleSystem.Stop();

            }
        }
        else if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // // 检测加速输入（例如按下左Shift键）
        // if (Input.GetKeyDown(KeyCode.LeftShift) && currentCooldown <= 0)
        // {
        //     ActivateBoost();
        // }
    }

    // 新增激活加速方法
    public void ActivateBoost()
    {
        if (currentCooldown > 0)
        {
            return;
        }
        isBoosting = true;
        currentBoostTime = boostDuration;
        // 可选：添加加速启动特效或声音
        Debug.Log("加速开始");
        cameraController.SetDistance();
        BoostStateParticleSystem.Play();
    }

    /*void CarMovement(){
        currentSpeed = wc[2].radius * wc[2].rpm * 60/1000 * Mathf.PI;
        currentSpeed = Mathf.Round(currentSpeed);
        if(currentSpeed < mSpeed && rbody.velocity.magnitude <= mMagnitude){
            for(int i = 0; i < wcTorqueLength; i++){
                wc[i].motorTorque = CrossPlatformInputManager.GetAxis("Vertical") * mTorque;
                leftPointLight.intensity = 0f;
                rightPointLight.intensity = 0f;
            }
        }else{
            currentSpeed = mSpeed;
        }
        wc[0].steerAngle = CrossPlatformInputManager.GetAxis("Horizontal") * mSteer;
        wc[1].steerAngle = CrossPlatformInputManager.GetAxis("Horizontal") * mSteer;
        Debug.Log("wc[0].steerAngle: " + wc[0].steerAngle);
        Debug.Log("wc[1].steerAngle: " + wc[1].steerAngle);
        if(wc[0].steerAngle < 0 && wc[1].steerAngle < 0 && CrossPlatformInputManager.GetAxis("Horizontal") == -1){
            rightPointLight.intensity = 20f;
        }else if(wc[0].steerAngle > 0 && wc[1].steerAngle > 0 && CrossPlatformInputManager.GetAxis("Horizontal") == 1){
            leftPointLight.intensity = 20f;
        }
    }*/

    void CarMovement()
    {
        currentSpeed = wc[2].radius * wc[2].rpm * 60 / 1000 * Mathf.PI;
        currentSpeed = Mathf.Round(currentSpeed);

        float vInput = CrossPlatformInputManager.GetAxis("Vertical");
        float hInput = CrossPlatformInputManager.GetAxis("Horizontal");

        // 计算基础扭矩值
        float baseTorque = vInput * mTorque;

        //Debug.Log("rbody.velocity.magnitude:" + rbody.velocity.magnitude);
        Debug.Log("baseTorque:" + baseTorque);

        // 应用加速效果（仅在前进时有效）
        if (isBoosting && vInput > 0)
        {
            baseTorque *= boostMultiplier;
            // 可选：添加加速时的视觉反馈（如尾焰效果）
        }

        // 前进
        if (vInput > 0 && currentSpeed < mSpeed && rbody.velocity.magnitude <= mMagnitude)
        {
            for (int i = 0; i < wcTorqueLength; i++)
            {
                wc[i].motorTorque = baseTorque;
            }

            leftPointLight.intensity = 0f;
            rightPointLight.intensity = 0f;
        }
        // 倒车
        else if (vInput < 0 && currentSpeed < mSpeed && rbody.velocity.magnitude <= mMagnitude)
        {
            for (int i = 0; i < wcTorqueLength; i++)
            {
                wc[i].motorTorque = vInput * (mTorque * 0.5f); // 倒车速度一般设为一半
            }

            leftPointLight.intensity = 0f;
            rightPointLight.intensity = 0f;
        }
        else
        {
            // 保持速度不变或踩刹车
            for (int i = 0; i < wcTorqueLength; i++)
            {
                wc[i].motorTorque = 0f;
            }
        }

        // 转向
        wc[0].steerAngle = hInput * mSteer;
        wc[1].steerAngle = hInput * mSteer;

        //Debug.Log("wc[0].steerAngle: " + wc[0].steerAngle);
        //Debug.Log("wc[1].steerAngle: " + wc[1].steerAngle);

        // 方向灯逻辑
        if (hInput < 0)
        {
            rightPointLight.intensity = 20f;
        }
        else if (hInput > 0)
        {
            leftPointLight.intensity = 20f;
        }
        else
        {
            leftPointLight.intensity = 0f;
            rightPointLight.intensity = 0f;
        }
    }


    /*void DecelerationSpeed(){
        if(!brakeAllowed && CrossPlatformInputManager.GetAxis("Vertical") != 1){
            for(int i = 0; i < wcDecelerationSpeedLength; i++){
                wc[i].brakeTorque = mDecelerationSpeed;
                wc[i].motorTorque = 0;
                leftPointLight.intensity = 20f;
                rightPointLight.intensity = 20f;
            }
        }
        if(CrossPlatformInputManager.GetAxis("Horizontal") != 0){
            turning = true;
            if(CrossPlatformInputManager.GetAxis("Horizontal") == -1){
                float floor = 0f;
                float ceil = 20f;
                float emission = floor + Mathf.PingPong(Time.deltaTime*2f, ceil - floor);
                rightPointLight.intensity = emission;
            }else if(CrossPlatformInputManager.GetAxis("Horizontal") == 1){
                float floor = 0f;
                float ceil = 20f;
                float emission = floor + Mathf.PingPong(Time.deltaTime*2f, ceil - floor);
                leftPointLight.intensity = emission;
            }
        }else{
            turning = false;
            leftPointLight.intensity = 20f;
            rightPointLight.intensity = 20f;
        }
    }*/

    void DecelerationSpeed()
    {
        float vInput = CrossPlatformInputManager.GetAxis("Vertical");

        if (!brakeAllowed && Mathf.Abs(vInput) < 0.1f)
        {
            for (int i = 0; i < wcDecelerationSpeedLength; i++)
            {
                wc[i].brakeTorque = mDecelerationSpeed;
                wc[i].motorTorque = 0;
            }
        }
        else
        {
            for (int i = 0; i < wcDecelerationSpeedLength; i++)
            {
                wc[i].brakeTorque = 0;
            }
        }
    }


    /*void HandBrake(){
        if(CrossPlatformInputManager.GetAxis("Vertical") == -1){
            brakeAllowed = true;
        }else{
            brakeAllowed = false;
        }

        if(rbody.velocity.magnitude <= 10f && brakeAllowed && turning){
            mBrake = 20f;
        }else if(brakeAllowed){
            for(int i = 0; i < wcTorqueLength; i++){
                wc[i].brakeTorque = mBrake;
                wc[i].motorTorque = 0f;
                leftPointLight.intensity = 20f;
                rightPointLight.intensity = 20f;
            }
            rbody.drag = 0.4f;
        }else if(!brakeAllowed && CrossPlatformInputManager.GetAxis("Vertical") == 1){
            
            for(int i = 0; i < wcTorqueLength; i++){
                wc[i].brakeTorque = 0;
                wc[i].motorTorque = mTorque;
                leftPointLight.intensity = 0f;
                rightPointLight.intensity = 0f;
            }
            rbody.drag = 0.1f;
        }
    }*/

    void HandBrake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            brakeAllowed = true;
        }
        else
        {
            brakeAllowed = false;
        }

        if (rbody.velocity.magnitude <= 10f && brakeAllowed && turning)
        {
            mBrake = 20f;
        }
        else if (brakeAllowed)
        {
            for (int i = 0; i < wcTorqueLength; i++)
            {
                wc[i].brakeTorque = mBrake;
                wc[i].motorTorque = 0f;
            }

            rbody.drag = 0.4f;
        }
        else
        {
            // 刹车被取消，motorTorque 由 CarMovement 控制
            for (int i = 0; i < wcTorqueLength; i++)
            {
                wc[i].brakeTorque = 0;
            }

            rbody.drag = 0.1f;
        }
    }


    void RotatingTires()
    {
        //Spinning Tires
        for (int i = 0; i < wcTorqueLength; i++)
        {
            tires[i].Rotate(wc[i].rpm / 60 * 360 * Time.deltaTime, 0f, 0f);
        }

        //Steering Tires
        for (int i = 0; i < 2; i++)
        {
            tires[i].localEulerAngles = new Vector3(tires[i].localEulerAngles.x,
                wc[i].steerAngle - tires[i].localEulerAngles.z, tires[i].localEulerAngles.z);
        }
    }

    void EngineSound()
    {
        float pitch = 0f;

        pitch = (float) ((currentSpeed / mSpeed) + 0.4);
        if (pitch >= 1.4f)
        {
            pitch = 0.9f;
        }

        audioSource.pitch = pitch;
    }
}