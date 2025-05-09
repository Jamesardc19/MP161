using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class evelator_controll_fixed : MonoBehaviour
{
    public List<int> sequenceElevator = new List<int>();
    public bool Elevator_in_run;
    public float[] FloorHighs;
    public GameObject ElevatorCabin;
  
    public void AddTaskEve(string name)
    {
        Debug.Log("AddTaskEve called with: " + name);
        
        if (name == "Button floor 1")
        {
            sequenceElevator.Add(0);
            Debug.Log("Added floor 1 to sequence");
        }
        else if (name == "Button floor 2")
        {
            sequenceElevator.Add(1);
            Debug.Log("Added floor 2 to sequence");
        }
        else if (name == "Button floor 3")
        {
            sequenceElevator.Add(2);
            Debug.Log("Added floor 3 to sequence");
        }
        else if (name == "Button floor 4")
        {
            sequenceElevator.Add(3);
            Debug.Log("Added floor 4 to sequence");
        }
        else if (name == "Button floor 5")
        {
            sequenceElevator.Add(4);
            Debug.Log("Added floor 5 to sequence");
        }
        else if (name == "Button floor 6")
        {
            sequenceElevator.Add(5);
            Debug.Log("Added floor 6 to sequence");
        }
        else
        {
            Debug.LogWarning("Unknown button name: " + name);
            return;
        }

        if (!Elevator_in_run)
        {
            Elevator_in_run = true;
            EvelatorDo = StartCoroutine(executeTask());
        }
    }

    Coroutine EvelatorDo;
    Coroutine DoorOpening;
    Coroutine DoorClose;

    public bool Doors_finished;

    public IEnumerator executeTask()
    {
        Debug.Log("Elevator task started. Sequence count: " + sequenceElevator.Count);
        
        while (sequenceElevator.Count > 0)
        {
            yield return new WaitForSeconds(0.005f);

            if (sequenceElevator.Count <= 0) break;
            
            int targetFloorIndex = sequenceElevator[sequenceElevator.Count - 1];
            float targetY = FloorHighs[targetFloorIndex];
            float currentY = ElevatorCabin.transform.localPosition.y;
            
            Debug.Log("Moving elevator to floor " + (targetFloorIndex + 1) + 
                      " (index: " + targetFloorIndex + "). Current Y: " + currentY + 
                      ", Target Y: " + targetY);

            // Move down
            if (currentY > targetY)
            {
                ElevatorCabin.transform.Translate(Vector3.down * Time.deltaTime);

                if (currentY < (targetY + 0.01f))
                {
                    Debug.Log("Reached floor " + (targetFloorIndex + 1) + " (moving down)");
                    DoorOpening = StartCoroutine(HandleDoorOpen(targetFloorIndex));
                    Doors_finished = false;
                    sequenceElevator.RemoveAt(sequenceElevator.Count - 1);
                    yield return new WaitWhile(() => !Doors_finished);
                }
            }
            // Move up
            else if (currentY < targetY)
            {
                ElevatorCabin.transform.Translate(Vector3.up * Time.deltaTime);

                if (currentY > (targetY - 0.01f))
                {
                    Debug.Log("Reached floor " + (targetFloorIndex + 1) + " (moving up)");
                    DoorOpening = StartCoroutine(HandleDoorOpen(targetFloorIndex));
                    Doors_finished = false;
                    sequenceElevator.RemoveAt(sequenceElevator.Count - 1);
                    yield return new WaitWhile(() => !Doors_finished);
                }
            }
            // Already at the right floor
            else
            {
                Debug.Log("Already at floor " + (targetFloorIndex + 1));
                DoorOpening = StartCoroutine(HandleDoorOpen(targetFloorIndex));
                Doors_finished = false;
                sequenceElevator.RemoveAt(sequenceElevator.Count - 1);
                yield return new WaitWhile(() => !Doors_finished);
            }

            ChangeFloorNumbers();
        }

        Debug.Log("Elevator task completed");
        Elevator_in_run = false;
    }

    public GameObject[] FloorNumbers;
    public int CurrentFloorNumber;

    public void ChangeFloorNumbers()
    {
        float currentY = ElevatorCabin.transform.localPosition.y;
        
        // Determine current floor based on position
        for (int i = 0; i < FloorHighs.Length; i++)
        {
            if (Mathf.Abs(currentY - FloorHighs[i]) < 0.1f)
            {
                CurrentFloorNumber = i + 1;
                break;
            }
        }

        // Update floor number displays
        foreach (GameObject numberAssembly in FloorNumbers)
        {
            for (int i = 0; i < numberAssembly.transform.childCount; i++)
            {
                GameObject numberDisplay = numberAssembly.transform.GetChild(i).gameObject;
                bool isCurrentFloor = (i == CurrentFloorNumber - 1);
                numberDisplay.SetActive(isCurrentFloor);
            }
        }
    }

    public float DoorOpenTime = 4f;

    public GameObject[] Door_outside_left; 
    public float[] Door_outside_left_close_value; 
    public float[] Door_outside_left_open_value;
    
    public GameObject[] Door_outside_right; 
    public float[] Door_outside_right_close_value; 
    public float[] Door_outside_right_open_value;

    public GameObject Door_inside_right; 
    public float Door_inside_right_close_value; 
    public float Door_inside_right_open_value;
    
    public GameObject Door_inside_left; 
    public float Door_inside_left_close_value; 
    public float Door_inside_left_open_value;

    public IEnumerator HandleDoorOpen(int whichFloor)
    {
        Debug.Log("Opening doors for floor " + (whichFloor + 1));
        
        bool doorsFullyOpen = false;
        
        while (!doorsFullyOpen)
        {
            yield return new WaitForSeconds(0.005f);

            // Move inside doors
            Vector3 insideLeftTarget = new Vector3(Door_inside_left_open_value, Door_inside_left.transform.localPosition.y, Door_inside_left.transform.localPosition.z);
            Door_inside_left.transform.localPosition = Vector3.Lerp(
                Door_inside_left.transform.localPosition, 
                insideLeftTarget, 
                DoorOpenTime * Time.deltaTime
            );
            
            Vector3 insideRightTarget = new Vector3(Door_inside_right_open_value, Door_inside_right.transform.localPosition.y, Door_inside_right.transform.localPosition.z);
            Door_inside_right.transform.localPosition = Vector3.Lerp(
                Door_inside_right.transform.localPosition, 
                insideRightTarget, 
                DoorOpenTime * Time.deltaTime
            );

            // Move outside doors
            Vector3 outsideLeftTarget = new Vector3(
                Door_outside_left_open_value[whichFloor], 
                Door_outside_left[whichFloor].transform.localPosition.y, 
                Door_outside_left[whichFloor].transform.localPosition.z
            );
            Door_outside_left[whichFloor].transform.localPosition = Vector3.Lerp(
                Door_outside_left[whichFloor].transform.localPosition, 
                outsideLeftTarget, 
                DoorOpenTime * Time.deltaTime
            );
            
            Vector3 outsideRightTarget = new Vector3(
                Door_outside_right_open_value[whichFloor], 
                Door_outside_right[whichFloor].transform.localPosition.y, 
                Door_outside_right[whichFloor].transform.localPosition.z
            );
            Door_outside_right[whichFloor].transform.localPosition = Vector3.Lerp(
                Door_outside_right[whichFloor].transform.localPosition, 
                outsideRightTarget, 
                DoorOpenTime * Time.deltaTime
            );

            // Check if doors are fully open
            if (Mathf.Abs(Door_inside_left.transform.localPosition.x - Door_inside_left_open_value) < 0.001f)
            {
                doorsFullyOpen = true;
            }
        }
        
        Debug.Log("Doors fully open, waiting 5 seconds");
        yield return new WaitForSeconds(5);

        Debug.Log("Starting door close sequence");
        DoorClose = StartCoroutine(HandleDoorClose(whichFloor));
        StopCoroutine(DoorOpening);
    }
    
    public IEnumerator HandleDoorClose(int whichFloor)
    {
        Debug.Log("Closing doors for floor " + (whichFloor + 1));
        
        bool doorsFullyClosed = false;
        
        while (!doorsFullyClosed)
        {
            yield return new WaitForSeconds(0.005f);

            // Move inside doors
            Vector3 insideLeftTarget = new Vector3(Door_inside_left_close_value, Door_inside_left.transform.localPosition.y, Door_inside_left.transform.localPosition.z);
            Door_inside_left.transform.localPosition = Vector3.Lerp(
                Door_inside_left.transform.localPosition, 
                insideLeftTarget, 
                DoorOpenTime * Time.deltaTime
            );
            
            Vector3 insideRightTarget = new Vector3(Door_inside_right_close_value, Door_inside_right.transform.localPosition.y, Door_inside_right.transform.localPosition.z);
            Door_inside_right.transform.localPosition = Vector3.Lerp(
                Door_inside_right.transform.localPosition, 
                insideRightTarget, 
                DoorOpenTime * Time.deltaTime
            );

            // Move outside doors
            Vector3 outsideLeftTarget = new Vector3(
                Door_outside_left_close_value[whichFloor], 
                Door_outside_left[whichFloor].transform.localPosition.y, 
                Door_outside_left[whichFloor].transform.localPosition.z
            );
            Door_outside_left[whichFloor].transform.localPosition = Vector3.Lerp(
                Door_outside_left[whichFloor].transform.localPosition, 
                outsideLeftTarget, 
                DoorOpenTime * Time.deltaTime
            );
            
            Vector3 outsideRightTarget = new Vector3(
                Door_outside_right_close_value[whichFloor], 
                Door_outside_right[whichFloor].transform.localPosition.y, 
                Door_outside_right[whichFloor].transform.localPosition.z
            );
            Door_outside_right[whichFloor].transform.localPosition = Vector3.Lerp(
                Door_outside_right[whichFloor].transform.localPosition, 
                outsideRightTarget, 
                DoorOpenTime * Time.deltaTime
            );

            // Check if doors are fully closed
            if (Mathf.Abs(Door_inside_left.transform.localPosition.x - Door_inside_left_close_value) < 0.001f)
            {
                doorsFullyClosed = true;
            }
        }
        
        Debug.Log("Doors fully closed");
        Doors_finished = true;
        StopCoroutine(DoorClose);
    }
}
