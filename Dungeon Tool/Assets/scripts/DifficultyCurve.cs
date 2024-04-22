using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph;

public class DifficultyCurve : MonoBehaviour
{
    private AnimationCurve m_difficultyCurve;

    private List<Index2NodeDataLinker> m_pathList;
    private void OnEnable()
    {
        GraphComponent.OnApplyDifficultyCurve += ApplyCurve;
    }
    private void OnDisable()
    {
        GraphComponent.OnApplyDifficultyCurve -= ApplyCurve;
    }

    public void ApplyCurve(List<Index2NodeDataLinker> pathList, AnimationCurve animCurve,int interval)
    {
        m_pathList = pathList;
        m_difficultyCurve = animCurve;
        //AdjustCurve(m_pathList.Count);

        for (float i = 0; i < m_pathList.Count; i++)
        {
            float randomInterval = Random.Range(
                (m_difficultyCurve.Evaluate(i / m_pathList.Count) - interval)<0 ? 0:(m_difficultyCurve.Evaluate(i / m_pathList.Count) - interval),
                m_difficultyCurve.Evaluate(i / m_pathList.Count) + interval);
            Debug.Log("node "+i+" has a diff value of "+Mathf.RoundToInt(m_difficultyCurve.Evaluate(i / m_pathList.Count) )+" with rand inter diff of "+ randomInterval + " at value "+ i / m_pathList.Count );
            m_pathList[(int)i].nodeData.difficultyRating += Mathf.RoundToInt(randomInterval); ;
        }
    }
}
