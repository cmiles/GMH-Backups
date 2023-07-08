namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://naesb.org/espi", IsNullable = false)]
public partial class UsagePoint
{

    private UsagePointServiceCategory serviceCategoryField;

    /// <remarks/>
    public UsagePointServiceCategory ServiceCategory
    {
        get
        {
            return this.serviceCategoryField;
        }
        set
        {
            this.serviceCategoryField = value;
        }
    }
}