using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AillieoUtils;
using UnityEngine.UI;
using System.Diagnostics;

public class TestLargeAmount : MonoBehaviour {
    List<DefaultScrollItemData> testData = new List<DefaultScrollItemData>();

    void updateFunc(int index, RectTransform item)
    {
        DefaultScrollItemData data = this.testData[index];
        item.gameObject.SetActive(true);
        item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", data.name, index);
    }

    Vector2 itemSizeFunc(int index)
    {
        this.TimeConsumingFunc();

        DefaultScrollItemData sd = this.testData[index];
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
        return this.testData.Count;
    }

    public ScrollView scrollView;
    public ScrollViewEx scrollViewEx;

    void Start () {
        this.scrollView.SetUpdateFunc(this.updateFunc);
        this.scrollView.SetItemSizeFunc(this.itemSizeFunc);
        this.scrollView.SetItemCountFunc(this.itemCountFunc);
        this.scrollViewEx.SetUpdateFunc(this.updateFunc);
        this.scrollViewEx.SetItemSizeFunc(this.itemSizeFunc);
        this.scrollViewEx.SetItemCountFunc(this.itemCountFunc);

        var dataCount = 0;
        do
        {
            var newData = new DefaultScrollItemData() { name = GetRandomSizeString() };
            this.testData.Add(newData);
        }
        while (++dataCount < 50000);

        this.scrollView.UpdateData(false);
        this.scrollViewEx.UpdateData(false);
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

    private void TimeConsumingFunc()
    {
        var s = string.Empty;
        var a = UnityEngine.Random.Range(0, 100);
        var b = UnityEngine.Random.Range(0, 100);

        for (var i = a; i < a + b; ++ i)
        {
            s += i;
        }
    }

    public void AddRandomData()
    {
        var newData = new DefaultScrollItemData() { name = GetRandomSizeString()};
        this.testData.Insert(UnityEngine.Random.Range(0,this.testData.Count), newData);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        this.scrollView.UpdateData(true);
        stopwatch.Stop();
        var time1 = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        stopwatch.Start();
        this.scrollViewEx.UpdateData(true);
        stopwatch.Stop();
        var time2 = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"cost time in ms:     ScrollView:{time1}     ScrollViewEx:{time2}");
    }

    public void RemoveRandomData()
    {
        if (this.testData.Count == 0)
        {
            return;
        }
        var index = UnityEngine.Random.Range(0, this.testData.Count);
        this.testData.RemoveAt(index);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        this.scrollView.UpdateData(true);
        stopwatch.Stop();
        var time1 = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        stopwatch.Start();
        this.scrollViewEx.UpdateData(true);
        stopwatch.Stop();
        var time2 = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"cost time in ms:     ScrollView:{time1}     ScrollViewEx:{time2}");
    }

    public void ScrollToRandom()
    {
        var index = UnityEngine.Random.Range(0, this.testData.Count);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        this.scrollView.ScrollTo(index);
        stopwatch.Stop();
        var time1 = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        stopwatch.Start();
        this.scrollViewEx.ScrollTo(index);
        stopwatch.Stop();
        var time2 = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"cost time in ms:     ScrollView:{time1}     ScrollViewEx:{time2}");
    }
}
