using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMoveScript : MonoBehaviour
{
    public PlayerControls controls;
    public Rigidbody2D rb;

    public LogicSystemScript logic;

    public float gravityScale = 3f;

    public int regularQuestionNumber = 0;
    public int advQuestionNumber = 0;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Movement.SwitchGravity.performed += ctx => Switch();
        controls.Movement.Pause.performed += ctx => logic.pauseGame();
    }

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
        transform.position = new Vector3(-3, 2, 0);
    }

    private void Switch()
    {
        if (!logic.isPaused & logic.gameHasStarted & !IsPointerOverPauseButton())
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
            regularQuestionNumber++;
            logic.askQuestion(false);
        } else if (collision.gameObject.tag == "AdvancedQuestionMarkBlock")
        {
            advQuestionNumber++;
            logic.askQuestion(true);
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

    public static bool IsPointerOverPauseButton()
    {
        PointerEventData currentEventData = new PointerEventData(EventSystem.current);
        currentEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(currentEventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++) // Loops through raycastResults, to get value use raycastResults[i]
        {
            if (raycastResults[i].gameObject.layer == 6) // Layer 6 is the pause button layer
            {
                return true;
            }
        }

        return false;
    }
}