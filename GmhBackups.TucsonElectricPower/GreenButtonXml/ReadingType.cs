namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://naesb.org/espi", IsNullable = false)]
public partial class ReadingType
{

    private byte accumulationBehaviourField;

    private byte commodityField;

    private byte dataQualifierField;

    private byte flowDirectionField;

    private ushort intervalLengthField;

    private byte kindField;

    private ushort phaseField;

    private byte powerOfTenMultiplierField;

    private byte timeAttributeField;

    private byte uomField;

    /// <remarks/>
    public byte accumulationBehaviour
    {
        get
        {
            return this.accumulationBehaviourField;
        }
        set
        {
            this.accumulationBehaviourField = value;
        }
    }

    /// <remarks/>
    public byte commodity
    {
        get
        {
            return this.commodityField;
        }
        set
        {
            this.commodityField = value;
        }
    }

    /// <remarks/>
    public byte dataQualifier
    {
        get
        {
            return this.dataQualifierField;
        }
        set
        {
            this.dataQualifierField = value;
        }
    }

    /// <remarks/>
    public byte flowDirection
    {
        get
        {
            return this.flowDirectionField;
        }
        set
        {
            this.flowDirectionField = value;
        }
    }

    /// <remarks/>
    public ushort intervalLength
    {
        get
        {
            return this.intervalLengthField;
        }
        set
        {
            this.intervalLengthField = value;
        }
    }

    /// <remarks/>
    public byte kind
    {
        get
        {
            return this.kindField;
        }
        set
        {
            this.kindField = value;
        }
    }

    /// <remarks/>
    public ushort phase
    {
        get
        {
            return this.phaseField;
        }
        set
        {
            this.phaseField = value;
        }
    }

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
    public byte timeAttribute
    {
        get
        {
            return this.timeAttributeField;
        }
        set
        {
            this.timeAttributeField = value;
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
}