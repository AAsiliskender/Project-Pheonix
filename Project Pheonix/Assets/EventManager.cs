using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static BattleManager;
using static GameManager;

public class EventManager : MonoBehaviour
{

    public MouseInputProvider mouseInputProvider;

    void Start()
    {
        // Subscribe to the MouseInputProvider events (clicking etc.)
        mouseInputProvider = FindObjectOfType<MouseInputProvider>();
        if (mouseInputProvider != null)
        {
            mouseInputProvider.onClicked.AddListener(OnMouseClicked);
            mouseInputProvider.onReleased.AddListener(OnMouseReleased);
        }
    }

    // Have classes listen to this event, i.e. EventManager.[Event] += [Function], subscribe to enable and unsubscribe to disable
    public delegate void TurnFinished();
    public static event TurnFinished turnFinished; 

    public static event Action<int> playerDamageRecieved;
    public static event Action playerDeathsDoor;

    [SerializeField]
    public UnityEvent onMouseClicked;
    public UnityEvent onMouseReleased;

    private void OnEnable()
    {
        // Subscribe to the MouseInputProvider events (clicking etc.)
        mouseInputProvider = FindObjectOfType<MouseInputProvider>();
        if (mouseInputProvider != null)
        {
            mouseInputProvider.onClicked.AddListener(OnMouseClicked);
            mouseInputProvider.onReleased.AddListener(OnMouseReleased);
        }


    }

    private void OnDisable()
    {
        if (mouseInputProvider != null)
        {
            // Unsubscribe from the MouseInputProvider events
            mouseInputProvider.onClicked.RemoveListener(OnMouseClicked);
            mouseInputProvider.onReleased.RemoveListener(OnMouseReleased); //TODO: ADD LISTENERS TO THIS EVENT CONDITIONALLY (I.E. TILE PRESS IF IN BATTLE AND UNPAUSED)
        }
    }

    private void OnMouseClicked()
    {
        // Handle mouse click event
        onMouseClicked.Invoke();
    }

    private void OnMouseReleased()
    {
        // Handle mouse release event
        onMouseReleased.Invoke();
    }

    public void SetMouseInputProvider(MouseInputProvider provider)
    {
        mouseInputProvider = provider;
    }


    // This is for buttons
    //void OnGUI()
    //{
        
    //}




    //void BattleClick





}
