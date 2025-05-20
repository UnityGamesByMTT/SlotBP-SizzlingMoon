using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField]
  private SlotBehaviour slotManager;

  [SerializeField]
  private UIManager uiManager;

  internal GameData initialData = null;
  internal UiData initUIData = null;
  internal GameData resultData = null;
  internal PlayerData playerdata = null;
  [SerializeField]
  internal List<string> bonusdata = null;
  internal bool isResultdone = false;

  private SocketManager manager;
  [SerializeField] internal JSFunctCalls JSManager;
  protected string nameSpace = "playground";
  private Socket gameSocket;

  protected string SocketURI = null;
  protected string TestSocketURI = "http://localhost:5001";
  //protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  //protected string TestSocketURI = "https://gl9r1h24-5001.inc1.devtunnels.ms/";

  [SerializeField]
  private string testToken;

  // protected string gameID = "";
  protected string gameID = "SL-SM";

  internal bool isLoaded = false;

  internal bool SetInit = false;

  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

  private void Awake()
  {
    //Debug.unityLogger.logEnabled = false;
    isLoaded = false;
    SetInit = false;

  }

  private void Start()
  {
    //OpenWebsocket();
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);

    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    SocketURI = data.socketURL;
    myAuth = data.cookie;
    nameSpace = data.nameSpace;
  }

  string myAuth = null;

  private void OpenSocket()
  {
    // Create and setup SocketOptions
    SocketOptions options = new SocketOptions();
    options.ReconnectionAttempts = maxReconnectionAttempts;
    options.ReconnectionDelay = reconnectionDelay;
    options.Reconnection = true;
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

    // JSManager.SendCustomMessage("authToken");
    // StartCoroutine(WaitForAuthToken(options));
#if UNITY_WEBGL && !UNITY_EDITOR
    string url = Application.absoluteURL;
    Debug.Log("Unity URL : " + url);
    ExtractUrlAndToken(url);

    Func<SocketManager, Socket, object> webAuthFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,
      };
    };
    options.Auth = webAuthFunction;
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,
      };
    };
    options.Auth = authFunction;
