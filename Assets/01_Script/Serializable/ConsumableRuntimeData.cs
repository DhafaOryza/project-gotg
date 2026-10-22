using UnityEngine;

[System.Serializable]
public class ConsumableRuntimeData
{
    public ConsumableDataSO data;
    public int currentStack;

    public ConsumableRuntimeData(ConsumableDataSO dataSO, int startingQuantity = 1)
    {
        data = dataSO;
        int max = dataSO != null ? dataSO.maxStack : 99;
        currentStack = Mathf.Clamp(startingQuantity, 0, max);
    }
    public bool HasStock => currentStack > 0;

    public void ConsumeOne()
    {
        if (currentStack > 0)
            currentStack--;
    }

    public void AddQuantity(int amount)
    {
        int max = data != null ? data.maxStack : 99;
        currentStack = Mathf.Min(currentStack + amount, max);
    }
}