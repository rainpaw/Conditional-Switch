using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMoveScript : MonoBehaviour
{
    public PlayerControls controls;
    public Rigidbody2D rb;

    public LogicSystemScript logic;

    public float gravityScale = 3f;

    // Max regular: 9
    //Max adv: 2
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
        switch (collision.gameObject.tag)
        {
            case "Saw":
                if (!logic.isPresentationMode)
                {
                    logic.isDead = true;
                    logic.gameOver();
                    gameObject.SetActive(false);
                }
                break;
            case "Boundary":
                if (!logic.isPresentationMode)
                {
                    logic.isDead = true;
                    logic.gameOver();
                    gameObject.SetActive(false);
                }
                break;
            case "QuestionMarkBlock":
                regularQuestionNumber++;
                logic.askQuestion(false);
                break;
            case "AdvancedQuestionMarkBlock":
                advQuestionNumber++;
                logic.askQuestion(true);
                break;
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