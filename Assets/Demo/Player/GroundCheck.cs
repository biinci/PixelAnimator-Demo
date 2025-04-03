using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded => isGrounded;
    [SerializeField] private bool isGrounded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var root = other.gameObject.transform.root.gameObject;
        if (root.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    
    private void OnTriggerStay2D(Collider2D other)
    {
        var root = other.gameObject.transform.root.gameObject;
        if (root.CompareTag("Ground") && isGrounded == false)
        {
            isGrounded = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        var root = other.gameObject.transform.root.gameObject;
        if (root.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
