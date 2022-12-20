using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPOI : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private PlayerController playerController;

    private void Awake()
    {
        //Master.Instance.Camera = this;
        playerController = new PlayerController();
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        playerController.Enable();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void Update()
    {
        
    }
}
