using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

/*public class Garden_ShopSystem : MonoBehaviour
{
    Garden_EditorManager editorManager;
    [SerializeField]
    PlayerData playerData;

    Garden_ItemSO selectedItem;

    [SerializeField]
    List<Garden_ItemSO> itemList; // アイテムのScriptableObjectをInspectorから設定

    [SerializeField]
    GameObject itemButtonPrefab; // アイテムボタンのプレハブ
    [SerializeField]
    GameObject[] needItemPanel;
    [SerializeField]
    TextMeshProUGUI[] currentItemCountText,needItemCountText,GetPanel_currentItemCountText,GetPanel_needItemCountText;
    [SerializeField]
    Slider[] currentSlider;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    GameObject GetPanel,GetButton; // アイテムパネルのUIオブジェクト
    [SerializeField]
    Transform itemButtonParent; // アイテムボタンを配置する親オブジェクト
    int itemBuyCount; // 購入するアイテムの個数
    [SerializeField]
    GameObject itemPlusButtonGroup, itemMinusButtonGroup; // 購入数を増減するボタン
    void Start()
    {
        editorManager = GetComponent<Garden_EditorManager>();
    }

    public void BuyButtonClicked()
    {
        //購入するか　また購入個数はどうするかを確認する
        GetPanel.SetActive(true);
        //一つ以上買える場合はスライダーを出す
        itemBuyCount = 1;
        GetPanel_needItemCountText[0].text = selectedItem.needNum.ToString();
        GetPanel_currentItemCountText[0].text = "/" + playerData.itemCount[Item.ItemType.MainStar];
        GetPanel_needItemCountText[1].text = selectedItem.needNum.ToString();
        GetPanel_currentItemCountText[1].text = "/" + playerData.itemCount[selectedItem.needItemType];
        itemMinusButtonGroup.SetActive(false);

    }

    public void BuyCount(bool isPlus)
    {

        itemBuyCount = isPlus ? itemBuyCount + 1 : itemBuyCount - 1;
        //テキストの内容も変更
        itemMinusButtonGroup.SetActive(itemBuyCount > 1);
        int buyMax = Mathf.Min(playerData.itemCount[Item.ItemType.MainStar] / selectedItem.needNum, 
                                playerData.itemCount[selectedItem.needItemType] / selectedItem.needNum);
        itemPlusButtonGroup.SetActive(itemBuyCount < buyMax);
    }
    public void MaxSet()
    {
        int m = Mathf.Min(playerData.itemCount[Item.ItemType.MainStar] / selectedItem.needNum, 
                                 playerData.itemCount[selectedItem.needItemType] / selectedItem.needNum);
        for (int i = itemBuyCount; i < itemBuyCount; i++)
        {
            BuyCount(true);
        }
            //購入数を最大に設定

        
    }
    public void MinSet()
    {
       for(int i = itemBuyCount; i > 1; i--)
        {
            BuyCount(false);
        }
        //購入数を最小に設定
    }
    

    public void GetGardenItem()
    {
        // アイテムをプレイヤーデータに追加
        selectedItem.initialTotalPossession++;
        selectedItem.Save();
    }

    public void SetSelectedItem(int num)
    {
        // 選択されたアイテムを設定
        selectedItem = itemList[num];
        currentItemCountText[0].text = "/" + playerData.itemCount[Item.ItemType.MainStar];
        needItemCountText[0].text =  selectedItem.needNum.ToString();
        currentSlider[0].value = (float)playerData.itemCount[Item.ItemType.MainStar] / selectedItem.needNum;

        currentItemCountText[1].text = "/" + playerData.itemCount[selectedItem.needItemType];
        needItemCountText[1].text =  selectedItem.needNum.ToString();
        currentSlider[1].value = (float)playerData.itemCount[selectedItem.needItemType] / selectedItem.needNum;

        Debug.Log("選択されたアイテム: " + selectedItem.name);
        if (playerData.itemCount[Item.ItemType.MainStar] >= selectedItem.needNum || playerData.itemCount[selectedItem.needItemType] >= selectedItem.needNum)
        {
          GetButton.SetActive(true);
        }
        else
        {
            GetButton.SetActive(false);
        }
    }
}
*/
//制作代案
public class Garden_ShopSystem : MonoBehaviour
{
    [SerializeField]
    UISystem uISystem; // UIシステムの参照
    [SerializeField]
    List<Garden_ItemSO> all_ItemList; // アイテムのScriptableObjectをInspectorから設定
    Garden_ItemSO selectedExchangeItem; // 選択された交換先のアイテム
    Garden_ItemSO[] selectedMyItemlist; // 選択された交換元のアイテム
    [SerializeField]
    GameObject itemButtonPrefab; // アイテムボタンのプレハブ
    [SerializeField]
    Transform itemButtonParent; // アイテムボタンを配置する親オブジェクト
    int selectMyItemNum; // 選択されたアイテムの番号
    [SerializeField]
    Image exchangeItemIcon; // 交換先アイテムのアイコンを表示する
    [SerializeField]
    Image [] myItemIcons; // 交換元アイテムのアイコンを表示する

