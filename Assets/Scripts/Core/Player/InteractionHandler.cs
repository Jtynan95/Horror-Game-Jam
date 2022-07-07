using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    private IEnumerator MoveDoor_Holder;
    private PlayerBase Player;
    [Header("Doors")]
    public bool isHoldingDoor;
    public float doorForce;
    [Header("Dialogue Settings")]
    public bool isInDialogue;
    public Dialogue currentDialogue;
    public Queue<string> dialogueQueue = new Queue<string>();

    
    
    void Start()
    {
        Player = GetComponent<PlayerBase>();

    }

    private void Update()
    {
        if (isInDialogue && Input.anyKeyDown) GoNextDialogue();
        
        
        if (UIManager.Instance.PlayerCursor.hoveredObject == null)
            return;


        if (UIManager.Instance.PlayerCursor.hoveredObject.CompareTag("Pickup") && Input.GetKeyDown(KeyCode.Mouse0))
        {
            PickupItem();
        }
        if (UIManager.Instance.PlayerCursor.hoveredObject.CompareTag("Door") && Input.GetKey(KeyCode.Mouse0))
            MoveDoor();

        if (UIManager.Instance.PlayerCursor.hoveredObject.TryGetComponent(out IDialogue dialogueObject) && Input.GetKeyDown(KeyCode.E))
        {
            
            StartDialogue(dialogueObject.GetDialogue());

        }
            
        
        
    }

    private void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Begin Dialogue");
        isInDialogue = true;
        currentDialogue = dialogue;
        UIManager.Instance.dialogueHandler.ShowDialogueBox();

        for (int i = 0; i < currentDialogue.dialogueEntrys.Length; i++)
        {
            dialogueQueue.Enqueue(currentDialogue.dialogueEntrys[i]);
            Debug.Log("Adding dialogue to queue");
        }
            
        
        GoNextDialogue();
        
    }

    private void GoNextDialogue()
    {
        if (UIManager.Instance.dialogueHandler.isWritingDialogue)
        {
            Debug.Log("Skipping to end of dialogue");
            UIManager.Instance.dialogueHandler.SkipDialogueWrite();
            return;
        }
        if (dialogueQueue.Count <= 0)
        {
            EndDialogue();
            return;
        }

        UIManager.Instance.dialogueHandler.DisplayDialogueText(dialogueQueue.Dequeue());


    }

    private void EndDialogue()
    {
        isInDialogue = false;
        UIManager.Instance.dialogueHandler.HideDialogueBox();
        
    }

    private void MoveDoor()
    {
        if (MoveDoor_Holder == null)
        {
            MoveDoor_Holder = HoldDoor_Co();
            StartCoroutine(MoveDoor_Holder);
        }
        else
        {
            if (isHoldingDoor)
                return;
            StopCoroutine(MoveDoor_Holder);
            MoveDoor_Holder = HoldDoor_Co();
            StartCoroutine(MoveDoor_Holder);
            
        }
            
    }

    private IEnumerator HoldDoor_Co()
    {
        float mouseY;
        float mouseX;
        Vector3 direction;
        //Vector3 camStartingRotation = Camera.main.transform.eulerAngles;
        isHoldingDoor = true;
        Player.playerController.canMouseLook = false;
        GameObject doorObj = UIManager.Instance.PlayerCursor.hoveredObject;
        Rigidbody doorRb = doorObj.GetComponent<Rigidbody>();
        
        while (isHoldingDoor)
        {
            if (Input.GetMouseButtonUp(0)) isHoldingDoor = false;

            //Camera.main.transform.eulerAngles = camStartingRotation;
            direction = (Player.transform.position - doorObj.transform.position) * -Input.GetAxis("Mouse Y");
           // Debug.Log(direction);
            doorRb.AddForceAtPosition(direction, doorObj.transform.position); 
            
            yield return null;
            

        }

        Player.playerController.canMouseLook = true;

    }

    public void PickupItem()
    {
        Player.playerInventory.AddToInventory(UIManager.Instance.PlayerCursor.hoveredObject.GetComponent<Pickup>().item);
        Destroy(UIManager.Instance.PlayerCursor.hoveredObject.transform.gameObject);
    }
    
    
}
