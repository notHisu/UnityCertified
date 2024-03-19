using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class PlayerShipBuild : MonoBehaviour, IUnityAdsListener, IUnityAdsInitializationListener
{
    GameObject target;
    GameObject tmpSelection;
    GameObject textBoxPanel;

    [SerializeField]
    GameObject[] visualWeapons;
    [SerializeField]
    SOActorModel defaultPlayerShip;
    GameObject playerShip;
    GameObject buyButton;
    GameObject bankObj;
    int bank = 600;
    bool purchaseMade = false;

    [SerializeField] string androidGameId;
    [SerializeField] string iOSGameId;
    [SerializeField] bool testMode = true;
    string adId = null;

    void Awake()
    {
        CheckPlatform();
    }

    void CheckPlatform()
    {
        string gameId = null;
#if UNITY_IOS
    {
        gameId = iOSGameId;
        adId = "Rewarded_iOS";
    }
#elif UNITY_ANDROID
        {
            gameId = androidGameId;
            adId = "Rewarded_Android";
        }
#endif
        Advertisement.Initialize(gameId, testMode, false, this);
    }

    void Start()
    {
        StartCoroutine(WaitForAd());
        textBoxPanel = GameObject.Find("textBoxPanel");
        TurnOffSelectionHighlights();

        purchaseMade = false;
        bankObj = GameObject.Find("bank");
        bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
        buyButton = GameObject.Find("BUY?").gameObject;
        buyButton.SetActive(false);
        TurnOffPlayerShipVisuals();
        PreparePlayerShipForUpgrade();
    }

    IEnumerator WaitForAd()
    {
        while (!Advertisement.isInitialized)
        {
            yield return null;
        }
        LoadAd();
    }

    void LoadAd()
    {
        Advertisement.AddListener(this);
        Advertisement.Load(adId);
    }

    void TurnOffSelectionHighlights()
    {
        GameObject[] selections = GameObject.FindGameObjectsWithTag("Selection");
        for (int i = 0; i < selections.Length; i++)
        {
            if (selections[i].GetComponentInParent<ShopPiece>())
            {
                if (selections[i].GetComponentInParent<ShopPiece>().ShopSelection.iconName == "sold Out")
                {
                    selections[i].SetActive(false);
                }
            }
        }
    }

    //REMOVED 05
    //void Update()
    //{
    //    AttemptSelection();
    //}


    //REMOVED 01
    //GameObject ReturnClickedObject(out RaycastHit hit)
    //{
    //    GameObject target = null;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(ray.origin, ray.direction * 100,out hit))
    //    {
    //        target = hit.collider.gameObject;
    //    }
    //    return target;

    //}

    public void AttemptSelection(GameObject buttonName)
    {

        //REMOVED 03
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            RaycastHit hitInfo;
        //            target = ReturnClickedObject(out hitInfo);
        //            if (target != null)
        //            {
        //                if (target.transform.Find("itemText"))
        //                {
        //TurnOffSelectionHighlights();
        //Select();

        //ENTER if (buttonName) around here code block here
        if (buttonName)
        {
            TurnOffSelectionHighlights();
            tmpSelection = buttonName;
            tmpSelection.transform.GetChild(1).gameObject.SetActive(true);

            UpdateDescriptionBox();

            //NOT ALREADY SOLD
            if (buttonName.GetComponentInChildren<Text>().text != "SOLD")
            {
                //can afford
                Affordable();
                //can not afford
                LackOfCredits();
            }
            else if (buttonName.GetComponentInChildren<Text>().text == "SOLD")
            {
                SoldOut();
            }
        }
    }
        //REMOVED 04
        //
        //else if (target.name == "BUY ?")
        //{
        //    BuyItem();
        //}
        //else if (target.name == "START")
        //{
        //    StartGame();
        //}
        //else if (target.name == "WATCH AD")
        //{
        //    WatchAdvert();
        //}
        //}
        //}
        //}

        public void WatchAdvert()
        {
            Advertisement.Show(adId);
        }

        public void StartGame()
        {
            if (purchaseMade)
            {
                playerShip.name = "UpgradedShip";

                if (playerShip.transform.Find("energy +1(Clone)"))
                {
                    playerShip.GetComponent<Player>().Health = 2;
                }

                DontDestroyOnLoad(playerShip);
            }
            GameManager.Instance.GetComponent<ScenesManager>().BeginGame(GameManager.gameLevelScene);
        }

        public void BuyItem()
        {
            Debug.Log("PURCHASED");
            purchaseMade = true;
            buyButton.SetActive(false);
            textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = "";
            textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = "";
            //tmpSelection.SetActive(false);

            for (int i = 0; i < visualWeapons.Length; i++)
            {
                if (visualWeapons[i].name == tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName)
                {
                    visualWeapons[i].SetActive(true);
                }
            }

            UpgradeToShip(tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName);

            bank = bank - System.Int16.Parse(tmpSelection.GetComponent<ShopPiece>().ShopSelection.cost);
            bankObj.transform.Find("bankText").GetComponent<TextMesh>().text = bank.ToString();
            tmpSelection.transform.Find("itemText").GetComponentInChildren<Text>().text = "SOLD";
        }

        void UpgradeToShip(string upgrade)
        {
            GameObject shipItem = GameObject.Instantiate(Resources.Load(upgrade)) as GameObject;
            shipItem.transform.SetParent(playerShip.transform);
            shipItem.transform.localPosition = Vector3.zero;
        }

        void Affordable()
        {
            if (bank >= System.Int32.Parse(tmpSelection.GetComponentInChildren<Text>().text))
            {
                Debug.Log("CAN BUY");
                buyButton.SetActive(true);
            }
        }

        void SoldOut()
        {
            Debug.Log("SOLD OUT");
        }

        void TurnOffPlayerShipVisuals()
        {
            for (int i = 0; i < visualWeapons.Length; i++)
            {
                visualWeapons[i].gameObject.SetActive(false);
            }
        }

        void PreparePlayerShipForUpgrade()
        {
            playerShip = GameObject.Instantiate(defaultPlayerShip.actor);
            playerShip.GetComponent<Player>().enabled = false;
            playerShip.transform.position = new Vector3(0, 10000, 0);
            playerShip.GetComponent<IActorTemplate>().ActorStats(defaultPlayerShip);
        }

        void LackOfCredits()
        {
            if (bank < System.Int32.Parse(tmpSelection.GetComponentInChildren<Text>().text))
            {
                Debug.Log("CAN'T BUY");
            }
        }

        //REMOVED 02
        //void Select()
        //{
        //    tmpSelection = target.transform.Find("SelectionQuad").gameObject;
        //    tmpSelection.SetActive(true);

        //}

        void UpdateDescriptionBox()
        {
            textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName;
            textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponent<ShopPiece>().ShopSelection.description;
        }

		public void OnUnityAdsReady(string placementId)
        {

        }

        public void OnUnityAdsDidError(string message)
        {

        }

        public void OnUnityAdsDidStart(string placementId)
        {

        }

        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            if (showResult == ShowResult.Finished)
            {
                // REWARD PLAYER
                Debug.Log("Unity Ads Rewarded Ad Completed");
                bank += 300;
                bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
            }

            else if (showResult == ShowResult.Skipped)
            {
                // DO NOT REWARD PLAYER
            }

            else if (showResult == ShowResult.Failed)
            {
                Debug.LogWarning("The ad did not finish due to an error.");
            }

            Advertisement.Load(placementId);
            TurnOffSelectionHighlights();
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
    }