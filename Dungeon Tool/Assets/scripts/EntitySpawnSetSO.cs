using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateEntitiesSet", order = 1)]
public class EntitySpawnSetSO : ScriptableObject
{
    [Serializable]
    public struct Entity
    {
        public GameObject m_entityPrefab;
        [Tooltip("num of empty cells this entity needs around it")]
        public int m_widthOfEntity;
        [Tooltip("num of empty cells this entity needs around it")]
        public int m_lengthOfEntity;
    }

    [Serializable]
    public struct EntitySet
    {
        [Tooltip("Always have a chance of 100 to allow at least 1 room to appear")]
        public int m_chanceOfAppearing;
        [Tooltip("This will be the key in the alphabet that will be associated with this room set")]
        public List<Entity> m_entities;
    }

    public List<EntitySet> m_entitySets;
}
