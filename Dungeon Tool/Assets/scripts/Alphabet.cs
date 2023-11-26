using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph;

public class Alphabet : MonoBehaviour
{
    [Serializable]public struct AlphabetData
    {
        public Color m_colour;
        public char m_symbol;
    }

    [Serializable] public class AlphabetLinker
    {
        [SerializeField] public Color m_colour;
        [SerializeField] public char m_symbol;

        public AlphabetLinker(AlphabetData alphabetData)
        {
            this.m_colour = alphabetData.m_colour;
            this.m_symbol = alphabetData.m_symbol;
        }
    }

    [SerializeField] public List<AlphabetLinker> m_alphabet;
}
