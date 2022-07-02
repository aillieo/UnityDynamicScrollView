using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AillieoUtils;
using UnityEngine.UI;
using SObject = System.Object;
using System.Text;

public struct DefaultScrollItemData
{
    public string longString;
    public string name;
}

public class TestScript : MonoBehaviour {
    List<DefaultScrollItemData> testData = new List<DefaultScrollItemData>();

    void updateFunc(int index, RectTransform item)
    {
        DefaultScrollItemData data = this.testData[index];
        item.gameObject.SetActive(true);
        item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", data.name, index);
    }

    void updateFunc_3(int index, RectTransform item)
    {
        DefaultScrollItemData data = this.testData[index];
        item.GetComponent<Text>().text = data.longString;
    }

    Vector2 itemSizeFunc_2(int index)
    {
        DefaultScrollItemData sd = this.testData[index];
        if (sd.name == "XXL")
            return new Vector2(300, 120);
        else if (sd.name == "XL")
            return new Vector2(300, 110);
        else if (sd.name == "L")
            return new Vector2(300, 100);
        else if (sd.name == "M")
            return new Vector2(300, 90);
        else // "S"
            return new Vector2(300, 80);
    }

    Vector2 itemSizeFunc_5(int index)
    {
        Vector2 size2 = this.itemSizeFunc_2(index);
        return new Vector2(size2.y, size2.x);
    }

    int itemCountFunc()
    {
        return this.testData.Count;
    }

    Vector2 itemSizeFunc_3(int index)
    {
        if (this.templateTextItemInstance == null)
        {
            this.templateTextItemInstance = GameObject.Instantiate(this.templateTextItem).GetComponent<RectTransform>();
            this.templateTextItemInstance.gameObject.SetActive(true);
            this.templateTextItemInstance.localScale = Vector3.zero;
        }
        this.templateTextItemInstance.GetComponent<Text>().text = this.testData[index].longString;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.templateTextItemInstance);
        var height = LayoutUtility.GetPreferredHeight(this.templateTextItemInstance);
        return new Vector2(300, height);
    }

    public ScrollView scrollView_1;

    public ScrollView scrollView_2;

    public ScrollView scrollView_3;

    public ScrollView scrollView_4;

    public ScrollView scrollView_5;

    public ScrollView scrollView_6;

    public RectTransform templateTextItem;
    private RectTransform templateTextItemInstance;

    void Start () {
        this.scrollView_1.SetUpdateFunc(this.updateFunc);
        this.scrollView_1.SetItemCountFunc(this.itemCountFunc);

        this.scrollView_2.SetUpdateFunc(this.updateFunc);
        this.scrollView_2.SetItemSizeFunc(this.itemSizeFunc_2);
        this.scrollView_2.SetItemCountFunc(this.itemCountFunc);

        this.scrollView_3.SetUpdateFunc(this.updateFunc_3);
        this.scrollView_3.SetItemSizeFunc(this.itemSizeFunc_3);
        this.scrollView_3.SetItemCountFunc(this.itemCountFunc);
        
        this.scrollView_4.SetUpdateFunc(this.updateFunc);
        this.scrollView_4.SetItemCountFunc(this.itemCountFunc);

        this.scrollView_5.SetUpdateFunc(this.updateFunc);
        this.scrollView_5.SetItemSizeFunc(this.itemSizeFunc_5);
        this.scrollView_5.SetItemCountFunc(this.itemCountFunc);

        this.scrollView_6.SetUpdateFunc(this.updateFunc_3);
        this.scrollView_6.SetItemSizeFunc(this.itemSizeFunc_3);
        this.scrollView_6.SetItemCountFunc(this.itemCountFunc);

        var dataCount = 0;
        do
        {
            this.AddRandomData();
        }
        while (++dataCount < 50);

        this.UpdateAllScrollViews();
    }

    static string GetRandomSizeString()
    {
        var f = UnityEngine.Random.value;
        if (f > 0.8)
        {
            return "XXL";
        }
        else if (f > 0.6)
        {
            return "XL";
        }
        else if (f > 0.4)
        {
            return "L";
        }
        else if (f > 0.2)
        {
            return "M";
        }
        else
        {
            return "S";
        }
    }

    static string GetRandomLongText()
    {
        var rand = UnityEngine.Random.Range(1,100);
        var stringBuilder = new StringBuilder(rand + 2);
        do {
            stringBuilder.Append((char)UnityEngine.Random.Range('A','Z'));
        }
        while (--rand > 0) ;
        stringBuilder.AppendLine();
        return stringBuilder.ToString();
    }

    public void AddRandomData()
    {
        var newData = new DefaultScrollItemData() { name = GetRandomSizeString(), longString = GetRandomLongText()};
        this.testData.Insert(UnityEngine.Random.Range(0,this.testData.Count), newData);
        this.UpdateAllScrollViews();
    }

    public void RemoveRandomData()
    {
        if (this.testData.Count == 0)
        {
            return;
        }
        var index = UnityEngine.Random.Range(0, this.testData.Count);
        this.testData.RemoveAt(index);
        this.UpdateAllScrollViews();
    }

    public void AppendRandomData()
    {
        var newData = new DefaultScrollItemData() { name = GetRandomSizeString(), longString = GetRandomLongText() };
        this.testData.Add(newData);
        this.UpdateAllScrollViewsIncrementally();
    }

    public void RemoveLastData()
    {
        if (this.testData.Count == 0)
        {
            return;
        }
        this.testData.RemoveAt(this.testData.Count - 1);
        this.UpdateAllScrollViewsIncrementally();
    }

    void UpdateAllScrollViews()
    {
        this.scrollView_1.UpdateData(false);
        this.scrollView_2.UpdateData(false);
        this.scrollView_3.UpdateData(false);
        this.scrollView_4.UpdateData(false);
        this.scrollView_5.UpdateData(false);
        this.scrollView_6.UpdateData(false);
    }

    void UpdateAllScrollViewsIncrementally()
    {
        this.scrollView_1.UpdateDataIncrementally(false);
        this.scrollView_2.UpdateDataIncrementally(false);
        this.scrollView_3.UpdateDataIncrementally(false);
        this.scrollView_4.UpdateDataIncrementally(false);
        this.scrollView_5.UpdateDataIncrementally(false);
        this.scrollView_6.UpdateDataIncrementally(false);
    }
}