#endif
    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  public void ExtractUrlAndToken(string fullUrl)
  {
    Uri uri = new Uri(fullUrl);
    string query = uri.Query; // Gets the query part, e.g., "?url=http://localhost:5000&token=e5ffa84216be4972a85fff1d266d36d0"

    Dictionary<string, string> queryParams = new Dictionary<string, string>();
    string[] pairs = query.TrimStart('?').Split('&');

    foreach (string pair in pairs)
    {
      string[] kv = pair.Split('=');
      if (kv.Length == 2)
      {
        queryParams[kv[0]] = Uri.UnescapeDataString(kv[1]);
      }
    }

    if (queryParams.TryGetValue("url", out string extractedUrl) &&
        queryParams.TryGetValue("token", out string token))
    {
      Debug.Log("Extracted URL: " + extractedUrl);
      Debug.Log("Extracted Token: " + token);
      testToken = token;
      SocketURI = extractedUrl;
    }
    else
    {
      Debug.LogError("URL or token not found in query parameters.");
    }
  }

  void OnResult(string data)
  {
    print(data);
    ParseResultData(data);
  }

  void ParseResultData(string json)
  {
    // ResultData ConvertedData = JsonConvert.DeserializeObject<ResultData>(json);

    // resultData = new GameData();
    // resultData.ResultReel = ConvertedData.matrix;
    Debug.Log("ParseResultData: " + json);
    isResultdone = true;
  }

  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    Debug.Log("My Auth is not null");
    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth,
        gameId = gameID
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
    if (string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace))
    {
      gameSocket = this.manager.Socket;
    }
    else
    {
      Debug.Log("Namespace used :" + nameSpace);
      gameSocket = this.manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
    gameSocket.On<string>(SocketIOEventTypes.Error, OnError);
    gameSocket.On<string>("game:init", OnListenEvent);
    gameSocket.On<string>("spin:result", OnResult);
    gameSocket.On<bool>("socketState", OnSocketState);
    gameSocket.On<string>("internalError", OnSocketError);
    gameSocket.On<string>("alert", OnSocketAlert);
    gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice);
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("Connected!");
    SendPing();
  }

  private void OnDisconnected(string response)
  {
    Debug.Log("Disconnected from the server");
    StopAllCoroutines();
    uiManager.DisconnectionPopup(false);
  }

  private void OnError(string response)
  {
    Debug.LogError("Error: " + response);
  }

  private void OnListenEvent(string data)
  {
    print(data);
    ParseResponse(data);
  }

  private void OnSocketState(bool state)
  {
    if (state)
    {
      Debug.Log("my state is " + state);
    }
  }
  internal void closeSocketCallReactnative()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
  }
  private void OnSocketError(string data)
  {
    Debug.Log("Received error with data: " + data);
  }
  private void OnSocketAlert(string data)
  {
    Debug.Log("Received alert with data: " + data);
  }

  private void OnSocketOtherDevice(string data)
  {
    Debug.Log("Received Device Error with data: " + data);
    uiManager.ADfunction();
  }

  private void SendPing()
  {
    InvokeRepeating("AliveRequest", 0f, 3f);
  }

  private void AliveRequest()
  {
    SendDataWithNamespace("YES I AM ALIVE");
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen)
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }

  internal void CloseSocket()
  {
    SendDataWithNamespace("EXIT");
  }

  private void ParseResponse(string jsonObject)
  {
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);
    string id = myData.id;
    switch (id)
    {
      case "initData":
        {
          initialData = myData.gameData;
          initUIData = myData.uiData;
          playerdata = myData.player;
          bonusdata = myData.BonusData;
          // LineData = myData.gameData.lines;
          if (!SetInit)
          {
            Debug.Log(String.Concat("<color=cyan><b>", jsonObject, "</b></color>"));
            // List<string> InitialReels = ConvertListOfListsToStrings(initialData.Reel);
            // InitialReels = RemoveQuotes(InitialReels);
            // PopulateSlotSocket(InitialReels);
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          Debug.Log(String.Concat("<color=green><b>", jsonObject, "</b></color>"));
          // myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
          // myData.message.GameData.FinalValuesToEmit = ConvertToNestedList(myData.message.GameData.symbolsToEmit);
          // resultData = myData.message.GameData;
          // playerdata = myData.message.PlayerData;
          isResultdone = true;
          break;
        }
      case "ExitUser":
        {
          gameSocket.Disconnect();
          if (this.manager != null)
          {
            Debug.Log(String.Concat("<color=red><b>", "Dispose My Socket", "</b></color>"));
            this.manager.Close();
          }
#if UNITY_WEBGL && !UNITY_EDITOR
          JSManager.SendCustomMessage("onExit");
#endif
          break;
        }
    }
  }

  private void RefreshUI()
  {
    uiManager.InitialiseUIData(initUIData.AbtLogo.link, initUIData.AbtLogo.logoSprite, initUIData.ToULink, initUIData.PopLink, initUIData.paylines);
  }

  private void PopulateSlotSocket(List<string> LineIds)
  {
    slotManager.shuffleInitialMatrix();
    slotManager.SetInitialUI();
    isLoaded = true;
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif
  }

  internal void AccumulateResult(double currBet)
  {
    isResultdone = false;
    MessageData message = new MessageData();
    message.data = new BetData();
    message.data.currentBet = currBet;
    message.data.spins = 1;
    message.data.currentLines = 1;
    message.id = "SPIN";
    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("message", json);
  }

  private List<string> RemoveQuotes(List<string> stringList)
  {
    for (int i = 0; i < stringList.Count; i++)
    {
      stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
    }
    return stringList;
  }

  private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
  {
    List<string> resultList = new List<string>();

    foreach (List<int> innerList in listOfLists)
    {
      // Convert each integer in the inner list to string
      List<string> stringList = new List<string>();
      foreach (int number in innerList)
      {
        stringList.Add(number.ToString());
      }

      // Join the string representation of integers with ","
      string joinedString = string.Join(",", stringList.ToArray()).Trim();
      resultList.Add(joinedString);
    }

    return resultList;
  }

  private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
  {
    List<string> outputList = new List<string>();

    foreach (List<string> row in inputList)
    {
      string concatenatedString = string.Join(",", row);
      outputList.Add(concatenatedString);
    }

    return outputList;
  }

  public static List<List<string>> ConvertToNestedList(List<string> symbolsToEmit)
  {
    // Create a dictionary to group coordinates by their row (second value).
    var groupedCoordinates = new Dictionary<int, List<string>>();

    foreach (var coordinate in symbolsToEmit)
    {
      var parts = coordinate.Split(',');
      if (parts.Length != 2)
        throw new FormatException($"Invalid coordinate format: {coordinate}");

      int x = int.Parse(parts[0]);
      int y = int.Parse(parts[1]);

      // Group coordinates by the row (y value)
      if (!groupedCoordinates.ContainsKey(y))
        groupedCoordinates[y] = new List<string>();

      groupedCoordinates[y].Add(coordinate);
    }

    // Convert the grouped dictionary into a List<List<string>>
    var result = groupedCoordinates
        .OrderBy(pair => pair.Key) // Ensure rows are in order
        .Select(pair => pair.Value) // Extract lists of coordinates
        .ToList();

    return result;
  }

  private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
  {
    // Flattened list
    List<string> flattenedList = new List<string>();
    foreach (List<string> sublist in originalList)
    {
      flattenedList.AddRange(sublist);
    }

    // Remove recurring elements
    HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

    // Transformed list
    List<string> transformedList = new List<string>();
    foreach (string element in uniqueElements)
    {
      transformedList.Add(element.Replace(",", ""));
    }
    return transformedList;
  }
}

