using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using static Graph;

[ExecuteInEditMode]
public class DifficultyCurve 
{
    private AnimationCurve m_difficultyCurve;

    private List<Index2NodeDataLinker> m_pathList;
    private void OnEnable()
    {
        GraphComponent.OnApplyDifficultyCurve += ApplyCurve;
        GraphComponent.OnDisableScripts += OnDisable;
    }
    private void OnDisable()
    {
        GraphComponent.OnApplyDifficultyCurve -= ApplyCurve;
        GraphComponent.OnDisableScripts -= OnDisable;
    }

    public DifficultyCurve()
    {
        OnEnable();
    }

    public void ApplyCurve(List<Index2NodeDataLinker> pathList, AnimationCurve animCurve, bool applyCurve ,bool useInterval)
    {
        m_pathList = pathList;
        m_difficultyCurve = animCurve;

        for (float i = 0; i < m_pathList.Count; i++)
        {
            float difficultyValue=0;
            if (useInterval&&applyCurve)
            { 
                difficultyValue = Random.Range(
                    (m_difficultyCurve.Evaluate(i / m_pathList.Count) - m_pathList[(int)i].nodeData.difficultyInterval) < 0 
                    ? 0 : (m_difficultyCurve.Evaluate(i / m_pathList.Count) - m_pathList[(int)i].nodeData.difficultyInterval),
                    m_difficultyCurve.Evaluate(i / m_pathList.Count) + m_pathList[(int)i].nodeData.difficultyInterval);
            }
            else if(useInterval)
            {
                difficultyValue = Random.Range(0,m_pathList[(int)i].nodeData.difficultyInterval);
            }
            else
            {
                difficultyValue = m_difficultyCurve.Evaluate(i / m_pathList.Count);
            }
            //Debug.Log("node "+i+" has a diff value of "+Mathf.RoundToInt(m_difficultyCurve.Evaluate(i / m_pathList.Count) )+" with rand inter diff of "+ randomInterval + " at value "+ i / m_pathList.Count );
            m_pathList[(int)i].nodeData.difficultyRating += Mathf.RoundToInt(difficultyValue);
            if (m_pathList[(int)i].nodeData.difficultyRating < 0)
                m_pathList[(int)i].nodeData.difficultyRating = 0;
        }
    }
}
