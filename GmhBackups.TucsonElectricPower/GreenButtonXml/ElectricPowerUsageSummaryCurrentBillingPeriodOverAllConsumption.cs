namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
public partial class ElectricPowerUsageSummaryCurrentBillingPeriodOverAllConsumption
{

    private byte powerOfTenMultiplierField;

    private uint timeStampField;

    private byte uomField;

    private uint valueField;

    /// <remarks/>
    public byte powerOfTenMultiplier
    {
        get
        {
            return this.powerOfTenMultiplierField;
        }
        set
        {
            this.powerOfTenMultiplierField = value;
        }
    }

    /// <remarks/>
    public uint timeStamp
    {
        get
        {
            return this.timeStampField;
        }
        set
        {
            this.timeStampField = value;
        }
    }

    /// <remarks/>
    public byte uom
    {
        get
        {
            return this.uomField;
        }
        set
        {
            this.uomField = value;
        }
    }

    /// <remarks/>
    public uint value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}