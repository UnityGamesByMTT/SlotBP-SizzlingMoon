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
    [SerializeField]
    private ImageAnimation m_FreeSpinExitAnimation;
    [SerializeField]
    private List<GameObject> m_ListOfMystery = new List<GameObject>();

    private bool isFreezeRunning = false;
    internal bool isMysteryRunning = false;

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
                    if(m_SocketManager.resultData.stickyBonusValue[i].value > 0)
                        m_SlotBehaviour.m_Sticky.Add(new Sticky
                        {
                            m_Transform = m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform,
                            m_Count = m_SocketManager.resultData.stickyBonusValue[i].value
                        });

                //HACK: Note that I prefer using class instead for accessing the gameobjects.
                //NOTE: The below code line is to enable the sticky bonus internal hole
                m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(3).gameObject.SetActive
                    (
                        m_SocketManager.resultData.stickyBonusValue[i].symbol == 11 ? m_SocketManager.resultData.stickyBonusValue[i].value > 0 ? true : false : false
                    );
                //NOTE: The below code line is to enable the score text
                m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(4).gameObject.SetActive(true);
                m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = m_SocketManager.resultData.stickyBonusValue[i].value.ToString();
                m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(4).GetComponent<TMP_Text>().text = m_SocketManager.resultData.stickyBonusValue[i].prizeValue.ToString();
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
                if (start_stop && m_SlotBehaviour.m_Sticky[i].m_Count >= 0)
                {
                    Sticky sticky = m_SlotBehaviour.m_Sticky[i];
                    sticky.m_Count--;
                    m_SlotBehaviour.m_Sticky[i] = sticky;
                    if (m_SlotBehaviour.m_Sticky[i].m_Count == -1)
                    {
                        m_SlotBehaviour.m_Sticky[i].m_Transform.GetChild(3).gameObject.SetActive(false);
                        m_SlotBehaviour.m_Sticky.Remove(m_SlotBehaviour.m_Sticky[i]);
                        m_SlotBehaviour.m_Sticky.TrimExcess();
                        return false;
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
                //if(m_SocketManager.resultData.frozenIndices[i].symbol > 11 && m_SocketManager.resultData.frozenIndices[i].symbol < 18)
                if(m_SocketManager.resultData.frozenIndices[i].symbol < 12)
                {
                    m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(4).gameObject.SetActive(true);
                    m_SlotBehaviour.m_ShowTempImages[row].slotImages[col].transform.GetChild(4).GetComponent<TMP_Text>().text = m_SocketManager.resultData.frozenIndices[i].prizeValue.ToString();
                }
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

    internal void StartMoonMysteryAndMystery()
    {
        if(m_SocketManager.resultData.freeSpinCount == 0)
        {
            if (m_SocketManager.resultData.moonMysteryData.Count > 0)
            {
                StartCoroutine(Mystery());
            }
        }
    }


    //HACK: This Mystery Method Needs To Be Called When We Are Triggering Mystery This Is The Core Method
    private IEnumerator Mystery()
    {
        yield return new WaitUntil(() => m_SlotBehaviour.m_CheckEndTraversal);
        yield return new WaitForSeconds(2f);

        Debug.Log("<color=red>Continuing The Traversal And Now Started Moon Mystery and Mystery</color>");

        isMysteryRunning = true;
        m_SlotBehaviour.InitBonusTween();

        List<List<int>> data = m_SocketManager.resultData.moonMysteryData;

        for (int i = 0; i < m_SocketManager.resultData.moonMysteryData.Count; i++)
        {
            m_ListOfMystery.Add(m_SlotBehaviour.m_ShowTempImages[data[i][1]].slotImages[data[i][2]].gameObject);
            Debug.Log(data[i][1] + " " + data[i][2] + " " + data[i][4]);
        }
        foreach(var i in m_ListOfMystery)
        {
            i.transform.GetChild(0).gameObject.SetActive(false);
            i.transform.GetChild(1).gameObject.SetActive(false);
            i.transform.GetChild(2).gameObject.SetActive(false);
            //RunTween(i.transform.GetChild(2));
        }

        yield return new WaitForSeconds(1f);

        for(int i = 0; i < m_SocketManager.resultData.moonMysteryData.Count; i++)
        {
            m_SlotBehaviour.m_ShowTempImages[data[i][1]].slotImages[data[i][2]].transform.GetChild(2).GetComponent<Image>().sprite = m_SlotBehaviour.myImages[data[i][4]];
        }

        yield return new WaitForSeconds(2f);

        foreach(var i in m_ListOfMystery)
        {
            i.transform.GetChild(0).gameObject.SetActive(true);
            i.transform.GetChild(1).gameObject.SetActive(true);
            i.transform.GetChild(2).gameObject.SetActive(true);
            i.transform.GetChild(1).GetComponent<ImageAnimation>().StartAnimation();
            m_SlotBehaviour.InitializeShowTweening(i.transform.GetChild(2));

            //yield return new WaitForSeconds(0.6f);
        }

        yield return new WaitForSeconds(0.8f);

        yield return m_SlotBehaviour.StopBonusTween();

        m_ListOfMystery.Clear();
        m_ListOfMystery.TrimExcess();

        isMysteryRunning = false;
    }

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

    internal void FreeSpinExitAnimation(bool startStop)
    {
        if (startStop)
        {
            m_FreeSpinExitAnimation.gameObject.SetActive(true);
            m_FreeSpinExitAnimation.StartAnimation();
        }
        else
        {
            m_FreeSpinExitAnimation.gameObject.SetActive(false);
            m_FreeSpinExitAnimation.StopAnimation();
        }
    }

    internal IEnumerator FreeSpinInitAnimRoutine()
    {
        Debug.Log("Starting Free Spin...");
        FreeSpinInitAnimation(true);

        yield return new WaitForSeconds(2f);

        m_UIManager.FreeSpinProcess((int)m_SocketManager.resultData.freeSpinCount);

        FreeSpinInitAnimation(false);
    }

    internal IEnumerator FreeSpinExitAnimRoutine(string m_winningtype)
    {
        FreeSpinExitAnimation(true);

        m_SocketManager.AccumulateResult(m_SlotBehaviour.BetCounter);
        yield return new WaitUntil(() => m_SocketManager.isResultdone);

        yield return new WaitForSeconds(1f);

        m_FreeSpinExitAnimation.transform.GetChild(0).gameObject.SetActive(true);
        DOTweenUIManager.Instance.FadeIn(m_FreeSpinExitAnimation.transform.GetChild(0).GetComponent<CanvasGroup>(), 1f);
        m_FreeSpinExitAnimation.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = m_winningtype;
        m_FreeSpinExitAnimation.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = ((double)m_SocketManager.playerdata.currentWining).ToString();

        yield return new WaitForSeconds(4f);

        m_FreeSpinExitAnimation.transform.GetChild(0).gameObject.SetActive(false);
        FreeSpinExitAnimation(false);
    }

    internal void ResetBonus()
    {
        isFreezeRunning = false;
        m_SlotBehaviour.m_Sticky.Clear();
        m_SlotBehaviour.m_Sticky.TrimExcess();
    }
}
