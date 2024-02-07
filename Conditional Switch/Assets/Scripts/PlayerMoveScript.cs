using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
    public PlayerControls controls;
    public Rigidbody2D rb;

    public LogicSystemScript logic;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Movement.SwitchGravity.performed += ctx => Switch();
    }

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Switch()
    {
        if (!logic.isPaused)
        {
            rb.gravityScale = 0 - rb.gravityScale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Saw" | collision.gameObject.tag == "Boundary")
        {
            logic.isDead = true;
            logic.gameOver();
            gameObject.SetActive(false);
        } else if (collision.gameObject.tag == "QuestionMarkBlock")
        {
            logic.askQuestion();
            Destroy(collision.gameObject);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
}