[Serializable]
public class BetData
{
  public double currentBet;
  public double currentLines;
  public double spins;
}

[Serializable]
public class AuthData
{
  public string GameID;
  //public double TotalLines;
}

[Serializable]
public class MessageData
{
  public BetData data;
  public string id;
}

[Serializable]
public class ExitData
{
  public string id;
}

[Serializable]
public class InitData
{
  public AuthData Data;
  public string id;
}

[Serializable]
public class AbtLogo
{
  public string logoSprite { get; set; }
  public string link { get; set; }
}

[Serializable]
public class GameData
{
  public List<List<string>> Reel { get; set; }
  public List<List<int>> Lines { get; set; }
  public List<List<int>> BonusResultReel { get; set; }
  public List<SpecialBonusSymbolMuliplier> specialBonusSymbolMulipliers { get; set; }
  public List<double> Bets { get; set; }
  public bool canSwitchLines { get; set; }
  public List<int> LinesCount { get; set; }
  public List<int> autoSpin { get; set; }
  public List<List<string>> ResultReel { get; set; }
  public List<int> linesToEmit { get; set; }
  public List<string> symbolsToEmit { get; set; }
  public double WinAmout { get; set; }
  public FreeSpins freeSpins { get; set; }
  public bool isFreeSpin { get; set; }
  public int freeSpinCount { get; set; }
  public bool freeSpinAdded { get; set; }
  public List<FrozenIndex> frozenIndices { get; set; }
  public bool isGrandPrize { get; set; }
  public bool isMoonJackpot { get; set; }
  public List<List<int>> moonMysteryData { get; set; }
  public bool isStickyBonus { get; set; }
  public List<StickyBonusValue> stickyBonusValue { get; set; }
  public List<string> FinalsymbolsToEmit { get; set; }
  public List<List<string>> FinalValuesToEmit { get; set; }
  public List<string> FinalResultReel { get; set; }
  public double jackpot { get; set; }
  public bool isBonus { get; set; }
  public double BonusStopIndex { get; set; }
  public int allWildMultiplier { get; set; }
  public bool isAllWild { get; set; }
}

[Serializable]
public class SpecialBonusSymbolMuliplier
{
  public string name { get; set; }
  public int value { get; set; }
}

[Serializable]
public class FreeSpins
{
  public int count { get; set; }
  public bool isNewAdded { get; set; }
}

[Serializable]
public class Message
{
  public GameData GameData { get; set; }
  public UiData UIData { get; set; }
  public PlayerData PlayerData { get; set; }
  public List<string> BonusData { get; set; }
}

[Serializable]
public class Root
{
  public string id { get; set; }
  public GameData gameData { get; set; }
  public UiData uiData { get; set; }
  public PlayerData player { get; set; }
  public List<string> BonusData { get; set; }
}

[Serializable]
public class UiData
{
  public Paylines paylines { get; set; }
  public List<string> spclSymbolTxt { get; set; }
  public AbtLogo AbtLogo { get; set; }
  public string ToULink { get; set; }
  public string PopLink { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
  public int ID { get; set; }
  public string Name { get; set; }
  [JsonProperty("multiplier")]
  public object MultiplierObject { get; set; }

  // This property will hold the properly deserialized list of lists of integers
  [JsonIgnore]
  public List<List<double>> Multiplier { get; private set; }

  // Custom deserialization method to handle the conversion
  [OnDeserialized]
  internal void OnDeserializedMethod(StreamingContext context)
  {
    // Handle the case where multiplier is an object (empty in JSON)
    if (MultiplierObject is JObject)
    {
      Multiplier = new List<List<double>>();
    }
    else
    {
      // Deserialize normally assuming it's an array of arrays
      Multiplier = JsonConvert.DeserializeObject<List<List<double>>>(MultiplierObject.ToString());
    }
  }
  public object defaultAmount { get; set; }
  public object symbolsCount { get; set; }
  public object increaseValue { get; set; }
  public object description { get; set; }
  public int payout { get; set; }
  public object mixedPayout { get; set; }
  public object defaultPayout { get; set; }
  public int freeSpin { get; set; }
}

[Serializable]
public class FrozenIndex
{
  public List<int> position { get; set; }
  public int prizeValue { get; set; }
  public int symbol { get; set; }
}

[Serializable]
public class StickyBonusValue
{
  public List<int> position { get; set; }
  public int prizeValue { get; set; }
  public int value { get; set; }
  public int symbol { get; set; }
}

[Serializable]
public class MoonMysteryDatum
{
  public List<int> position { get; set; }
  public int prizeValue { get; set; }
  public int symbol { get; set; }
}

[Serializable]
public class PlayerData
{
  public double Balance { get; set; }
  public double haveWon { get; set; }
  public double currentWining { get; set; }
  public double totalbet { get; set; }
}
[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace;
}


