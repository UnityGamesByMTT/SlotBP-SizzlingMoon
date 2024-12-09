using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{

    [Header("Menu UI")]
    [SerializeField]
    private Button Menu_Button;
    [SerializeField]
    private GameObject Menu_Object;
    [SerializeField]
    private RectTransform Menu_RT;
    [SerializeField]
    private GameObject Info_Object;
    [SerializeField]
    private Button Info_Button;
    [SerializeField]
    private Button Info_Exit;
    [SerializeField]
    private RectTransform Info_RT;

    //[SerializeField]
    //private Button About_Button;
    //[SerializeField]
    //private GameObject About_Object;
    //[SerializeField]
    //private RectTransform About_RT;

    [Header("Settings UI")]
    [SerializeField]
    private Button Settings_Button;
    [SerializeField]
    private GameObject Settings_Object;
    [SerializeField]
    private RectTransform Settings_RT;
    [SerializeField]
    private Button Terms_Button;
    [SerializeField]
    private Button Privacy_Button;

    [SerializeField]
    private Button Exit_Button;
    [SerializeField]
    private GameObject Exit_Object;
    [SerializeField]
    private RectTransform Exit_RT;

    [SerializeField]
    private Button Paytable_Button;
    [SerializeField]
    private GameObject Paytable_Object;
    [SerializeField]
    private RectTransform Paytable_RT;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;
    [SerializeField]
    private Image AboutLogo_Image;
    [SerializeField]
    private Button Support_Button;

    [Header("Paytable Popup")]
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private TMP_Text[] SymbolsText;
    [SerializeField]
    private TMP_Text FreeSpin_Text;
    [SerializeField]
    private TMP_Text Scatter_Text;
    [SerializeField]
    private TMP_Text Jackpot_Text;
    [SerializeField]
    private TMP_Text Bonus_Text;
    [SerializeField]
    private TMP_Text Wild_Text;

    [Header("Settings Popup")]
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Button Sound_Button;
    [SerializeField]
    private Button Music_Button;

    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;

    [Header("Win Popup")]
    [SerializeField]
    private Sprite BigWin_Sprite;
    [SerializeField]
    private Sprite HugeWin_Sprite;
    [SerializeField]
    private Sprite MegaWin_Sprite;
    [SerializeField]
    private Sprite Jackpot_Sprite;
    [SerializeField]
    private Image Win_Image;
    [SerializeField]
    private GameObject WinPopup_Object;
    [SerializeField]
    private TMP_Text Win_Text;

    [Header("FreeSpins Popup")]
    [SerializeField]
    private GameObject FreeSpinPopup_Object;
    [SerializeField]
    private TMP_Text Free_Text;
    [SerializeField]
    private Button FreeSpin_Button;

    [Header("Splash Screen")]
    [SerializeField]
    private GameObject Loading_Object;
    [SerializeField]
    private Image Loading_Image;
    [SerializeField]
    private TMP_Text Loading_Text;
    [SerializeField]
    private TMP_Text LoadPercent_Text;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("Disconnection Popup")]
    [SerializeField]
    private Button CloseDisconnect_Button;
    [SerializeField]
    private GameObject DisconnectPopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField]
    private Button CloseAD_Button;
    [SerializeField]
    private GameObject ADPopup_Object;

    [Header("Reconnection Popup")]
    [SerializeField]
    private TMP_Text reconnect_Text;
    [SerializeField]
    private GameObject ReconnectPopup_Object;

    [Header("LowBalance Popup")]
    [SerializeField]
    private Button LBExit_Button;
    [SerializeField]
    private GameObject LBPopup_Object;

    [Header("Turbo Speed Buttons")]
    [SerializeField]
    private Button m_Turtle_Speed_Button;
    [SerializeField]
    private Button m_Rabbit_Speed_Button;
    [SerializeField]
    private Button m_Cheetah_Speed_Button;

    [Header("Value Paytable")]
    [SerializeField]
    private TMP_Text m_Moon_Value;
    [SerializeField]
    private TMP_Text m_Grand_Value;
    [SerializeField]
    private TMP_Text m_Major_Value;
    [SerializeField]
    private TMP_Text m_Minor_Value;
    [SerializeField]
    private TMP_Text m_Mini_Value;

    [Header("Quit Popup")]
    [SerializeField]
    private GameObject QuitPopup_Object;
    [SerializeField]
    private Button YesQuit_Button;
    [SerializeField]
    private Button NoQuit_Button;
    [SerializeField]
    private Button CrossQuit_Button;

    [SerializeField]
    private AudioController audioController;
    [SerializeField]
    private Button m_AwakeGameButton;

    [SerializeField]
    private Button GameExit_Button;

    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private SocketIOManager socketManager;

    [SerializeField]
    internal List<Button> m_BetButtons;

    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    private int FreeSpins;

    private int bet_data_counter = 0;
    private int bet_selected = 0;


    //private void Awake()
    //{
    //    //if (Loading_Object) Loading_Object.SetActive(true);
    //    //StartCoroutine(LoadingRoutine());
    //    SimulateClickByDefault();
    //}

    private IEnumerator LoadingRoutine()
    {
        StartCoroutine(LoadingTextAnimate());
        float imageFill = 0f;
        DOTween.To(() => imageFill, (val) => imageFill = val, 0.7f, 2f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitUntil(() => socketManager.isLoaded);
        DOTween.To(() => imageFill, (val) => imageFill = val, 1, 1f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(1f);
        if (Loading_Object) Loading_Object.SetActive(false);
        StopCoroutine(LoadingTextAnimate());
    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (Loading_Text) Loading_Text.text = "Loading.";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading..";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading...";
            yield return new WaitForSeconds(1f);
        }
    }

    private void Start()
    {

        if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

        if (Exit_Button) Exit_Button.onClick.RemoveAllListeners();
        if (Exit_Button) Exit_Button.onClick.AddListener(CloseMenu);

        //if (About_Button) About_Button.onClick.RemoveAllListeners();
        //if (About_Button) About_Button.onClick.AddListener(delegate { OpenPopup(AboutPopup_Object); });

        if (Info_Button) Info_Button.onClick.RemoveAllListeners();
        if (Info_Button) Info_Button.onClick.AddListener(delegate { OpenPopup(Info_Object); });

        if (Info_Exit) Info_Exit.onClick.RemoveAllListeners();
        if (Info_Exit) Info_Exit.onClick.AddListener(delegate { ClosePopup(Info_Object); });

        if (AboutExit_Button) AboutExit_Button.onClick.RemoveAllListeners();
        if (AboutExit_Button) AboutExit_Button.onClick.AddListener(delegate { ClosePopup(AboutPopup_Object); });

        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { 
            OpenPopup(QuitPopup_Object);
            Debug.Log("Quit event: pressed Big_X button");
            
            });

        if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
        if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { if (!isExit) { 
            ClosePopup(QuitPopup_Object); 
            Debug.Log("quit event: pressed NO Button ");
            } });

        if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
        if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate { if (!isExit) { 
            ClosePopup(QuitPopup_Object); 
            Debug.Log("quit event: pressed Small_X Button ");
            
            } });

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
        if (YesQuit_Button) YesQuit_Button.onClick.AddListener(delegate{
            CallOnExitFunction();
            Debug.Log("quit event: pressed YES Button ");
            
            });

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate{ StartFreeSpins(FreeSpins); });

        if (QuitSplash_button) QuitSplash_button.onClick.RemoveAllListeners();
        if (QuitSplash_button) QuitSplash_button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        if (audioController) audioController.ToggleMute(false);

        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

        if (m_Turtle_Speed_Button) m_Turtle_Speed_Button.onClick.RemoveAllListeners();
        if (m_Turtle_Speed_Button) m_Turtle_Speed_Button.onClick.AddListener(() =>
        {
            m_Turtle_Speed_Button.gameObject.SetActive(false);
            m_Rabbit_Speed_Button.gameObject.SetActive(true);
            m_Cheetah_Speed_Button.gameObject.SetActive(false);
            slotManager.m_Speed = 0.4f;
            slotManager.m_Is_Turtle = false;
            slotManager.m_Is_Rabbit = true;
            slotManager.m_Is_Cheetah = false;
        });

        if (m_Rabbit_Speed_Button) m_Rabbit_Speed_Button.onClick.RemoveAllListeners();
        if (m_Rabbit_Speed_Button) m_Rabbit_Speed_Button.onClick.AddListener(() =>
        {
            m_Turtle_Speed_Button.gameObject.SetActive(false);
            m_Rabbit_Speed_Button.gameObject.SetActive(false);
            m_Cheetah_Speed_Button.gameObject.SetActive(true);
            slotManager.m_Speed = 0.2f;
            slotManager.m_Is_Turtle = false;
            slotManager.m_Is_Rabbit = false;
            slotManager.m_Is_Cheetah = true;
        });

        if (m_Cheetah_Speed_Button) m_Cheetah_Speed_Button.onClick.RemoveAllListeners();
        if (m_Cheetah_Speed_Button) m_Cheetah_Speed_Button.onClick.AddListener(() =>
        {
            m_Turtle_Speed_Button.gameObject.SetActive(true);
            m_Rabbit_Speed_Button.gameObject.SetActive(false);
            m_Cheetah_Speed_Button.gameObject.SetActive(false);
            slotManager.m_Speed = 0.6f;
            slotManager.m_Is_Turtle = true;
            slotManager.m_Is_Rabbit = false;
            slotManager.m_Is_Cheetah = false;
        });

        //AssignBetButtons(m_DummyBetValues);

    }

    private void SimulateClickByDefault()
    {
        Debug.Log("Awaken The Game...");
        m_AwakeGameButton.onClick.AddListener(() => { Debug.Log("Called The Game..."); });
        m_AwakeGameButton.onClick.Invoke();
    }

    internal void UpdateExternalPaytableValue()
    {
        m_Moon_Value.text = (socketManager.initialData.Bets[slotManager.BetCounter] * socketManager.initialData.specialBonusSymbolMulipliers[4].value).ToString() + " FUN";
        m_Grand_Value.text = (socketManager.initialData.Bets[slotManager.BetCounter] * socketManager.initialData.specialBonusSymbolMulipliers[3].value).ToString() + " FUN";
        m_Major_Value.text = (socketManager.initialData.Bets[slotManager.BetCounter] * socketManager.initialData.specialBonusSymbolMulipliers[2].value).ToString() + " FUN";
        m_Minor_Value.text = (socketManager.initialData.Bets[slotManager.BetCounter] * socketManager.initialData.specialBonusSymbolMulipliers[1].value).ToString() + " FUN";
        m_Mini_Value.text = (socketManager.initialData.Bets[slotManager.BetCounter] * socketManager.initialData.specialBonusSymbolMulipliers[0].value).ToString() + " FUN";
    }

    #region [[===BET BUTTONS HANDLING===]]

    internal void ChangeBets(int transact, bool incDec)
    {
        //HACK: The below code may be work accurately with out using bet_data_counter
        int temp_counter = slotManager.BetCounter;
        for (int i = 0; i < m_BetButtons.Count; i++)
        {
            Button tempButton = m_BetButtons[i];
            if (incDec)
            {
                if (slotManager.BetCounter < socketManager.initialData.Bets.Count - 1)
                {
                    tempButton.transform.GetChild(0).GetComponent<TMP_Text>().text = socketManager.initialData.Bets[transact != 0 ? temp_counter - ((m_BetButtons.Count - 1) - i) : temp_counter].ToString();
                    if (transact == 0)
                    {
                        temp_counter++;
                    }
                }
            }
            else
            {
                if (slotManager.BetCounter >= 0)
                {
                    tempButton.transform.GetChild(0).GetComponent<TMP_Text>().text = socketManager.initialData.Bets[transact != 0 ? temp_counter : temp_counter - ((m_BetButtons.Count - 1) - i)].ToString();
                    if (transact != 0)
                    {
                        temp_counter++;
                    }
                }
            }
        }
    }

    //internal void PrevBets(int transact)
    //{
    //    //HACK: The below code may be work accurately with out using bet_data_counter
    //    int temp_counter = slotManager.BetCounter;
    //    for (int i = 0; i < m_BetButtons.Count; i++)
    //    {
    //        if (slotManager.BetCounter >= 0)
    //        {
    //            Button tempButton = m_BetButtons[i];
    //            tempButton.transform.GetChild(0).GetComponent<TMP_Text>().text = socketManager.initialData.Bets[transact != 0 ? temp_counter : temp_counter - ((m_BetButtons.Count - 1) - i)].ToString();
    //            if (transact != 0)
    //            {
    //                temp_counter++;
    //            }
    //        }
    //    }
    //}

    internal void SelectBetButton(bool incdec)
    {
        if (incdec)
        {
            if(bet_selected < m_BetButtons.Count - 1)
            {
                bet_selected++;
                bet_data_counter++;
                ChangeBetToggle(bet_selected);
            }
            else
            {
                //NextBets(1);
                ChangeBets(1, incdec);
            }
        }
        else
        {
            if(bet_selected > 0)
            {
                bet_selected--;
                bet_data_counter--;
                ChangeBetToggle(bet_selected);
            }
            else
            {
                //PrevBets(1);
                ChangeBets(1, incdec);
            }
        }
        UpdateExternalPaytableValue();
        //Debug.Log(string.Concat("<color=red><b>", "Bet Counter: " + slotManager.BetCounter + " Bet Data Counter: " + bet_data_counter, "</b></color>"));
    }

    private void ChangeBetToggle(int index)
    {
        for(int i = 0; i < m_BetButtons.Count; i++)
        {
            m_BetButtons[i].interactable = true;
        }

        m_BetButtons[index].interactable = false;
    }

    internal void StartBetCounter()
    {
        bet_selected = 0;
        bet_data_counter = 0;
        //NextBets(0);
        ChangeBets(0, true);
        ChangeBetToggle(bet_selected);
        UpdateExternalPaytableValue();
    }

    internal void LastBetCounter()
    {
        bet_selected = 3;
        bet_data_counter = socketManager.initialData.Bets.Count - 1;
        //PrevBets(0);
        ChangeBets(0, false);
        ChangeBetToggle(bet_selected);
        UpdateExternalPaytableValue();
    }

    private int GetBetCounter(Button m_Click_Button)
    {
        for (int _ = 0; _ < m_BetButtons.Count; _++)
        {
            if (m_BetButtons[_] == m_Click_Button)
            {
                return _;
            }
        }
        return 0;
    }
    #endregion

    internal void LowBalPopup()
    {
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup(bool isReconnection)
    {
        //if(isReconnection)
        //{
        //    OpenPopup(ReconnectPopup_Object);
        //}
        //else
        //{
        //    ClosePopup(ReconnectPopup_Object);
        //}

        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
    }

    internal void PopulateWin(int value, double amount)
    {
        switch(value)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;
            case 4:
                if (Win_Image) Win_Image.sprite = Jackpot_Sprite;
                break;
        }

        StartPopupAnim(amount);
    }

    private void StartFreeSpins(int spins)
    {
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
        slotManager.FreeSpin(spins);
    }

    internal void FreeSpinProcess(int spins)
    {
        FreeSpins = spins;
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(true);
        if (Free_Text) Free_Text.text = spins.ToString() + " Free spins awarded.";
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void StartPopupAnim(double amount)
    {
        int initAmount = 0;
        if (WinPopup_Object) WinPopup_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, (int)amount, 5f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString();
        });

        DOVirtual.DelayedCall(6f, () =>
        {
            ClosePopup(WinPopup_Object);
            slotManager.CheckPopups = false;
        });
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object); 
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        if (Support_Button) Support_Button.onClick.RemoveAllListeners();
        if (Support_Button) Support_Button.onClick.AddListener(delegate { UrlButtons(SupportUrl); });

        if (Terms_Button) Terms_Button.onClick.RemoveAllListeners();
        if (Terms_Button) Terms_Button.onClick.AddListener(delegate { UrlButtons(TermsUrl); });

        if (Privacy_Button) Privacy_Button.onClick.RemoveAllListeners();
        if (Privacy_Button) Privacy_Button.onClick.AddListener(delegate { UrlButtons(PrivacyUrl); });

        StartCoroutine(DownloadImage(AbtImgUrl));
        PopulateSymbolsPayout(symbolsText);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        //for (int i = 0; i < SymbolsText.Length; i++)
        //{
        //    string text = null;
        //    if (paylines.symbols[i].Multiplier[0][0] != 0)
        //    {
        //        text += "5x - " + paylines.symbols[i].Multiplier[0][0];
        //    }
        //    if (paylines.symbols[i].Multiplier[1][0] != 0)
        //    {
        //        text += "\n4x - " + paylines.symbols[i].Multiplier[1][0];
        //    }
        //    if (paylines.symbols[i].Multiplier[2][0] != 0)
        //    {
        //        text += "\n3x - " + paylines.symbols[i].Multiplier[2][0];
        //    }
        //    if (SymbolsText[i]) SymbolsText[i].text = text;
        //}

        //for (int i = 0; i < paylines.symbols.Count; i++)
        //{
        //    if (paylines.symbols[i].Name.ToUpper() == "FREESPIN")
        //    {
        //        if (FreeSpin_Text) FreeSpin_Text.text = paylines.symbols[i].description.ToString();
        //    }
        //    if (paylines.symbols[i].Name.ToUpper() == "SCATTER")
        //    {
        //        if (Scatter_Text) Scatter_Text.text = paylines.symbols[i].description.ToString();
        //    }
        //    if (paylines.symbols[i].Name.ToUpper() == "JACKPOT")
        //    {
        //        if (Jackpot_Text) Jackpot_Text.text = paylines.symbols[i].description.ToString();
        //    }
        //    if (paylines.symbols[i].Name.ToUpper() == "BONUS")
        //    {
        //        if (Bonus_Text) Bonus_Text.text = paylines.symbols[i].description.ToString();
        //    }
        //    if (paylines.symbols[i].Name.ToUpper() == "WILD")
        //    {
        //        if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
        //    }
        //}
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
    }

    private void OpenMenu()
    {
        audioController.PlayButtonAudio();
        if (Menu_Object) Menu_Object.SetActive(false);
        if (Exit_Object) Exit_Object.SetActive(true);
        if (Info_RT.gameObject) Info_RT.gameObject.SetActive(true);
        if (Paytable_Object) Paytable_Object.SetActive(true);
        if (Settings_Object) Settings_Object.SetActive(true);

        DOTween.To(() => Info_RT.anchoredPosition, (val) => Info_RT.anchoredPosition = val, new Vector2(Info_RT.anchoredPosition.x, Info_RT.anchoredPosition.y + 375), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Info_RT);
        });

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y + 125), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y + 250), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });
    }

    private void CloseMenu()
    {

        if (audioController) audioController.PlayButtonAudio();

        DOTween.To(() => Info_RT.anchoredPosition, (val) => Info_RT.anchoredPosition = val, new Vector2(Info_RT.anchoredPosition.x, Info_RT.anchoredPosition.y - 375), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Info_RT);
        });

        DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y - 125), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
        });

        DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y - 250), 0.1f).OnUpdate(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
        });

        DOVirtual.DelayedCall(0.1f, () =>
         {
             if (Menu_Object) Menu_Object.SetActive(true);
             if (Exit_Object) Exit_Object.SetActive(false);
             if (Info_RT.gameObject) Info_RT.gameObject.SetActive(false);
             if (Paytable_Object) Paytable_Object.SetActive(false);
             if (Settings_Object) Settings_Object.SetActive(false);
         });
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf) 
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void ToggleMusic()
    {
        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            audioController.ToggleMute(true, "bg");
        }
    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            if (audioController) audioController.ToggleMute(false,"button");
            if (audioController) audioController.ToggleMute(false,"wl");
        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            if(audioController) audioController.ToggleMute(true,"button");
            if (audioController) audioController.ToggleMute(true,"wl");
        }
    }

    private IEnumerator DownloadImage(string url)
    {
        // Create a UnityWebRequest object to download the image
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // Wait for the download to complete
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Apply the sprite to the target image
            AboutLogo_Image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
    }
}
