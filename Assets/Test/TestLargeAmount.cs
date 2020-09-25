using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AillieoUtils;
using UnityEngine.UI;
using SObject = System.Object;
using System.Text;
using System.Diagnostics;

public class TestLargeAmount : MonoBehaviour {

    List<DefaultScrollItemData> testData = new List<DefaultScrollItemData>();

    void updateFunc(int index, RectTransform item)
    {
        DefaultScrollItemData data = testData[index];
        item.gameObject.SetActive(true);
        item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", data.name, index);
    }

    Vector2 itemSizeFunc(int index)
    {
        // TimeConsumingFunc();
        // test =====================================================================================


        DefaultScrollItemData sd = testData[index];
        if (sd.name == "XXL")
            return new Vector2(300, 150);
        else if (sd.name == "XL")
            return new Vector2(300, 125);
        else if (sd.name == "L")
            return new Vector2(300, 100);
        else if (sd.name == "M")
            return new Vector2(300, 75);
        else // "S"
            return new Vector2(300, 50);
    }

    int itemCountFunc()
    {
        return testData.Count;
    }

    public ScrollViewEx scrollView;

    void Start () {

        scrollView.SetUpdateFunc(updateFunc);
        scrollView.SetItemSizeFunc(itemSizeFunc);
        scrollView.SetItemCountFunc(itemCountFunc);

        int dataCount = 0;
        do
        {
            AddRandomData();
        }
        while (++dataCount < 50000);

        scrollView.UpdateData(false);
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

    private void TimeConsumingFunc()
    {
        string s = string.Empty;
        int a = UnityEngine.Random.Range(0, 10000);
        int b = UnityEngine.Random.Range(0, 10000);

        for (int i = a; i < a + b; ++ i)
        {
            s += i;
        }
    }

    public void AddRandomData()
    {
        DefaultScrollItemData newData = new DefaultScrollItemData() { name = GetRandomSizeString()};
        testData.Insert(UnityEngine.Random.Range(0,testData.Count), newData);
        scrollView.UpdateData(false);
    }

    public void RemoveRandomData()
    {
        if(testData.Count == 0)
        {
            return;
        }
        int index = UnityEngine.Random.Range(0, testData.Count);
        testData.RemoveAt(index);
        scrollView.UpdateData(false);
    }

    public void ScrollToRandom()
    {
        int index = UnityEngine.Random.Range(0, testData.Count);

        // test =====================================================================================
        // always last
        index = testData.Count - 1;

        scrollView.ScrollTo(index);
        scrollView.UpdateData(false);
    }
}
