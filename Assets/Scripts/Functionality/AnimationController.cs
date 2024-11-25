using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Transform m_ParentSlotsHolder;
    [SerializeField]
    internal List<SlotImage> m_AnimatedSlots;
    [SerializeField]
    private List<AnimCords> m_Cords;
    [SerializeField]
    private SlotBehaviour m_SlotBehaviour;

    private Coroutine m_AnimationRoutine;
    private List<Tweener> m_SlotsAnim = new List<Tweener>();
    private bool m_PlayingAnimation = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartAnimation();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            StopAnimation();
        }
    }

    internal void StartAnimation()
    {
        m_PlayingAnimation = true;
        if(m_AnimationRoutine == null)
            m_AnimationRoutine = StartCoroutine(ActivateAllAnimation());
    }

    internal void StopAnimation()
    {
        m_PlayingAnimation = false;
        ResetAnimatedView();
        if(m_AnimationRoutine != null)
            StopCoroutine(m_AnimationRoutine);
        m_AnimationRoutine = null;
    }

    private IEnumerator ActivateAllAnimation()
    {
        while (m_PlayingAnimation)
        {
            foreach(var i in m_Cords)
            {
                foreach(var k in i.m_Cords)
                {
                    ActivateAnimatedView(k.i, k.j);
                }
            }
            yield return new WaitForSeconds(1f);
            foreach (var i in m_Cords)
            {
                foreach(var k in i.m_Cords)
                {
                    ActivateAnimatedView(k.i, k.j);
                }
                yield return new WaitForSeconds(2f);
                ResetAnimatedView();
            }
            yield return new WaitForSeconds(1f);
            ResetAnimatedView();
        }
    }

    private void ActivateAnimatedView(int i, int j)
    {
        if (!m_ParentSlotsHolder.gameObject.activeSelf)
        {
            ResetAnimatedView();
            m_ParentSlotsHolder.gameObject.SetActive(true);
        }
        m_AnimatedSlots[i].slotImages[j].gameObject.SetActive(true);
        m_SlotBehaviour.Tempimages[i].slotImages[j].gameObject.SetActive(false);
        //m_AnimatedSlots[i].slotImages[j].transform.DOScale(1.2f, 1);
        if(m_AnimatedSlots[i].slotImages[j].gameObject.GetComponent<ImageAnimation>().textureArray.Count > 0)
            m_AnimatedSlots[i].slotImages[j].gameObject.GetComponent<ImageAnimation>().StartAnimation();
        else
        {
            var tween = m_AnimatedSlots[i].slotImages[j].gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.15f, 1.15f), .8f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
            tween.Play();
            m_SlotsAnim.Add(tween);
        }
    }

    private void ResetAnimatedView()
    {
        //for(int i = 0; i < m_AnimatedSlots.Count; i++)
        //{
        //    for(int j = 0; j < m_AnimatedSlots[i].slotImages.Count; j++)
        //    {
        //        m_AnimatedSlots[i].slotImages[j].gameObject.SetActive(false);
        //        m_SlotBehaviour.Tempimages[i].slotImages[j].gameObject.SetActive(true);
        //        if (m_AnimatedSlots[i].slotImages[j].gameObject.GetComponent<ImageAnimation>().textureArray.Count > 0)
        //            m_AnimatedSlots[i].slotImages[j].gameObject.GetComponent<ImageAnimation>().StopAnimation();
        //        else
        //            m_AnimatedSlots[i].slotImages[j].transform.localScale = new Vector3(1, 1, 1);
        //    }
        //}
        //foreach(var i in m_SlotsAnim)
        //{
        //    i.Kill();
        //}
        //m_SlotsAnim.Clear();
        //m_SlotsAnim.TrimExcess();
        //m_ParentSlotsHolder.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct Cord
{
    public int i;
    public int j;
}

[System.Serializable]
public struct AnimCords
{
    public List<Cord> m_Cords;
}
