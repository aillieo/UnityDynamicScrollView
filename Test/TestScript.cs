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
        DefaultScrollItemData data = testData[index];
        item.gameObject.SetActive(true);
        item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", data.name, index);
    }

    void updateFunc_3(int index, RectTransform item)
    {
        DefaultScrollItemData data = testData[index];
        item.GetComponent<Text>().text = data.longString;
    }

    Vector2 itemSizeFunc_2(int index)
    {
        DefaultScrollItemData sd = testData[index];
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
        Vector2 size2 = itemSizeFunc_2(index);
        return new Vector2(size2.y, size2.x);
    }

    int itemCountFunc()
    {
        return testData.Count;
    }


    Vector2 itemSizeFunc_3(int index)
    {
        if (templateTextItemInstance == null)
        {
            templateTextItemInstance = GameObject.Instantiate(templateTextItem).GetComponent<RectTransform>();
            templateTextItemInstance.gameObject.SetActive(true);
            templateTextItemInstance.localScale = Vector3.zero;
        }
        templateTextItemInstance.GetComponent<Text>().text = testData[index].longString;
        LayoutRebuilder.ForceRebuildLayoutImmediate(templateTextItemInstance);
        float height = LayoutUtility.GetPreferredHeight(templateTextItemInstance);
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

        scrollView_1.SetUpdateFunc(updateFunc);
        scrollView_1.SetItemCountFunc(itemCountFunc);

        scrollView_2.SetUpdateFunc(updateFunc);
        scrollView_2.SetItemSizeFunc(itemSizeFunc_2);
        scrollView_2.SetItemCountFunc(itemCountFunc);

        scrollView_3.SetUpdateFunc(updateFunc_3);
        scrollView_3.SetItemSizeFunc(itemSizeFunc_3);
        scrollView_3.SetItemCountFunc(itemCountFunc);
        
        scrollView_4.SetUpdateFunc(updateFunc);
        scrollView_4.SetItemCountFunc(itemCountFunc);

        scrollView_5.SetUpdateFunc(updateFunc);
        scrollView_5.SetItemSizeFunc(itemSizeFunc_5);
        scrollView_5.SetItemCountFunc(itemCountFunc);

        scrollView_6.SetUpdateFunc(updateFunc_3);
        scrollView_6.SetItemSizeFunc(itemSizeFunc_3);
        scrollView_6.SetItemCountFunc(itemCountFunc);

        int dataCount = 0;
        do
        {
            AddRandomData();
        }
        while (++dataCount < 50);

        UpdateAllScrollViews();
    }

    static string GetRandomSizeString()
    {
        float f = UnityEngine.Random.value;
        if(f > 0.8)
        {
            return "XXL";
        }
        else if(f > 0.6)
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
        int rand = UnityEngine.Random.Range(1,100);
        StringBuilder stringBuilder = new StringBuilder(rand + 2);
        do {
            stringBuilder.Append((char)UnityEngine.Random.Range('A','Z'));
        }
        while (--rand > 0) ;
        stringBuilder.AppendLine();
        return stringBuilder.ToString();
    }

    public void AddRandomData()
    {
        DefaultScrollItemData newData = new DefaultScrollItemData() { name = GetRandomSizeString(), longString = GetRandomLongText()};
        testData.Insert(UnityEngine.Random.Range(0,testData.Count), newData);
        UpdateAllScrollViews();
    }

    public void RemoveRandomData()
    {
        if(testData.Count == 0)
        {
            return;
        }
        int index = UnityEngine.Random.Range(0, testData.Count);
        testData.RemoveAt(index);
        UpdateAllScrollViews();
    }

    public void AppendRandomData()
    {
        DefaultScrollItemData newData = new DefaultScrollItemData() { name = GetRandomSizeString(), longString = GetRandomLongText() };
        testData.Add(newData);
        UpdateAllScrollViewsIncrementally();
    }

    public void RemoveLastData()
    {
        if (testData.Count == 0)
        {
            return;
        }
        testData.RemoveAt(testData.Count - 1);
        UpdateAllScrollViewsIncrementally();
    }

    void UpdateAllScrollViews()
    {
        scrollView_1.UpdateData(false);
        scrollView_2.UpdateData(false);
        scrollView_3.UpdateData(false);
        scrollView_4.UpdateData(false);
        scrollView_5.UpdateData(false);
        scrollView_6.UpdateData(false);
    }

    void UpdateAllScrollViewsIncrementally()
    {
        scrollView_1.UpdateDataIncrementally(false);
        scrollView_2.UpdateDataIncrementally(false);
        scrollView_3.UpdateDataIncrementally(false);
        scrollView_4.UpdateDataIncrementally(false);
        scrollView_5.UpdateDataIncrementally(false);
        scrollView_6.UpdateDataIncrementally(false);
    }
}
