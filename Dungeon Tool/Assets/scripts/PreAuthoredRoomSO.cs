using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreAuthoredRoomSO : ScriptableObject
{
    [Serializable]
    public struct Room
    {
        [Tooltip("Always have a chance of 100 to allow at least 1 room to appear")]
        public int m_chanceOfAppearing;
        [Tooltip("This will dictate the size of blank space created")]
        public int m_roomWidth;
        public int m_roomHeight;
        public GameObject m_roomPrefab;
        public GameObject m_roomDoorBlockerPrefab;
    }

    [Serializable]
    public struct Roomset
    {
        [Tooltip("This will be the key in the alphabet that will be associated with this room set")]
        public string m_alphabetKey;
        public List<Room> m_roomPrefab;
    }

    public List<Roomset> m_roomSets;


}
