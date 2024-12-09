using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    internal List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField]
    internal List<SlotImage> m_ShowTempImages;
    [SerializeField]
    private AnimationController m_AnimationController;

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;
    [SerializeField]
    private Transform[] Slot_ShowTransform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button m_BetButton;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Cat_Sprite;
    [SerializeField]
    private Sprite[] Eagle_Sprite;
    [SerializeField]
    private Sprite[] Bear_Sprite;
    [SerializeField]
    private Sprite[] Wolf_Sprite;
    [SerializeField]
    private Sprite[] Buffalo_Sprite;
    [SerializeField]
    private Sprite[] Gold_Buffalo;
    [SerializeField]
    private Sprite[] Landscape_Sprite;
    [SerializeField]
    private Sprite[] Bonus_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private GameObject m_CoverPanel;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField]
    private BonusController _bonusManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    [SerializeField]
    private List<string> m_Instructions;

    [SerializeField]
    private List<OrderingUI> m_UI_Order = new List<OrderingUI>();

    [SerializeField]
    private List<Sticky> m_Sticky = new List<Sticky>();

    [SerializeField]
    private Transform m_DoubleCover;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Coroutine TweenSpinning = null;
    private Coroutine CoverInitCoroutine = null;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal bool m_Is_Turtle = false;
    internal bool m_Is_Rabbit = true;
    internal bool m_Is_Cheetah = false;
    internal float m_Speed = 0.4f;
    internal int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    [SerializeField]
    private int SpaceFactor = 0;
    private int numberOfSlots = 4;          //number of columns

    private void Awake()
    {
        currentBalance = 160.2346;
        Balance_text.text = currentBalance.ToString();
    }

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(delegate { MaxBet(); uiManager.LastBetCounter(); });

        //if (m_BetButton) m_BetButton.onClick.RemoveAllListeners();
        //if (m_BetButton) m_BetButton.onClick.AddListener(() =>
        //{
        //    uiManager.OpenBetPanel();
        //});

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.interactable = false;

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.interactable = true;
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(2);
            i++;
            if (FSnum_text) FSnum_text.text = (spinchances - i).ToString();
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }

    #region LinesCalculation
    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        StaticLine_Texts[count].text = (count + 1).ToString();
        StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    #endregion

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 2)
            {
                BetCounter++;
                uiManager.SelectBetButton(IncDec);
            }
            else
            {
                BetCounter = 0;
                uiManager.StartBetCounter();
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
                uiManager.SelectBetButton(IncDec);
            }
            else
            {
                BetCounter = SocketManager.initialData.Bets.Count - 2;
                uiManager.LastBetCounter();
            }
        }

        uiManager.UpdateExternalPaytableValue();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    internal void OnBetClicked(int Bet, double Value)
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = Bet;
        if (LineBet_text) LineBet_text.text = Value.ToString();
        if (TotalBet_text) TotalBet_text.text = (Value).ToString();
        currentTotalBet = Value;
        CompareBalance();
    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        if (MaxBet_Button) MaxBet_Button.transform.GetChild(0).GetComponent<TMP_Text>().text = SocketManager.initialData.Bets[SocketManager.initialData.Bets.Count - 1].ToString();
        //uiManager.LoadBetButtons(true);
        //uiManager.NextBets();
        uiManager.StartBetCounter();
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        //_bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 6:
                for (int i = 0; i < Cat_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Cat_Sprite[i]);
                }
                animScript.AnimationSpeed = 60f;
                break;
            case 7:
                for (int i = 0; i < Eagle_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Eagle_Sprite[i]);
                }
                animScript.AnimationSpeed = 60f;
                break;
            case 8:
                for (int i = 0; i < Bear_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bear_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 9:
                for (int i = 0; i < Wolf_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wolf_Sprite[i]);
                }
                animScript.AnimationSpeed = 60f;
                break;
            case 10:
                for (int i = 0; i < Buffalo_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Buffalo_Sprite[i]);
                }
                animScript.AnimationSpeed = 50f;
                break;
            case 11:
                for (int i = 0; i < Gold_Buffalo.Length; i++)
                {
                    animScript.textureArray.Add(Gold_Buffalo[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;
            case 12:
                for (int i = 0; i < Landscape_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Landscape_Sprite[i]);
                }
                animScript.AnimationSpeed = 50f;
                break;
            case 13:
                for (int i = 0; i < Bonus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bonus_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) 
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        //ResetRectSizes();
        StopLevelOrderTraversal();//HACK: Needed to be uncommented when needed

        //m_AnimationController.StopAnimation();

        TotalWin_text.text = m_Instructions[1];

        //StartTweeningShow();
        //yield return new WaitForSeconds(0.1f);

        //CoverInitCoroutine = StartCoroutine(InitCoverTween());

        //yield return CoverInitCoroutine;

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            //yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }

        //HACK: This will be used when to send the spin instruction to the socket and wait for the socket to receive the request.
        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);


        for(int i = 0; i < Tempimages.Count; i++)
        {
            for(int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].sprite = myImages[int.Parse(SocketManager.resultData.ResultReel[i][j])];
            }
        }

        StartStickyBonus();

        yield return new WaitForSeconds(m_Is_Turtle ? 0.7f : m_Is_Rabbit ? 0.5f : 0.3f);

        TweenSpinning = StartCoroutine(LevelOrderTraversal());

        yield return TweenSpinning;

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(6, Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);

        StopCoroutine(TweenSpinning);

        //HACK: Instruction Updated After Spin Ends If Wins then it shouldn't be updated other wise it will prompt 0th index
        TotalWin_text.text = m_Instructions[0];

        //HACK: Check For The Result And Activate Animations Accordingly
        //CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        //m_AnimationController.StartAnimation();
        PopulateResult();

        //HACK: Kills The Tweens So That They Will Get Ready For Next Spin
        KillAllTweens();

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        currentBalance = SocketManager.playerdata.Balance;

        if (SocketManager.resultData.jackpot > 0)
        {
            uiManager.PopulateWin(4, SocketManager.resultData.jackpot);
            yield return new WaitUntil(() => !CheckPopups);
            CheckPopups = true;
        }

        if (SocketManager.resultData.isBonus)
        {
            CheckBonusGame();
        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }
        if (SocketManager.resultData.isFreeSpin)
        {
            if (!IsFreeSpin)
            {
                uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);
            }
            else
            {
                if (SocketManager.resultData.freeSpinAdded)
                {
                    IsFreeSpin = false;
                    if (FreeSpinRoutine != null)
                    {
                        StopCoroutine(FreeSpinRoutine);
                        FreeSpinRoutine = null;
                    }
                    uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);
                }
            }
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void PopulateResult()
    {
        //for(int i = 0; i < SocketManager.resultData.FinalsymbolsToEmit.Count; i++)
        //{
        //    PopulateAnimationSprites(m_ShowTempImages[SocketManager.resultData.FinalsymbolsToEmit[i][0]]
        //        .slotImages[SocketManager.resultData.FinalsymbolsToEmit[i][1]],

        //        );
        //}
    }

    private IEnumerator InitCoverTween()
    {
        Vector3 location = m_CoverPanel.transform.position;
        DOTweenUIManager.Instance.MoveDir(m_CoverPanel.transform, location.y + 2, "Y", 0.5f, () =>
        {
            DOTweenUIManager.Instance.MoveDir(m_CoverPanel.transform, location.y - 8, "Y", 0.5f, () =>
            {
                m_CoverPanel.transform.position = location;
            });
        });
        yield return new WaitForSeconds(0.8f);
        StopLevelOrderTraversal();//HACK: Needed to be uncommented when needed
    }

    private IEnumerator LevelOrderTraversal()
    {
        for(int i = 0; i < m_ShowTempImages.Count; i++)
        {
            for(int j = 0; j < m_ShowTempImages[i].slotImages.Count; j++)
            {
                yield return new WaitForSeconds(0.08f);
                m_ShowTempImages[j].slotImages[i].gameObject.SetActive(true);
                m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<Image>().sprite = myImages[int.Parse(SocketManager.resultData.ResultReel[i][j])];
                if (!GetSticky(m_ShowTempImages[j].slotImages[i].transform, false))
                    InitializeShowTweening(m_ShowTempImages[j].slotImages[i].transform.GetChild(2));
                m_ShowTempImages[j].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StartAnimation();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void StartStickyBonus()
    {
        int row = 0;
        int col = 0;
        if (SocketManager.resultData.isStickyBonus)
        {
            for(int i = 0; i < SocketManager.resultData.stickyBonusValue.Count; i ++)
            {
                row = SocketManager.resultData.stickyBonusValue[i].position[0];
                col = SocketManager.resultData.stickyBonusValue[i].position[1];

                if(!CheckSticky(m_ShowTempImages[row].slotImages[col].transform))
                    m_Sticky.Add(new Sticky
                    {
                        m_Transform = m_ShowTempImages[row].slotImages[col].transform, m_Count = SocketManager.resultData.stickyBonusValue[i].value
                    });
            }
        }
    }

    private void StartMoonMysteryBonus()
    {
        if (SocketManager.resultData.isMoonJackpot)
        {
            for(int i = 0; i < SocketManager.resultData.moonMysteryData.Count; i++)
            {

            }
        }
    }

    private void StopLevelOrderTraversal()
    {
        for(int i = 0; i < m_ShowTempImages.Count; i++)
        {
            for(int j = 0; j < m_ShowTempImages[i].slotImages.Count; j++)
            {
                if(!GetSticky(m_ShowTempImages[i].slotImages[j].transform, true))
                {
                    m_ShowTempImages[i].slotImages[j].gameObject.SetActive(false);
                }
            }
        }
    }

    private bool CheckSticky(Transform m_Transform)
    {
        for(int i = 0; i < m_Sticky.Count; i++)
        {
            if(m_Sticky[i].m_Transform == m_Transform)
            {
                return true;
            }
        }
        return false;
    }

    private bool GetSticky(Transform m_transform, bool start_stop)
    {
        for(int i = 0; i < m_Sticky.Count; i ++)
        {
            if(m_Sticky[i].m_Transform == m_transform)
            {
                if (start_stop && m_Sticky[i].m_Count > 0)
                {
                    Sticky sticky = m_Sticky[i];
                    sticky.m_Count--;
                    m_Sticky[i] = sticky;
                    if(m_Sticky[i].m_Count == 0)
                    {
                        m_Sticky.Remove(m_Sticky[i]);
                        m_Sticky.TrimExcess();
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

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
        currentBalance = balance;
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        _bonusManager.StartBonus((int)SocketManager.resultData.BonusStopIndex);
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            if (jackpot <= 0)
            {
                if (audioController) audioController.PlayWLAudio("win");
            }

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            if (jackpot > 0)
            {
                if (audioController) audioController.PlayWLAudio("megaWin");
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(m_ShowTempImages[i].slotImages[k].transform.GetChild(2).gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(m_ShowTempImages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].transform.GetChild(2).gameObject);
                        }
                        else
                        {
                            StartGameAnimation(m_ShowTempImages[0].slotImages[points_anim[k]].transform.GetChild(2).gameObject);
                        }
                    }
                }
            }
            WinningsAnim(true);
        }
        else
        {

            //if (audioController) audioController.PlayWLAudio("lose");
            if (audioController) audioController.StopWLAaudio();
        }
        CheckSpinAudio = false;
    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }


    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        if (m_BetButton) m_BetButton.interactable = toggle;
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        if(temp.textureArray.Count > 0)
        {
            temp.StartAnimation();
            TempList.Add(temp);
        }
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }

    private void ResetRectSizes()
    {
        for(int i = 0; i < Tempimages.Count; i++)
        {
            for(int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].rectTransform.sizeDelta = new Vector2(242, 185);
                m_AnimationController.m_AnimatedSlots[i].slotImages[j].rectTransform.sizeDelta = new Vector2(242, 185);
            }
        }

        foreach(var i in m_UI_Order)
        {
            //i.current_object.SetParent(i.this_parent);
            i.current_object.SetSiblingIndex(i.child_index);
            i.current_object.localPosition = i.current_position;
        }

        m_UI_Order.Clear();
        m_UI_Order.TrimExcess();
    }

    private void PrioritizeList()
    {
        //m_UI_Order.Sort((x, y) => y.m_Priority.CompareTo(x.m_Priority)); //Descending
        m_UI_Order.Sort((x, y) => x.m_Priority.CompareTo(y.m_Priority)); //Asscending

        foreach(var i in m_UI_Order)
        {
            i.current_object.SetAsLastSibling();
        }

    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        //slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, slotTransform.localPosition.y);
        Tweener tweener = slotTransform.DOLocalMoveY(-(tweenHeight), m_Speed).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private void InitializeShowTweening(Transform slotTransform)
    {
        Vector3 original_position = slotTransform.position;
        original_position.y += 0.2f;
        slotTransform.position = original_position;
        Tweener tween = slotTransform.DOMoveY(original_position.y - 0.2f, 0.2f).SetEase(Ease.OutBounce);
        tween.Play();
    }

    private void StartTweeningShow()
    {
        for (int i = 0; i < Slot_ShowTransform.Length; i++)
        {
            DOTweenUIManager.Instance.Jump(Slot_ShowTransform[i].GetComponent<RectTransform>(), 80f, 1, 0.4f, () => { if (i == Slot_ShowTransform.Length) StopLevelOrderTraversal(); });
        }
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        //int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));
        //alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public struct Sticky
{
    public Transform m_Transform;
    public int m_Count;
}

[Serializable]
public struct OrderingUI
{
    public Priority m_Priority;
    public int child_index;
    public Transform this_parent;
    public Transform current_object;
    public Vector3 current_position;
}

[Serializable]
public enum Priority
{
    Gold_Buffalo = 7,
    Landscape = 6,
    Buffalo = 5,
    Bear = 4,
    Wolf = 3,
    Lion = 2,
    Eagle = 1
}