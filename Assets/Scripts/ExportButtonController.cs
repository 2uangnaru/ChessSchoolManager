using UnityEngine;

public class ExportButtonController : MonoBehaviour
{
    public void ExportCurrentPairing()
    {
        XlsxExportManager.ExportCurrentPairing();
    }

    public void ExportRanking()
    {
        XlsxExportManager.ExportRanking();
    }

}