    [SerializeField]
    GameObject exchangePanel , shopPanel, exchangeButton; // アイテム選択パネルのUIオブジェクト

List<GameObject> currentItemList; // プレイヤーが所持しているアイテムのリスト

void Start()
    {
        // 初期化
        selectedMyItemlist = new Garden_ItemSO[myItemIcons.Length];
    }
    public void LoadItem(bool reward)
    {
        if (currentItemList == null)
        {
            currentItemList = new List<GameObject>();
        }
        else
        {
            foreach (GameObject obj in currentItemList)
            {
                Destroy(obj);
            }
            currentItemList.Clear();
        }
        for (int i = 0; i < all_ItemList.Count; i++)
        {
            if (!reward && all_ItemList[i].initialTotalPossession <= 0) continue; // 交換を行う際、自分がアイテムを所持していないものは表示しない

            GameObject itemButton = Instantiate(itemButtonPrefab, itemButtonParent);
            itemButton.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = all_ItemList[i].icon; // アイコンを設定

            //itemButton.GetComponentInChildren<TextMeshProUGUI>().text = all_ItemList[i].name;

            //ボタンからiconとテキストを取得する
            int index = i; // ローカル変数を使用してクロージャーを回避
            if (reward)
            {
                itemButton.GetComponent<Button>().onClick.AddListener(() => SelectedRewardItem(index));
            }
            else
            {
                itemButton.GetComponent<Button>().onClick.AddListener(() => SelectedMyItem(index));
            }
            itemButton.GetComponent<Button>().onClick.AddListener(() => uISystem.Panel_Open(exchangePanel.transform));
            currentItemList.Add(itemButton);
        }

        if (currentItemList.Count == 0)
        {
            // アイテムがない場合の処理
            if (reward)
            {
                uISystem.Show_notification("交換可能なアイテムがありません。");
            }
            else
            {
                uISystem.Show_notification("所持アイテムがありません。");
            }
            shopPanel.SetActive(false); // アイテム選択パネルを非表示
            exchangePanel.SetActive(false); // 交換パネルを非表示
        }
        else
        {
            exchangePanel.SetActive(false); // 交換パネルを非表示
            uISystem.Panel_Open(shopPanel.transform); // アイテム選択パネルを開く
        }
    }
    //プレイヤーが交換するアイテムを選択した時に呼ばれるアイテム
    public void SelectedRewardItem(int num)
    {
        selectedExchangeItem = all_ItemList[num];
        exchangeItemIcon.sprite = selectedExchangeItem.icon; // UIにアイコンを表示
        Debug.Log("選択された交換先アイテム: " + selectedExchangeItem.name);
        //交換のためにプレイヤーが出すアイテムを選択するためのUIを表示
    }

    public void OpenMyItemList(int num)
    {
        selectMyItemNum = num;
        LoadItem(false); // 所持アイテムリストをロード
    }

    public void SelectedMyItem(int num)
    {
        selectedMyItemlist[selectMyItemNum] = all_ItemList[num];
        selectedMyItemlist[selectMyItemNum].initialTotalPossession--; // 選択したアイテムの所持数を減らす
        myItemIcons[selectMyItemNum].sprite = selectedMyItemlist[selectMyItemNum].icon; // UIにアイコンを表示

        myItemIcons[selectMyItemNum].gameObject.SetActive(true); // アイコンを表示
        Debug.Log("選択された交換元アイテム: " + selectedMyItemlist[selectMyItemNum].name);
        int count = 0;
        for (int i = 0; i < selectedMyItemlist.Length; i++)
        {
            if (selectedMyItemlist[i] != null)
            {
                count++; // アイコンを非表示
            }
        }
        if(count == selectedMyItemlist.Length)
        {
            // 交換ボタンを有効化
            uISystem.Panel_Open(exchangeButton.transform);
        }
       
    }

    public void ExchangeItem()
    {
        // 交換処理を実行
        // 交換元のアイテムを削除し、交換先のアイテムを追加する
        for (int i = 0; i < selectedMyItemlist.Length; i++)
        {
            if (selectedMyItemlist[i] != null)
            {
                selectedMyItemlist[i].Save();
                selectedMyItemlist[i] = null; // アイテムを削除
                myItemIcons[i].gameObject.SetActive(false); // アイコンを非表示
            }
        }
        selectedExchangeItem.initialTotalPossession++;
        selectedExchangeItem.Save();
        selectedExchangeItem = null; // 交換先アイテムをリセット
        
        uISystem.NewGetItem_DirectionPlay(UISystem.AnimationPattern.Popup, selectedExchangeItem.icon, selectedExchangeItem.name);
        exchangePanel.SetActive(false); // 交換パネルを閉じる
        shopPanel.SetActive(false); // アイテム選択パネルを閉じる
        exchangeButton.SetActive(false); // 交換ボタンを閉じる
        Debug.Log("アイテム交換が完了しました。");
        exchangeButton.SetActive(false); // 交換ボタンを非表示
    }

}
