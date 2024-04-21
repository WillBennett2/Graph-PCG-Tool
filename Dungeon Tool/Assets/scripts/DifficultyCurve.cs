using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph;

public class DifficultyCurve : MonoBehaviour
{
    [SerializeField] private AnimationCurve m_difficultyCurve;

    private List<Index2NodeDataLinker> m_pathList;
    private void AdjustCurve(int maxLength)
    {
        Debug.Log("key value = " + m_difficultyCurve.Evaluate(m_difficultyCurve.length));

        int keyIndex = m_difficultyCurve.length - 1;
        Keyframe lastKey = m_difficultyCurve[keyIndex];

        lastKey.time = maxLength;
        //m_difficultyCurve.RemoveKey(keyIndex);
        m_difficultyCurve.MoveKey(keyIndex, lastKey);

        //adjust all keys throughout to keep curve shape
       
    }
    public void ApplyCurve(List<Index2NodeDataLinker> pathList)
    {
        m_pathList = pathList;
        AdjustCurve(m_pathList.Count);

        for (int i = 0; i < m_pathList.Count; i++)
        {
            m_pathList[i].nodeData.difficultyRating += Mathf.RoundToInt(m_difficultyCurve.Evaluate(i));
        }
    }
}
