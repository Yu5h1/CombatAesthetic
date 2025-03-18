
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never), System.ComponentModel.Browsable(false)]
public static class EnergyInfoEx
{

    public static int Sum(this EnergyInfo[] energies, AttributeType type)
    {
        int sum = 0;
        foreach (var info in energies)
            if (info.attributeType.HasFlag(type))
                sum += (int)info.amount;
        return sum;
    }
}