using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 10;
    [SerializeField] private float playerSpeedUpMult = 1.3f;
    [SerializeField] private float playerJumpSpeed = 15;

    private Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //передвижение влево и вправо
    public void MoveLR(InputAction.CallbackContext context)
    {
        float axis = context.ReadValue<float>();
        rb2d.AddForce(new Vector2(axis, 0) * playerSpeed, ForceMode2D.Force);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rb2d.AddForce(Vector2.up * playerJumpSpeed, ForceMode2D.Impulse);
        }
        
    }
}
