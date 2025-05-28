using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 1;

    bool isMoving = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMoving = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMoving = false;
            return;
        }
        if (!isMoving)
            return;

        Vector2 mouseInput = default;
        Vector3 movementInput = default;
        int verticalInput = 0;

        mouseInput.x = Input.GetAxis("Mouse X");
        mouseInput.y = Input.GetAxis("Mouse Y");
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Q))
            verticalInput = -1;
        else if (Input.GetKey(KeyCode.E))
            verticalInput = 1;

            transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(-mouseInput.y, mouseInput.x, 0));
        transform.position += transform.forward * movementInput.y * speed * Time.deltaTime;
        transform.position += transform.right * movementInput.x * speed * Time.deltaTime;
        transform.position += transform.up * verticalInput * speed * Time.deltaTime;
    }
}
