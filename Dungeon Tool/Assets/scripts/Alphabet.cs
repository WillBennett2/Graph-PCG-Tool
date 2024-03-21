using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateAlphabetEntry", order = 2)]

public class Alphabet : ScriptableObject
{
    [Serializable]
    public struct AlphabetData
    {
        public Color m_colour;
        public string m_symbol;
    }

    [Serializable]
    public class AlphabetLinker
    {
        [SerializeField] public Color m_colour;
        [SerializeField] public string m_symbol;

        public AlphabetLinker(AlphabetData alphabetData)
        {
            this.m_colour = alphabetData.m_colour;
            this.m_symbol = alphabetData.m_symbol;
        }
    }

    [SerializeField] public List<AlphabetLinker> m_alphabet;
}
