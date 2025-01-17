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
    internal Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    internal List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField]
    internal List<SlotImage> m_ShowTempImages = new List<SlotImage>();
    [SerializeField]
    private AnimationController m_AnimationController;

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;
    [SerializeField]
    internal Transform[] Slot_ShowTransform;

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
    private Button SlotStop_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
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
    private Sprite[] m_Seven_Sprites;
    [SerializeField]
    private Sprite[] m_Nine_Sprites;
    [SerializeField]
    private Sprite[] m_Ten_Sprites;
    [SerializeField]
    private Sprite[] m_A_Sprites;
    [SerializeField]
    private Sprite[] m_J_Sprites;
    [SerializeField]
    private Sprite[] m_K_Sprites;
    [SerializeField]
    private Sprite[] m_Q_Sprites;
    [SerializeField]
    private Sprite[] m_Star_Sprites;
    [SerializeField]
    private Sprite[] m_Wild_Sprites;
    [SerializeField]
    private Sprite[] m_Bonus_Sprites;
    [SerializeField]
    private Sprite[] m_StickyBonus_Sprites;
    [SerializeField]
    private Sprite[] m_Mystery_Sprites;
    [SerializeField]
    private Sprite[] m_MoonMystery_Sprites;
    [SerializeField]
    private Sprite[] m_Mini_Sprites;
    [SerializeField]
    private Sprite[] m_Minor_Sprites;
    [SerializeField]
    private Sprite[] m_Major_Sprites;
    [SerializeField]
    private Sprite[] m_Moon_Sprites;

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

    [Header("Deactivated BetPanel")]
    [SerializeField]
    private GameObject m_Deactivated_BetPanel;

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
    private List<Tweener> bonusTweens = new List<Tweener>();

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
    internal List<Sticky> m_Sticky = new List<Sticky>();

    [SerializeField]
    private Transform m_DoubleCover;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Coroutine TweenSpinning = null;
    private Coroutine CoverInitCoroutine = null;
    private Coroutine FreeSpinInitRoutine = null;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool IsAutoFreeSpin = false;
    private bool CheckSpinAudio = false;
    private bool IsStoppedSpin = false;
    internal bool CheckPopups = false;
    internal bool m_Is_Turtle = false;
    internal bool m_Is_Rabbit = true;
    internal bool m_Is_Cheetah = false;
    internal float m_Speed = 0.4f;
    internal int BetCounter = 0;
    private float SpinDelay;
    private double currentBalance = 0;
    private double currentTotalBet = 0;

    internal bool m_CheckEndTraversal = true;

    protected int Lines = 1;
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
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots();});

        if (SlotStop_Button) SlotStop_Button.onClick.RemoveAllListeners();
        if (SlotStop_Button) SlotStop_Button.onClick.AddListener(delegate { StopSlots();});

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
            //IsAutoFreeSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            //if (AutoSpin_Button) AutoSpin_Button.interactable = true;
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
        IsAutoFreeSpin = false;
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
        Debug.Log("Free Spin Started...");
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
        Debug.Log("Entered...");
        while (i < spinchances)
        {
            //uiManager.FreeSpins--;
            //if (FSnum_text) FSnum_text.text = (uiManager.FreeSpins).ToString();
            if (FSnum_text) FSnum_text.text = SocketManager.resultData.freeSpinCount.ToString();
            StartSlots();
            yield return tweenroutine;
            //yield return new WaitForSeconds(2);
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        BalanceUpdate();
        _bonusManager.ResetBonus();
        FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinExitAnimRoutine($"<b>You Won</b>\n"));
        yield return FreeSpinInitRoutine;
        StopFreeSpin();
        m_AnimationController.ResetAnimation();
        Debug.Log("Free Spin Ended...");
        if (IsAutoFreeSpin)
        {
            AutoSpin();
        }
        else
        {
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }

    private void StopFreeSpin()
    {
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        _bonusManager.ResetBonus();
        //if (IsAutoFreeSpin)
        //{
        //    AutoSpin();
        //}
        //else
        //{
        //    ToggleButtonGrp(true);
        //}
        IsFreeSpin = false;
        if(FreeSpinRoutine != null)
        {
            StopCoroutine(FreeSpinRoutine);
            FreeSpinRoutine = null;
        }
        //ResetAllAnimations();
        BalanceUpdate();
        Debug.Log(string.Concat("<color=red><b>", IsAutoFreeSpin, "</b></color>"));
        if (IsAutoFreeSpin)
        {
            AutoSpin();
        }
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
        if (audioController) audioController.PlayBetButton();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayBetButton();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
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
                BetCounter = SocketManager.initialData.Bets.Count - 1;
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
        if (audioController) audioController.PlayNormalButton();
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
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        //if (MaxBet_Button) MaxBet_Button.transform.GetChild(0).GetComponent<TMP_Text>().text = SocketManager.initialData.Bets[SocketManager.initialData.Bets.Count - 1].ToString();
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
        audioController.CheckFocusFunction(focus);
    }

    //function to populate animation sprites accordingly
    internal void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                foreach(Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 1:
                foreach (Sprite i in m_Nine_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 2:
                foreach (Sprite i in m_Ten_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 3:
                foreach (Sprite i in m_A_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 4:
                foreach (Sprite i in m_J_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 5:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 6:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 7:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 8:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 9:
                foreach (Sprite i in m_Wild_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 10:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 11:
                //foreach (Sprite i in m_Seven_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 12:
                foreach (Sprite i in m_Mystery_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 13:
                foreach (Sprite i in m_MoonMystery_Sprites) { animScript.textureArray.Add(i); }
                break;
            case 14:
                foreach (Sprite i in m_Mini_Sprites) { animScript.textureArray.Add(i); animScript.AnimationSpeed = 40f; }
                break;
            case 15:
                foreach (Sprite i in m_Minor_Sprites) { animScript.textureArray.Add(i); animScript.AnimationSpeed = 40f; }
                break;
            case 16:
                foreach (Sprite i in m_Major_Sprites) { animScript.textureArray.Add(i); animScript.AnimationSpeed = 40f; }
                break;
            case 17:
                foreach (Sprite i in m_Moon_Sprites) { animScript.textureArray.Add(i); }
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButton();

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

        if(!IsAutoSpin && !IsFreeSpin && !m_Is_Cheetah)
        {
            SlotStart_Button.gameObject.SetActive(false);
            SlotStop_Button.gameObject.SetActive(true);
        }
    }

    private void StopSlots()
    {
        if(!IsAutoSpin && !IsFreeSpin)
        {
            IsStoppedSpin = true;
            Debug.Log("Clicked On Stop Slots..." + IsStoppedSpin);
            SlotStart_Button.gameObject.SetActive(true);
            SlotStop_Button.gameObject.SetActive(false);
        }
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

        audioController.PlaySpinAudio(true);

        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        if (!IsFreeSpin)
            ResetAllAnimations();
        //ResetAllAnimations();

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
            InitializeTweening(Slot_Transform[i], false);
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

        _bonusManager.StartStickyBonus();
        _bonusManager.StartFreezeBonus();
        _bonusManager.StartMoonMysteryAndMystery();

        if (m_Is_Turtle)
        {
            yield return new WaitForSeconds(0.4f);
        }
        else if (m_Is_Rabbit)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else if (m_Is_Cheetah)
        {
            yield return new WaitForSeconds(0f);
        }
        if(!m_Is_Cheetah)
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (IsStoppedSpin)
                {
                    break;
                }
            }
            SlotStop_Button.gameObject.SetActive(false);
            SlotStart_Button.gameObject.SetActive(true);
        }

        PopulateResult();
        TweenSpinning = StartCoroutine(LevelOrderTraversal(IsStoppedSpin));

        yield return TweenSpinning;

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(6, Slot_Transform[i], i, IsStoppedSpin, false);
        }

        IsStoppedSpin = false;
        yield return alltweens[^1].WaitForCompletion();
        //StopSlots();

        audioController.PlaySpinAudio(false);

        //yield return new WaitForSeconds(0.3f);

        StopCoroutine(TweenSpinning);

        yield return new WaitForSeconds(0.1f);
        if (SocketManager.playerdata.currentWining > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }

        //HACK: Instruction Updated After Spin Ends If Wins then it shouldn't be updated other wise it will prompt 0th index
        TotalWin_text.text = m_Instructions[0];

        //HACK: Check For The Result And Activate Animations Accordingly
        //CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        //m_AnimationController.StartAnimation();
        //PopulateResult();

        //if (SocketManager.playerdata.currentWining > 0)
        //{
        //    m_AnimationController.StartAnimation();
        //}

        //HACK: Kills The Tweens So That They Will Get Ready For Next Spin
        KillAllTweens();

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("F3");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");

        currentBalance = SocketManager.playerdata.Balance;

        //if (SocketManager.resultData.jackpot > 0)
        //{
        //    uiManager.PopulateWin(4, SocketManager.resultData.jackpot);
        //    yield return new WaitUntil(() => !CheckPopups);
        //    CheckPopups = true;
        //}

        //if (SocketManager.resultData.isBonus)
        //{
        //    CheckBonusGame();
        //}
        //else
        //{
        //}
        CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);

        Debug.Log(_bonusManager.isMysteryRunning);

        yield return new WaitUntil(() => !_bonusManager.isMysteryRunning);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            IsSpinning = false;
        }

        Debug.Log("Checking For Free Spin...");

        if (SocketManager.resultData.isFreeSpin && !SocketManager.resultData.isMoonJackpot && !SocketManager.resultData.isGrandPrize)
        {
            if (!IsFreeSpin)
            {
                FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinInitAnimRoutine());
                //yield return FreeSpinInitRoutine;
                //uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);
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
                    //FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinInitAnimRoutine());
                    uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);
                    //yield return FreeSpinInitRoutine;
                    //uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);
                }
            }
            if (IsAutoSpin)
            {
                Debug.Log("Free Spin In Auto Spin Received...");
                IsAutoFreeSpin = true;
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            if(SocketManager.resultData.isMoonJackpot)
            {
                audioController.PlayWin(Sound.BigWin);
                FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinExitAnimRoutine("<b>Received</b>\nMoon Jackpot"));
                yield return FreeSpinInitRoutine;
                StopFreeSpin();
                m_AnimationController.ResetAnimation();
                if (!IsAutoFreeSpin) ToggleButtonGrp(true);
                BalanceUpdate();
                Debug.Log("Moon Jackpot Received...");
            }
            else if (SocketManager.resultData.isGrandPrize)
            {
                audioController.PlayWin(Sound.MegaWin);
                FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinExitAnimRoutine("<b>Received</b>\nGrand Jackpot"));
                yield return FreeSpinInitRoutine;
                StopFreeSpin();
                m_AnimationController.ResetAnimation();
                if (!IsAutoFreeSpin) ToggleButtonGrp(true);
                BalanceUpdate();
                Debug.Log("Grand Prize Received...");
            }
            else
            {
                BalanceUpdate();
            }
            //else if (IsFreeSpin)
            //{
            //    FreeSpinInitRoutine = StartCoroutine(_bonusManager.FreeSpinExitAnimRoutine($"<b>You Won</b>\n"));
            //    yield return FreeSpinInitRoutine;
            //    StopFreeSpin();
            //    m_AnimationController.ResetAnimation();
            //    Debug.Log("Free Spin Ended...");
            //}
        }

    }

    private void PopulateResult()
    {
        int row = 0;
        int col = 0;
        for (int i = 0; i < SocketManager.resultData.symbolsToEmit.Count; i++)
        {
            //Debug.Log(SocketManager.resultData.symbolsToEmit[i].Split(',')[0] + " " + SocketManager.resultData.symbolsToEmit[i].Split(',')[1]);
            row = int.Parse(SocketManager.resultData.symbolsToEmit[i].Split(',')[0]);
            col = int.Parse(SocketManager.resultData.symbolsToEmit[i].Split(',')[1]);
            PopulateAnimationSprites(m_ShowTempImages[col]
                .slotImages[row].transform.GetChild(2).GetComponent<ImageAnimation>(),
                GetValueFromMatrix(row, col)
                );
            //Debug.Log(row + " " + col + " " + GetValueFromMatrix(row, col));
        }

        if (SocketManager.resultData.isAllWild)
        {
            if (TotalWin_text) TotalWin_text.text = (SocketManager.initialData.allWildMultiplier * SocketManager.initialData.Bets[BetCounter]).ToString();
        }
    }

    internal int GetValueFromMatrix(int row, int col)
    {
        int value = 0;
        value = int.Parse(SocketManager.resultData.ResultReel[row][col]);
        return value;
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

    private IEnumerator LevelOrderTraversal(bool StopImmidiate)
    {
        for(int i = 0; i < m_ShowTempImages.Count; i++)
        {
            m_CheckEndTraversal = false;
            for(int j = 0; j < m_ShowTempImages[i].slotImages.Count; j++)
            {
                //yield return new WaitForSeconds(0.08f);
                if (!StopImmidiate)
                {
                    if (m_Is_Turtle)
                    {
                        yield return new WaitForSeconds(0.06f);
                    }
                    else if (m_Is_Rabbit)
                    {
                        yield return new WaitForSeconds(0.04f);
                    }
                    else if (m_Is_Cheetah)
                    {
                        yield return new WaitForSeconds(0.02f);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0f);
                }

                m_ShowTempImages[j].slotImages[i].gameObject.SetActive(true);
                if (IsFreeSpin || SocketManager.resultData.isFreeSpin)
                {
                    if(SocketManager.resultData.BonusResultReel.Count > 0)
                    {
                        m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<Image>().sprite = myImages[SocketManager.resultData.BonusResultReel[i][j]];

                        Debug.Log(string.Concat("<color=yellow><b>", $"Bonus Result Reel Is Not Empty: {SocketManager.resultData.BonusResultReel.Count}", "</b></color>"));

                        if (SocketManager.resultData.BonusResultReel[i][j] > 13 && SocketManager.resultData.BonusResultReel[i][j] < 17)
                        {
                            Debug.Log(string.Concat("<color=yellow><b>", $"Found The Element In It: {SocketManager.resultData.BonusResultReel.Count}", "</b></color>"));
                            PopulateAnimationSprites(m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>(), SocketManager.resultData.BonusResultReel[i][j]);
                            if (m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().textureArray.Count > 0)
                            {
                                Vector2 size = new Vector2(430, 430);
                                if(m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<RectTransform>().sizeDelta != size)
                                {
                                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = size;
                                }
                                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().StartAnimation();
                            }
                        }
                    }
                    else
                    {
                        m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<Image>().sprite = myImages[int.Parse(SocketManager.resultData.ResultReel[i][j])];
                    }
                    if (!_bonusManager.GetFreezed(m_ShowTempImages[j].slotImages[i].transform, false))
                        InitializeShowTweening(m_ShowTempImages[j].slotImages[i].transform.GetChild(2));

                    //Debug.Log(string.Concat("<color=yellow><b>", "Bonus Executed...", "</b></color>"));

                }
                else
                {
                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<Image>().sprite = myImages[int.Parse(SocketManager.resultData.ResultReel[i][j])];
                    if (!_bonusManager.GetSticky(m_ShowTempImages[j].slotImages[i].transform, false))
                        InitializeShowTweening(m_ShowTempImages[j].slotImages[i].transform.GetChild(2));

                    //Debug.Log(string.Concat("<color=red><b>", "Bonus Not Executed...", "</b></color>"));
                }
                ImageAnimation anim = m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>();
                m_ShowTempImages[j].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StartAnimation();
                if(anim.textureArray.Count > 0)
                    anim.StartAnimation();
            }
            //yield return new WaitForSeconds(0.2f);
            if (!StopImmidiate)
            {
                if (m_Is_Turtle)
                {
                    yield return new WaitForSeconds(0.6f);
                }
                else if (m_Is_Rabbit)
                {
                    yield return new WaitForSeconds(0.4f);
                }
                else if (m_Is_Cheetah)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                yield return new WaitForSeconds(0f);
            }
        }
        m_CheckEndTraversal = true;
    }

    private void StopLevelOrderTraversal()
    {
        for(int i = 0; i < m_ShowTempImages.Count; i++)
        {
            for(int j = 0; j < m_ShowTempImages[i].slotImages.Count; j++)
            {
                if(!_bonusManager.GetSticky(m_ShowTempImages[i].slotImages[j].transform, true))
                {
                    m_ShowTempImages[i].slotImages[j].gameObject.SetActive(false);
                    m_ShowTempImages[i].slotImages[j].transform.GetChild(1).GetComponent<ImageAnimation>().StopAnimation();
                    m_ShowTempImages[i].slotImages[j].transform.GetChild(3).gameObject.SetActive(false);
                    m_ShowTempImages[i].slotImages[j].transform.GetChild(4).gameObject.SetActive(false);
                }
            }
        }
    }

    private void ResetAllAnimations()
    {
        for(int i = 0; i < m_ShowTempImages.Count; i++)
        {
            for (int j = 0; j < m_ShowTempImages[i].slotImages.Count; j++)
            {
                m_ShowTempImages[i].slotImages[j].transform.GetChild(2).GetComponent<ImageAnimation>().textureArray.Clear();
                m_ShowTempImages[i].slotImages[j].transform.GetChild(2).GetComponent<ImageAnimation>().textureArray.TrimExcess();

                Vector2 size = new Vector2(430, 430);
                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(230, 230);
                if(IsFreeSpin)
                {
                    if(m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<RectTransform>().sizeDelta == size)
                    {
                        //m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = size;
                        m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().StartAnimation();
                    }
                }
                else
                {
                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().StopAnimation();
                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().textureArray.Clear();
                    m_ShowTempImages[j].slotImages[i].transform.GetChild(2).GetComponent<ImageAnimation>().textureArray.TrimExcess();
                }
            }
        }

        m_AnimationController.ResetAnimation();
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
            if (Balance_text) Balance_text.text = initAmount.ToString("F3");
        });
        currentBalance = balance;
    }

    internal void BalanceUpdate()
    {
        if (TotalWin_text) TotalWin_text.text = (SocketManager.playerdata.currentWining).ToString("F3");
    }

    internal void CheckWinPopups()
    {
        //if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        //{
        //    uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        //}
        //else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        //{
        //    uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        //}
        //else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        //{
        //    uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        //}
        //else
        //{
        //    CheckPopups = false;
        //}
        if(SocketManager.playerdata.currentWining > 0)
        {
            uiManager.PopulateWin(1, SocketManager.playerdata.currentWining);
            m_AnimationController.StartAnimation();
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        //_bonusManager.StartBonus((int)SocketManager.resultData.BonusStopIndex);
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
        if (m_Deactivated_BetPanel) m_Deactivated_BetPanel.SetActive(!toggle);
        //uiManager.EnableDisableSpeed(toggle);
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

    //private void ResetRectSizes()
    //{
    //    for(int i = 0; i < Tempimages.Count; i++)
    //    {
    //        for(int j = 0; j < Tempimages[i].slotImages.Count; j++)
    //        {
    //            Tempimages[i].slotImages[j].rectTransform.sizeDelta = new Vector2(242, 185);
    //            m_AnimationController.m_AnimatedSlots[i].slotImages[j].rectTransform.sizeDelta = new Vector2(242, 185);
    //        }
    //    }

    //    foreach(var i in m_UI_Order)
    //    {
    //        //i.current_object.SetParent(i.this_parent);
    //        i.current_object.SetSiblingIndex(i.child_index);
    //        i.current_object.localPosition = i.current_position;
    //    }

    //    m_UI_Order.Clear();
    //    m_UI_Order.TrimExcess();
    //}

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
    private void InitializeTweening(Transform slotTransform, bool isBonus)
    {
        //slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, slotTransform.localPosition.y);
        Tweener tweener = slotTransform.DOLocalMoveY(-(tweenHeight), m_Speed).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
        tweener.Play();
        if (!isBonus)
            alltweens.Add(tweener);
        else
            bonusTweens.Add(tweener);
    }

    internal void InitializeShowTweening(Transform slotTransform)
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

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop, bool isBonus)
    {
        if (!isBonus)
            alltweens[index].Pause();
        else
            bonusTweens[index].Pause();

        int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));

        if (!isBonus)
        {
            alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutQuad);
        }
        else
        {
            bonusTweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutQuad);
        }
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }

    internal void InitBonusTween()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i], true);
            //yield return new WaitForSeconds(0.1f);
        }
    }

    internal IEnumerator StopBonusTween()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(6, Slot_Transform[i], i, IsStoppedSpin, true);
        }

        KillBonusTweens();
    }

    private void KillBonusTweens()
    {
        for(int i = 0; i < numberOfSlots; i++)
        {
            bonusTweens[i].Kill();
        }
        bonusTweens.Clear();
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