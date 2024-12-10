using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BonusController : MonoBehaviour
{
    [SerializeField]
    private SocketIOManager m_SocketManager;
    [SerializeField]
    private UIManager m_UIManager;
    [SerializeField]
    private SlotBehaviour m_SlotBehaviour;
    [SerializeField]
    private ImageAnimation m_FreeSpinInitAnimation;

    private bool isFreezeRunning = false;

    #region STICKY BONUS
    internal void StartStickyBonus()
    {
        int row = 0;
        int col = 0;
        if (m_SocketManager.resultData.isStickyBonus)
        {
            for (int i = 0; i < m_SocketManager.resultData.stickyBonusValue.Count; i++)
            {
                row = m_SocketManager.resultData.stickyBonusValue[i].position[0];
                col = m_SocketManager.resultData.stickyBonusValue[i].position[1];

                if (!CheckSticky(m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform))
                    m_SlotBehaviour.m_Sticky.Add(new Sticky
                    {
                        m_Transform = m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform,
                        m_Count = m_SocketManager.resultData.stickyBonusValue[i].value
                    });
            }
        }
    }

    private bool CheckSticky(Transform m_Transform)
    {
        for (int i = 0; i < m_SlotBehaviour.m_Sticky.Count; i++)
        {
            if (m_SlotBehaviour.m_Sticky[i].m_Transform == m_Transform)
            {
                return true;
            }
        }
        return false;
    }

    internal bool GetSticky(Transform m_transform, bool start_stop)
    {
        for (int i = 0; i < m_SlotBehaviour.m_Sticky.Count; i++)
        {
            if (m_SlotBehaviour.m_Sticky[i].m_Transform == m_transform)
            {
                if (start_stop && m_SlotBehaviour.m_Sticky[i].m_Count > 0)
                {
                    Sticky sticky = m_SlotBehaviour.m_Sticky[i];
                    sticky.m_Count--;
                    m_SlotBehaviour.m_Sticky[i] = sticky;
                    if (m_SlotBehaviour.m_Sticky[i].m_Count == 0)
                    {
                        m_SlotBehaviour.m_Sticky.Remove(m_SlotBehaviour.m_Sticky[i]);
                        m_SlotBehaviour.m_Sticky.TrimExcess();
                    }
                    return true;
                }
                else
                {
                    if (!start_stop)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region FREEZE BONUS
    internal void StartFreezeBonus()
    {
        int row = 0;
        int col = 0;
        //PopulateFreeSpinResult();
        if (m_SocketManager.resultData.isFreeSpin)
        {
            if(!isFreezeRunning)
            {
                ResetBonus();
                isFreezeRunning = true;
                //Debug.Log("Executed...");
            }

            //if (m_SocketManager.resultData.BonusResultReel.Count > 0)
            //    PopulateFreeSpinResult();

            for (int i = 0; i < m_SocketManager.resultData.frozenIndices.Count; i++)
            {
                row = m_SocketManager.resultData.frozenIndices[i].position[0];
                col = m_SocketManager.resultData.frozenIndices[i].position[1];

                if (!CheckFreeze(m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform))
                    m_SlotBehaviour.m_Sticky.Add(new Sticky
                    {
                        m_Transform = m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform,
                        m_Count = m_SocketManager.resultData.frozenIndices[i].prizeValue
                    });
            }
        }
    }

    //HACK: May be used in future with backend
    internal void PopulateFreeSpinResult()
    {
        for (int i = 0; i < m_SlotBehaviour.Tempimages.Count; i++)
        {
            for (int j = 0; j < m_SlotBehaviour.Tempimages[i].slotImages.Count; j++)
            {
                m_SlotBehaviour.m_ShowTempImages[i].slotImages[j].transform.GetChild(2).GetComponent<Image>().sprite = m_SlotBehaviour.myImages[m_SocketManager.resultData.BonusResultReel[i][j]];
            }
        }

        //int row = 0;
        //int col = 0;
        //for (int i = 0; i < m_SocketManager.resultData.FinalsymbolsToEmit.Count; i++)
        //{
        //    row = m_SocketManager.resultData.FinalResultReel[i][0];
        //    col = m_SocketManager.resultData.FinalResultReel[i][1];
        //    m_SlotBehaviour.PopulateAnimationSprites(m_SlotBehaviour.m_ShowTempImages[row]
        //        .slotImages[col].transform.GetChild(2).GetComponent<ImageAnimation>(),
        //        m_SlotBehaviour.GetValueFromMatrix(row, col)
        //        );
        //}
    }

    private bool CheckFreeze(Transform m_Transform)
    {
        for (int i = 0; i < m_SlotBehaviour.m_Sticky.Count; i++)
        {
            if (m_SlotBehaviour.m_Sticky[i].m_Transform == m_Transform)
            {
                return true;
            }
        }
        return false;
    }

    internal bool GetFreezed(Transform m_transform, bool start_stop)
    {
        for (int i = 0; i < m_SlotBehaviour.m_Sticky.Count; i++)
        {
            if (m_SlotBehaviour.m_Sticky[i].m_Transform == m_transform)
            {
                if (start_stop && m_SlotBehaviour.m_Sticky[i].m_Count > 0)
                {
                    Sticky sticky = m_SlotBehaviour.m_Sticky[i];
                    //sticky.m_Count--;
                    m_SlotBehaviour.m_Sticky[i] = sticky;
                    if (m_SlotBehaviour.m_Sticky[i].m_Count == 0)
                    {
                        m_SlotBehaviour.m_Sticky.Remove(m_SlotBehaviour.m_Sticky[i]);
                        m_SlotBehaviour.m_Sticky.TrimExcess();
                    }
                    return true;
                }
                else
                {
                    if (!start_stop)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    internal void FreeSpinInitAnimation(bool startStop)
    {
        if (startStop)
        {
            m_FreeSpinInitAnimation.gameObject.SetActive(true);
            m_FreeSpinInitAnimation.StartAnimation();
        }
        else
        {
            m_FreeSpinInitAnimation.gameObject.SetActive(false);
            m_FreeSpinInitAnimation.StopAnimation();
        }
    }

    internal IEnumerator FreeSpinInitAnimRoutine()
    {
        FreeSpinInitAnimation(true);

        yield return new WaitForSeconds(2f);

        FreeSpinInitAnimation(false);
    }

    internal void ResetBonus()
    {
        isFreezeRunning = false;
        m_SlotBehaviour.m_Sticky.Clear();
        m_SlotBehaviour.m_Sticky.TrimExcess();
    }
}
