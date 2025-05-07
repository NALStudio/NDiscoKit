namespace NDiscoKit.Pages;
public partial class LightsPage
{
    private bool philipsComponentCreate;

    public void PhilipsMenuOpenedChanged(bool opened)
    {
        if (opened)
            philipsComponentCreate = true;
    }
}
