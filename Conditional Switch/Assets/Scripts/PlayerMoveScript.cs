using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
    public PlayerControls controls;
    public Rigidbody2D rb;

    public LogicSystemScript logic;

    public float gravityScale = 3f;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Movement.SwitchGravity.performed += ctx => Switch();
    }

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
        transform.position = new Vector3(-3, 2, 0);
    }

    private void Switch()
    {
        if (!logic.isPaused & logic.gameHasStarted)
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
        } else if (collision.gameObject.tag == "AdvancedQuestionMarkBlock")
        {
            logic.askQuestion();
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