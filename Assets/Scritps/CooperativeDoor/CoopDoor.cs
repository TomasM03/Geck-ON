using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class CoopDoor : MonoBehaviourPun
{
    [System.Serializable]
    public class DoorConfig
    {
        public int doorID;
        public GameObject doorObject;
        public string teamRequired;
        public bool button1Pressed = false;
        public bool button2Pressed = false;
        public float button1Time = -1f;
        public float button2Time = -1f;
    }

    public List<DoorConfig> doors = new List<DoorConfig>();

    public float syncTimeWindow = 2f;

    void Start()
    {
        foreach (DoorConfig door in doors)
        {
            if (door.doorObject != null)
            {
                door.doorObject.SetActive(true);
            }
        }
    }

    public void OnButtonActivated(int doorID, string team)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Not master client, ignoring button activation");
            return;
        }

        photonView.RPC("SyncButtonActivation", RpcTarget.All, doorID, team, (float)PhotonNetwork.Time);
    }

    [PunRPC]
    void SyncButtonActivation(int doorID, string team, float activationTime)
    {
        DoorConfig door = doors.Find(d => d.doorID == doorID);

        if (door == null)
        {
            Debug.LogError($"Door with ID {doorID} not found!");
            return;
        }

        if (door.teamRequired != team)
        {
            Debug.Log($"Door {doorID} requires team {door.teamRequired}, got {team}");
            return;
        }

        if (!door.button1Pressed)
        {
            door.button1Pressed = true;
            door.button1Time = activationTime;
            Debug.Log($"Door {doorID}: Button 1 pressed at {activationTime}");
        }
        else if (!door.button2Pressed)
        {
            door.button2Pressed = true;
            door.button2Time = activationTime;
            Debug.Log($"Door {doorID}: Button 2 pressed at {activationTime}");

            CheckDoorOpen(door);
        }
    }

    void CheckDoorOpen(DoorConfig door)
    {
        if (!door.button1Pressed || !door.button2Pressed)
            return;

        float timeDiff = Mathf.Abs(door.button1Time - door.button2Time);

        Debug.Log($"Door {door.doorID}: Time difference = {timeDiff}s (max: {syncTimeWindow}s)");

        if (timeDiff <= syncTimeWindow)
        {
            OpenDoor(door);
        }
        else
        {
            Debug.Log($"Door {door.doorID}: Buttons not pressed in sync, resetting...");
            ResetDoor(door);
        }
    }

    void OpenDoor(DoorConfig door)
    {
        Debug.Log($"<color=cyan>Door {door.doorID} OPENED for team {door.teamRequired}!</color>");

        if (door.doorObject != null)
        {
            door.doorObject.SetActive(false);
        }
    }

    void ResetDoor(DoorConfig door)
    {
        door.button1Pressed = false;
        door.button2Pressed = false;
        door.button1Time = -999f;
        door.button2Time = -999f;

        CoopDoorButton[] allButtons = FindObjectsOfType<CoopDoorButton>();
        foreach (CoopDoorButton button in allButtons)
        {
            if (button.GetDoorID() == door.doorID && button.GetRequiredTeam() == door.teamRequired)
            {
                button.ResetButton();
            }
        }
    }

    public void ResetAllDoors()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncResetAllDoors", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncResetAllDoors()
    {
        foreach (DoorConfig door in doors)
        {
            if (door.doorObject != null)
            {
                door.doorObject.SetActive(true);
            }

            ResetDoor(door);
        }

        Debug.Log("All doors reset");
    }
}